using MeterReaderApp.Interfaces;
using MeterReaderApp.Services;

namespace MeterReaderApp.ViewModels
{
    [QueryProperty("Area", "Area")]
    [QueryProperty("Refresh", "Refresh")]
    public partial class ReadingViewModel : BaseViewModel
    {
        private readonly ReadingService _readingService;
        private readonly ReadingExportService _exportService;
        private readonly CustomerService _customerService;
        private readonly MonthService _monthService;
        private readonly DbContext _db;
        private readonly AppShell _appShell;

        public ReadingViewModel(
            ReadingService readingService,
            ReadingExportService readingExportService,
            CustomerService customerService,
            MonthService monthService,
            DbContext dbContext,
            AppShell appShell)
        {
            _readingService = readingService;
            _exportService = readingExportService;
            _customerService = customerService;
            _monthService = monthService;
            _db = dbContext;
            _appShell = appShell;
        }

        [ObservableProperty] private bool isRefreshing;
        [ObservableProperty] private string area;
        [ObservableProperty] private string areaTitle;
        [ObservableProperty] private int capturedReadingsCount;
        [ObservableProperty] private int uncapturedReadingsCount;
        [ObservableProperty] private int zeroReadingsCount;
        [ObservableProperty] private int abnormalCount;
        [ObservableProperty] private string uncapturedTitle;
        [ObservableProperty] private string capturedTitle;

        [ObservableProperty]
        private ObservableCollection<Reading> allReadings = new();

        [ObservableProperty]
        private ObservableCollection<Reading> areaReadings = new();

        [ObservableProperty]
        private ObservableCollection<LocationReadings> allLocation = new();

        public ObservableCollection<Reading> exceptionReadings { get; } = new();

        public static List<Reading> ReadingsListForSearch { get; private set; } = new();
        public static List<Reading> AreaReadingsListForSearch { get; private set; } = new();
        public static List<LocationReadings> LocationListForSearch { get; private set; } = new();

        // ── Captured Readings ──────────────────────────────────────────
        [RelayCommand]
        private async Task GetCapturedReadings()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var list = await _readingService.GetAllCapturedReadings();

                if (list is not { Count: > 0 })
                {
                    AllReadings = new ObservableCollection<Reading>();
                    return;
                }

                // Prepare data off-UI — no collection notifications yet
                foreach (var r in list)
                {
                    r.IsFlagged = IsReadingFlagged((decimal)r.PREVIOUS_READING, r.CURRENT_READING);
                    r.ReadingTaken = r.CURRENT_READING >= 1;
                    r.ReadingNotTaken = !r.ReadingTaken;
                }

                // Single swap — one PropertyChanged, CollectionView rebuilds once
                AllReadings = new ObservableCollection<Reading>(list);

                ReadingsListForSearch.Clear();
                ReadingsListForSearch.AddRange(list);

                CapturedReadingsCount = list.Count;
                AbnormalCount = list.Count(r => r.CURRENT_READING - r.PREVIOUS_READING > 20);
                ZeroReadingsCount = list.Count(r => r.CURRENT_READING == r.PREVIOUS_READING);
                CapturedTitle = $"Captured: {CapturedReadingsCount}  Zero: {ZeroReadingsCount}  Abnormal: {AbnormalCount}";
            }
            catch (Exception ex)
            {
                await AlertService.ShowError("Unable to retrieve readings");
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = IsRefreshing = false;
            }
        }

        // ── Uncaptured Readings ────────────────────────────────────────
        [RelayCommand]
        private async Task GetUncapturedReadings()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var list = await _readingService.GetAllUncapturedReadings();

                if (list is not { Count: > 0 })
                {
                    AllReadings = new ObservableCollection<Reading>();
                    return;
                }

                foreach (var r in list)
                {
                    r.ReadingTaken = r.CURRENT_READING >= 1;
                    r.ReadingNotTaken = !r.ReadingTaken;
                }

                AllReadings = new ObservableCollection<Reading>(list);

                ReadingsListForSearch.Clear();
                ReadingsListForSearch.AddRange(list);

                int uncaptured = list.Count;
                int zeros = list.Count(r => r.CURRENT_READING == r.PREVIOUS_READING);
                UncapturedTitle = $"Uncaptured: {uncaptured}  Meters not in use: {zeros}";
            }
            catch (Exception ex)
            {
                await AlertService.ShowError("Unable to retrieve readings");
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = IsRefreshing = false;
            }
        }

        // ── Navigation ─────────────────────────────────────────────────
        [RelayCommand]
        public async Task GoToCustomerDetails(Reading reading)
        {
            if (reading?.CUSTOMER_NUMBER is null) return;

            var customer = await _customerService.GetCustomerDetails(reading.CUSTOMER_NUMBER);
            if (customer is null)
            {
                await AlertService.ShowError("Failed to get customer details");
                return;
            }
            await Shell.Current.GoToAsync(nameof(CustomerDetailPage), true,
                new Dictionary<string, object> { { nameof(Customer), new CustomerWrapper(customer) } });
        }

        // ── Locations ──────────────────────────────────────────────────
        [RelayCommand]
        private async Task GetLocations()
        {
            try
            {
                IsBusy = true;

                var listOfLocations = await _readingService.GetListOfLocations();
                if (listOfLocations != null && listOfLocations.Count > 0)
                {
                    foreach (var location in listOfLocations)
                    {
                        location.IsAllNotCaptured = !(bool)location.IsAllCaptured;
                    }

                    AllLocation = new ObservableCollection<LocationReadings>(listOfLocations);

                    LocationListForSearch.Clear();
                    LocationListForSearch.AddRange(listOfLocations);
                }
                else
                {
                    AllLocation = new ObservableCollection<LocationReadings>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetLocations error: {ex}");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToListOfUncapturedReadingsByArea(LocationReadings area)
        {
            if (area == null) return;

            try
            {
                IsBusy = true;

                var uncapturedReadings = await _readingService.GetUncapturedReadingsByArea(area);

                if (uncapturedReadings == null || uncapturedReadings.Count == 0)
                {
                    await Shell.Current.DisplayAlert("No Readings", "No records found here.", "OK");
                    return;
                }

                AreaTitle = area.AREANAME;

                foreach (var i in uncapturedReadings)
                {
                    i.ReadingTaken = false;
                    i.ReadingNotTaken = true;
                }

                AreaReadings = new ObservableCollection<Reading>(uncapturedReadings);

                await Shell.Current.GoToAsync(
                    nameof(UncapturedReadingsByAreaPage), true,
                    new Dictionary<string, object>
                    {
                        { "Readings", new List<Reading>(uncapturedReadings) }
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GoToListOfUncapturedReadingsByArea error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void SetReadings(List<Reading> readings)
        {
            AreaReadings = new ObservableCollection<Reading>(readings);

            AreaReadingsListForSearch.Clear();
            AreaReadingsListForSearch.AddRange(readings);
        }
        // ── Exception / Abnormal Readings ──────────────────────────────
        [RelayCommand]
        public async Task GoToExceptionList()
        {
            string email = Preferences.Default.Get("username", "Unknown");
            string reader = email.Split('@')[0];

            try
            {
                var all = await _db.Database.Table<Reading>()
                                            .Where(r => r.CURRENT_READING > 0)
                                            .ToListAsync();

                exceptionReadings.Clear();
                foreach (var r in all.Where(r => r.CURRENT_READING - r.PREVIOUS_READING >= 20))
                {
                    exceptionReadings.Add(new Reading
                    {
                        CUSTOMER_NAME = string.Join(" ", r.CUSTOMER_NAME.Split().Take(2)),
                        ERF_NUMBER = r.ERF_NUMBER,
                        METER_NUMBER = r.METER_NUMBER,
                        CURRENT_READING = r.CURRENT_READING,
                        PercentageChange = (int?)(r.CURRENT_READING - r.PREVIOUS_READING),
                        ReadingDate = r.ReadingDate,
                        METER_READER = string.IsNullOrEmpty(r.METER_READER) ? reader : r.METER_READER
                    });
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.ToString();
            }
            finally
            {
                IsBusy = IsRefreshing = false;
            }
        }

        // ── Sync / Export ──────────────────────────────────────────────
        [RelayCommand]
        public async Task ScanForNewExport()
        {
            IsBusy = true;
            await _exportService.ScanForNewItems();
            IsBusy = false;
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task ScanForNewlyAddedCustomerReadings()
        {
            IsBusy = true;
            await _readingService.ScanNewCustomersReadingsFromSql();
            IsBusy = false;
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task ReflushData()
        {
            IsBusy = true;
            await _exportService.FlushAndSeed();
            IsBusy = false;
            await Shell.Current.GoToAsync("..");
        }

        // ── Reset / Admin ──────────────────────────────────────────────
        [RelayCommand]
        public async Task ResetReading()
        {
            var response = await Shell.Current.DisplayPromptAsync(
                "Delete, Reset or Sync", "",
                "Confirm", "Cancel",
                "Enter your command here...",
                keyboard: Keyboard.Text);

            if (response is null) return;

            IsBusy = true;
            await Task.Delay(5000);

            if (response == "%sync%")
            {
                var rows = await _db.Database.Table<Reading>().ToListAsync();
                foreach (var r in rows)
                {
                    r.ReadingSync = true; r.AreaUpdated = true;
                    r.ReadingNotTaken = false; r.ReadingTaken = true;
                }
                await _db.Database.UpdateAllAsync(rows);
                await AlertService.ShowSuccess("SYNC Completed!");
            }
            else if (response == "%reset%")
            {
                var rows = await _db.Database.Table<Reading>().ToListAsync();
                foreach (var r in rows) { r.ReadingSync = false; r.ReadingTaken = true; }
                await _db.Database.UpdateAllAsync(rows);
                await AlertService.ShowSuccess("RESET Completed!");
            }
            else if (response == "%delete%")
            {
                await _db.Database.Table<ReadingExport>().DeleteAsync(r => r.WaterReadingExportID > 0);
                await _db.Database.Table<Reading>().DeleteAsync(r => r.Id > 0);
                await _db.Database.Table<Customer>().DeleteAsync(r => r.CUSTNMBR != null);
                await _db.Database.Table<ReadingMedia>().DeleteAsync(r => r.Id > 0);
                await AlertService.ShowSuccess("DELETE Completed!");
            }
            else
            {
                DisplayToast("Command not recognized");
            }

            IsBusy = false;
        }

        // ── Helpers ────────────────────────────────────────────────────
        public static bool IsReadingFlagged(decimal previous, decimal current) =>
            Math.Abs(current - previous) >= 20;
    }
}

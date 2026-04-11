using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Graphics;
﻿namespace SampleMauiMvvmApp.ViewModels
{
    public partial class MonthViewModel : BaseViewModel
    {
        public ObservableCollection<Month> Months { get; } = new();
        public ObservableCollection<Reading> listReadings { get; set; } = new ObservableCollection<Reading> { };
        public ObservableCollection<Customer> customer { get; set; } = new();

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string myTitle;

        [ObservableProperty]
        public static int cMonth;

        [ObservableProperty]
        public static string sMonth;

        [ObservableProperty]
        private string syncTime;

        [ObservableProperty]
        public static decimal lastReadingByCustId;

        private string message = string.Empty;

        [ObservableProperty]
        private int currentYear;

        private MonthService monthService;
        private CustomerService customerService;
        private ReadingService readingService;
        private ReadingExportService readingExportService;
        private IConnectivity connectivity;

        public MonthViewModel(
            MonthService _monthService,
            ReadingExportService _readingExportService,
            IConnectivity _connectivity,
            ReadingService _readingService,
            CustomerService _customerService
            )
        {
            Title = "Readings By Month";
            connectivity = _connectivity;
            monthService = _monthService;
            readingService = _readingService;
            readingExportService = _readingExportService;
            customerService = _customerService;
        }

        [RelayCommand]
        public async Task<decimal> GetLastReadingByCustomerId(string customer)
        {
            var LastReadingByCustomerId = await readingService.GetLastReadingByIdAsync(customer);
            LastReadingByCustId = (decimal)LastReadingByCustomerId.CURRENT_READING;
            return LastReadingByCustId;
        }

        [RelayCommand]
        private async Task GetMonthsAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                await Task.Delay(500);
                var months = await monthService.GetListOfMonthsFromSqlite();
                if (Months.Count != 0)
                    Months.Clear();
                if (months == null)
                {
                    await AlertService.ShowError("Failed to fetch data");
                    return;
                }

                foreach (var month in months)
                {
                    month.IsActive = await monthService.IsMonthPopulated(month);
                    Months.Add(month);
                }
                ;
                if (Months.Count == 0)
                {
                    await AlertService.ShowError("Failed to fetch data");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get months: {ex.Message}");
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToListOfReadingsByMonth(Month monthId)
        {
            if (monthId.MonthID <= 0) return;
            var readings = await readingService.GetReadingsByMonthId(monthId.MonthID);
            foreach (var item in readings)
            {
                if (item.ReadingTaken == false)
                {
                    item.ReadingTaken = true;
                }
                ;
            }
            if (readings.Count == 0)
            {
                await Shell.Current.DisplayAlert("No Readings", $"No records found in {monthId.MonthName}", "OK");
                return;
            }
            CMonth = monthId.MonthID;
            SMonth = $"Successfully Synced for : {monthId.MonthName} ";
            SyncTime = System.DateTime.UtcNow.Hour + $":{System.DateTime.UtcNow.Minute}";
            listReadings.Clear();
            foreach (var reading in readings)
            {
                listReadings.Add(reading);
            }
            //listReadings.AddRange(readings);
            MyTitle = $"{monthId.MonthName}";
            await Shell.Current.GoToAsync(nameof(ListOfReadingByMonthPage), true, new Dictionary<string, object>
                            {
                                {"Month", monthId }
                            });
        }

        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task SyncByMonthIdAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            int syncedReadings = 0;
            int syncedImages   = 0;

            try
            {
                syncedReadings = await readingService.SyncReadingsByMonthIdAsync(CMonth);
                // Count images synced (exposed as static by ReadingService)
                syncedImages = SampleMauiMvvmApp.Services.ReadingService.allImageItemsByCount;

                message = readingService.StatusMessage;

                if (syncedReadings > 0)
                {
                    // ── Rich in-app notification ───────────────────────────
                    string notifMsg = syncedImages > 0
                        ? $"✅ {syncedReadings} reading(s) and {syncedImages} photo(s) synced successfully!"
                        : $"✅ {syncedReadings} reading(s) synced successfully!";

                    var snackbar = Snackbar.Make(
                        notifMsg,
                        action: null,
                        actionButtonText: "OK",
                        duration: TimeSpan.FromSeconds(5),
                        visualOptions: new SnackbarOptions
                        {
                            BackgroundColor = Color.FromArgb("#1B6E2D"),
                            TextColor       = Colors.White,
                            ActionButtonTextColor = Colors.White,
                            CornerRadius    = new CornerRadius(12),
                            Font            = Microsoft.Maui.Font.SystemFontOfSize(14),
                            CharacterSpacing = 0.3,
                        });
                    await snackbar.Show();

                    // Notify user data will refresh
                    await Shell.Current.DisplayAlert(
                        "Sync Complete",
                        $"{syncedReadings} reading(s) uploaded.Your data will now be refreshed.",
                        "OK");

                    await readingExportService.FlushAndSeed();
                }
                else if (syncedReadings == 0)
                {
                    // No readings needed syncing — show info snackbar
                    var snackbar = Snackbar.Make(
                        "No new readings to sync at this time.",
                        action: null,
                        actionButtonText: "OK",
                        duration: TimeSpan.FromSeconds(4),
                        visualOptions: new SnackbarOptions
                        {
                            BackgroundColor = Color.FromArgb("#1A7DB5"),
                            TextColor       = Colors.White,
                            ActionButtonTextColor = Colors.White,
                            CornerRadius    = new CornerRadius(12),
                            Font            = Microsoft.Maui.Font.SystemFontOfSize(14),
                        });
                    await snackbar.Show();
                }
            }
            catch (Exception ex)
            {
                // ── Failure notification ───────────────────────────────
                string failMsg = $"❌ Sync failed: {ex.Message}";
                var errSnackbar = Snackbar.Make(
                    failMsg,
                    action: null,
                    actionButtonText: "Dismiss",
                    duration: TimeSpan.FromSeconds(6),
                    visualOptions: new SnackbarOptions
                    {
                        BackgroundColor = Color.FromArgb("#B91C1C"),
                        TextColor       = Colors.White,
                        ActionButtonTextColor = Colors.White,
                        CornerRadius    = new CornerRadius(12),
                        Font            = Microsoft.Maui.Font.SystemFontOfSize(14),
                    });
                await errSnackbar.Show();
                Debug.WriteLine($"Sync error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowAlert(string message)
        {
            await Shell.Current.DisplayAlert("Info", message, "Ok");
        }
    }
}
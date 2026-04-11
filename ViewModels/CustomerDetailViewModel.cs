using Ardalis.GuardClauses;
using Microsoft.Maui.Networking;
using System.Text.RegularExpressions;

namespace MeterReaderApp.ViewModels
{
    [QueryProperty("Customer", "Customer")]
    [QueryProperty("Reading", "Reading")]
    public partial class CustomerDetailViewModel : BaseViewModel
    {
        private DbContext dbContext;
        private ReadingService readingService;
        private MonthService monthService;
        private CustomerService customerService;

        [ObservableProperty]
        private CustomerWrapper customer;

        [ObservableProperty]
        private ReadingWrapper reading;

        [ObservableProperty]
        private string erfNumber;

        [ObservableProperty]
        private string custStateErf;

        [ObservableProperty]
        private long custphone1;

        [ObservableProperty]
        private decimal custPrevReading;

        [ObservableProperty]
        private decimal custCurrentReading;

        [ObservableProperty]
        private string totalUsage;

        [ObservableProperty]
        private string meterNumber;

        [ObservableProperty]
        private decimal latitude;

        [ObservableProperty]
        private decimal longitude;

        [ObservableProperty]
        private string routeNumber;

        [ObservableProperty]
        private ReadingWrapper vmReading;

        //[ObservableProperty]
        //string? currentMonth;
        [ObservableProperty]
        public static bool isExist;

        [ObservableProperty]
        private bool isUpdate;

        [ObservableProperty]
        private bool isCurrentReading;

        private int selectedCompressionQuality = 25;
        private IGeolocation geolocation;

        private ReadingMonitorService _monitor;
        private readonly BillingLocationService _billingLocationService;

        public CustomerDetailViewModel(DbContext _dbContext, ReadingService readingService,
            CustomerService _customerService, MonthService _monthService,
            IGeolocation geolocation, ReadingMonitorService monitor,
            BillingLocationService billingLocationService)
        {
            Title = "Customer Detail Page";
            this.dbContext = _dbContext;
            this.readingService = readingService;
            this.customerService = _customerService;
            this.monthService = _monthService;
            this.geolocation = geolocation;
            this._monitor = monitor;
            this._billingLocationService = billingLocationService;

            //WeakReferenceMessenger.Default.Register<ReadingCreateMessage>(this, (obj, handler) =>
            //{
            //    MainThread.BeginInvokeOnMainThread(() =>
            //    {
            //        var newReading = new ReadingWrapper(handler.Value)
            //        {
            //            IsNew = true
            //        };

            //        if (Customer.Readings == null) Customer.Readings = new();
            //        Customer.Readings.Insert(0, newReading);

            //    });
            //});
        }

        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("../..");
        }

        [RelayCommand]
        private async Task CustDisplayDetailsAsync()
        {
            var reading = await readingService.GetLastReadingByIdAsync(Customer.Custnmbr);
            if (reading != null)
            {
                CustPrevReading = (decimal)reading.PREVIOUS_READING;
                CustCurrentReading = (decimal)reading.CURRENT_READING;
                MeterNumber = reading.METER_NUMBER;
                RouteNumber = reading.RouteNumber;
                Custphone1 = (long)reading.PHONE1;
                erfNumber = reading.ERF_NUMBER;
                Longitude  = (decimal)(reading.Longitude ?? 0);
                Latitude = (decimal)(reading.Latitude ?? 0);
                TotalUsage = $"{((decimal?)reading.CURRENT_READING >= (decimal?)reading.PREVIOUS_READING ? (decimal?)reading.CURRENT_READING - (decimal?)reading.PREVIOUS_READING : 0)}";
                bool isCurrentReading = IsCurrentReadingCaptured(reading.CURRENT_READING);

                //CurrentMonth =  monthService?.GetCurrentMonthNameById(reading.MonthID).GetAwaiter().GetResult();
                if (isCurrentReading)
                {
                    IsCurrentReading = true;
                }
                else
                {
                    IsCurrentReading = false;
                }

                //CustStateErf = $"{reading.AREA.Trim()} - (ERF {reading.ERF_NUMBER.Replace("ERF","").Trim()})" ?? "NO ERF";
                bool result = IsUpdateMode(CustCurrentReading);
                if (result)
                {
                    IsUpdate = true;
                }
                else
                {
                    IsUpdate = false;
                }

                if (string.IsNullOrEmpty(reading.AREA) || !Regex.IsMatch(reading.ERF_NUMBER, @"\d") || reading.AREA is null)
                {
                    CustStateErf = $"{reading?.AREA?.Trim()} - NO ERF";
                }
                else
                {
                    CustStateErf = $"{reading.AREA.Trim()} - (ERF {reading.ERF_NUMBER.Replace("ERF", "").Trim()})";
                }

                Title = $"{reading.CUSTOMER_NAME.Trim()}";
            }

            bool isExist = await readingService.IsReadingExistForMonthId(Customer.Custnmbr);
            IsExist = isExist;
            return;
        }

        [RelayCommand]
        public async Task CreateReadingAsync()
        {
            try
            {
                IsValid();
                var CurrentMonthReading = await readingService.GetCurrentMonthReadingByCustIdAsync(Customer.Custnmbr);
                var customerInfo = await customerService.GetCustomerDetails(Customer.Custnmbr);
                //var loggedInUser = await dbContext.Database.Table<LoginHistory>()?.OrderByDescending(r => r.LoginId).FirstAsync();

                if (VmReading.C_reading != null && VmReading.C_reading.Any())
                {
                    if (int.TryParse(VmReading.C_reading, out int intValue))
                    {
                        CurrentMonthReading.CURRENT_READING = intValue;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert($"Error",
                                                        $"Something went wrong while converting reading to int", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert($"Null Or Empty",
                                                        $"Please enter a valid reading!", "OK");
                    return;
                }

                if (CurrentMonthReading.WaterReadingExportID <= 0)
                {
                    await Shell.Current.DisplayAlert($"No Reading Export Found",
                                                         $"Confirm Database and try again!", "OK");
                    await ClearForm();
                    return;
                }

                if (CurrentMonthReading.CURRENT_READING < CustPrevReading && CurrentMonthReading.CURRENT_READING >= 0)
                {
                    await Shell.Current.DisplayAlert($"Current Reading lesser than Previous of:{CustPrevReading}",
                                                          $"Please check current reading and try again!", "OK");

                    await Shell.Current.DisplayAlert($"Invalid Input!",
                                                          $"Please enter valid reading!", "OK");
                    await ClearForm();
                    return;
                }

                if (CurrentMonthReading.CURRENT_READING == 0)
                {
                    var myAction = await Shell.Current.DisplayAlert($"Zero(0) readings entered",
                                                           $"Are you sure you want to enter this reading?", "Cancel", "Yes");

                    if (myAction)
                    {
                        await ClearForm();
                        return;
                    }
                }

                if (CurrentMonthReading.Latitude == null || CurrentMonthReading.Longitude == null ||
                    CurrentMonthReading.Latitude == 0 || CurrentMonthReading.Longitude == 0)
                {
                    var locationCoordinate = await GetCustomerLocationCoordinatesAsync();
                    CurrentMonthReading.Longitude = locationCoordinate.Longitude;
                    CurrentMonthReading.Latitude = locationCoordinate.Latitude;
                }

                CurrentMonthReading.Comment = VmReading.Comment;
                CurrentMonthReading.ReadingTaken = true;
                CurrentMonthReading.ReadingNotTaken = false;
                CurrentMonthReading.ReadingSync = false;
                CurrentMonthReading.WaterReadingExportID = (int)await readingService.GetLatestExportItemId();

                Reading newReading = new Models.Reading();

                if (string.IsNullOrEmpty(CurrentMonthReading.AREA) ||
                                string.IsNullOrWhiteSpace(CurrentMonthReading.AREA.Trim()) ||
                                CurrentMonthReading.AREA.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(CurrentMonthReading.METER_NUMBER))
                    {
                        CurrentMonthReading.METER_NUMBER = CurrentMonthReading.METER_NUMBER;
                    }
                    else
                    {
                        var MeterNo = await UpdateCustomerMeterNo();
                        CurrentMonthReading.METER_NUMBER = MeterNo;
                        meterNumber = MeterNo.ToString();
                    }
                    CurrentMonthReading.AREA = await AddNewCustomerLocation(Customer.Custnmbr);
                    await readingService.InsertReading(CurrentMonthReading);
                    await GoBackAsync();
                }
                else
                {
                    newReading = await readingService.InsertReading(Models.Reading.GenerateNewFromWrapper(new ReadingWrapper(CurrentMonthReading)));
                }

                IsExist = true;

                if (newReading != null)
                {
                    var latestMonthName = await monthService.GetMonthNameById();
                    if (IsUpdate)
                    {
                        await Shell.Current.DisplayAlert($"Success!", $"A reading for {CurrentMonthReading.CUSTOMER_NAME.Substring(0, 15).Trim()}... Updated!", "OK");
                        CustCurrentReading = custCurrentReading;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert($"Success!", $"A reading for {CurrentMonthReading.CUSTOMER_NAME.Substring(0, 15).Trim() ?? $"customer"} Created!", "OK");
                    }

                    // Propagate the new reading to the main reading page.
                    WeakReferenceMessenger.Default.Send(new ReadingCreateMessage(newReading));
                    _ = _monitor.CheckAndNotifyAsync();
                    await Task.Delay(1000);
                    await GoBackAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert($"Error!",
                                   $"Something Wrong,Try Again!", "OK");
                    await ClearForm();
                    await Task.Delay(500);
                    await GoBackAsync();
                }
            }
            catch(Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// Returns location names from the local SQLite BillingLocation table (synced from API).
        /// Falls back to a minimal hardcoded list only when the table is empty
        /// so the app still works offline on first launch before any sync.
        /// </summary>
        private async Task<string[]> GetAvailableLocationsAsync()
        {
            var fromDb = await _billingLocationService.GetLocationNamesAsync();
            if (fromDb.Length > 0) return fromDb;

            // Fallback — only reached when database has not yet been seeded
            return new[]
            {
                "Omaruru Town - Extension 1",
                "Omaruru Town - Extension 2",
                "Omaruru Town - Extension 3",
                "Omaruru Town - Extension 4",
                "Omaruru Town - Extension 5",
                "Ozondje Town - Welwitchia",
                "Ozondje Town - Herero Location",
                "Ozondje Town - Damara Location",
                "Ozondje Town - Sonskyn",
                "Ozondje Town - Hakahana",
                "Ozondje Town - Vambo Location",
                "Erongo Park",
                "Wildlife Estates",
                "Unclassified"
            };
        }


        [RelayCommand]
        public async Task TakePhotoClicked()
        {
            // MAUI MediaPicker handles permissions internally on Android & iOS.
            // Do NOT add a manual Permissions.RequestAsync before this call —
            // it causes a race condition on Android that crashes the camera intent.
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlert("Not Supported",
                    "Camera is not available on this device.", "OK");
                return;
            }

            FileResult photo;
            try
            {
                photo = await MediaPicker.Default.CapturePhotoAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Camera error: {ex}");
                // User cancelled or permission denied — fail silently
                return;
            }

            if (photo is null) return;

            // ── Convert to base64 ─────────────────────────────────────
            string base64Image;
            using (var stream = await photo.OpenReadAsync())
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                base64Image = Convert.ToBase64String(ms.ToArray());
            }

            // ── Look up the current reading record ────────────────────
            var latestExport = await dbContext.Database.Table<ReadingExport>()
                .OrderByDescending(r => r.WaterReadingExportID)
                .FirstOrDefaultAsync();
            if (latestExport is null) return;

            int exportId = latestExport.WaterReadingExportID;

            var reading = await dbContext.Database.Table<Reading>()
                .Where(r => r.CUSTOMER_NUMBER == Customer.Custnmbr
                         && r.WaterReadingExportID == exportId)
                .FirstOrDefaultAsync();
            if (reading is null) return;

            // ── Remove any existing photo for this reading ────────────
            var existing = await dbContext.Database.Table<ReadingMedia>()
                .Where(r => r.WaterReadingExportDataId == reading.WaterReadingExportDataID)
                .ToListAsync();
            foreach (var img in existing)
                await dbContext.Database.DeleteAsync(img);

            // ── Save new photo ────────────────────────────────────────
            var captured = new ReadingMedia
            {
                WaterReadingExportDataId = reading.WaterReadingExportDataID,
                WaterReadingExportId     = (int)reading.WaterReadingExportID,
                Title                    = photo.FileName,
                MeterImage               = base64Image,
                DateTaken                = DateTime.UtcNow.ToLongDateString(),
            };

            int saved = await dbContext.Database.InsertAsync(captured);
            if (saved == 1)
                await Toast.Make("📷 Photo saved", CommunityToolkit.Maui.Core.ToastDuration.Short, 10).Show();
        }

        #region Get Current Location

        public async void GetLocation()

        {
            var location = await geolocation.GetLastKnownLocationAsync();
            if (location == null)

            {
                location = await geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                    ,
                    RequestFullAccuracy = true,
                });
                await Shell.Current.DisplayAlert($"Current Location!", $"Longitude is {location.Longitude} , Latitude {location.Latitude}", "OK");
            }
            return;
        }

        #endregion Get Current Location

        public bool IsValid()
        {
            try
            {
                Guard.Against.OutOfRange<Decimal>((decimal)VmReading.Current_reading, nameof(VmReading.Current_reading), 0, Decimal.MaxValue);
                Guard.Against.OutOfRange<Decimal>((decimal)VmReading.Current_reading, nameof(VmReading.Current_reading), 100000, Decimal.MinValue);
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                return false;
            }

            return true;
        }

        public bool IsCurrentReadingCaptured(decimal? Currreading)
        {
            if ((decimal?)Currreading > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsUpdateMode(decimal existingReading)
        {
            if (existingReading > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [RelayCommand]
        private async Task ClearForm()
        {
            await Task.Yield();
            VmReading.C_reading = string.Empty;
        }

        public async Task<string> AddNewCustomerLocation(string customerNo)
        {
            var cstObj = await dbContext.Database.Table<Reading>()
                            .Where(r => r.CUSTOMER_NUMBER == customerNo)
                            .FirstOrDefaultAsync();

            bool hasLocation = false;

            if (cstObj != null)
            {
                if (cstObj.AREA != null)
                {
                    cstObj.AREA = cstObj.AREA.Trim();
                }

                hasLocation = !(string.IsNullOrEmpty(cstObj.AREA) ||
                                string.IsNullOrWhiteSpace(cstObj.AREA) ||
                                cstObj.AREA.Equals("NULL", StringComparison.OrdinalIgnoreCase));
            }

            while (!hasLocation)
            {
                var locations = await GetAvailableLocationsAsync();
                var userLocation = await Shell.Current.DisplayActionSheet("Select Location", "Cancel", null, locations);

                if (string.IsNullOrEmpty(userLocation) || userLocation == "Cancel")
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(userLocation) &&
                    !string.IsNullOrWhiteSpace(userLocation) &&
                    !userLocation.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                {
                    var newReading = await readingService.GetCurrentMonthReadingByCustIdAsync(cstObj.CUSTOMER_NUMBER);
                    newReading.AREA = userLocation.Trim();

                    var custNewArea = await readingService.UpsertArea(newReading);
                    hasLocation = true;
                    return custNewArea;
                }
            }

            return "";
        }

        [RelayCommand]
        public async Task<string> UpdateCustomerMeterNo()
        {
            try
            {
                var cstObj = await dbContext.Database.Table<Reading>()
                              .Where(r => r.CUSTOMER_NUMBER == Customer.Custnmbr)
                              .FirstOrDefaultAsync();

                var userMenterNo = await Shell.Current.DisplayPromptAsync(
                    "Update Meter Number",
                    "Edit the meter number below",
                    "Save",
                    "Cancel",
                    placeholder: "Enter meter number here...",
                    initialValue: cstObj?.METER_NUMBER ?? "",
                    keyboard: Keyboard.Text);
                if (!string.IsNullOrWhiteSpace(userMenterNo))
                {
                    cstObj.METER_NUMBER = userMenterNo;
                    var custNewMeter = await readingService.UpsertMeter(cstObj);

                    if (!string.IsNullOrEmpty(custNewMeter))
                    {
                        await Shell.Current.DisplayAlert("Success!", "Meter Updated!", "OK");
                        MeterNumber = custNewMeter;
                    }
                    return userMenterNo;
                }
                else
                {
                    cstObj.METER_NUMBER = cstObj?.METER_NUMBER;
                    var custNewArea = await readingService.UpsertMeter(cstObj);
                    return cstObj?.METER_NUMBER;
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        }

        [RelayCommand]
        public async Task<string> UpdateCustomerLocation()
        {
            StatusMessage = "";
            try
            {
                var cstObj1 = await dbContext.Database.Table<Reading>()
                              .Where(r => r.CUSTOMER_NUMBER == Customer.Custnmbr)
                              .FirstOrDefaultAsync();

                if (cstObj1 != null)
                {
                    var locations = await GetAvailableLocationsAsync();
                    var userLocation = await Shell.Current.DisplayActionSheet("Select Location", "Cancel", null, locations);

                    if (string.IsNullOrEmpty(userLocation) || userLocation == "Cancel")
                    {
                        return null;
                    }

                    if (!string.IsNullOrEmpty(userLocation))
                    {
                        cstObj1.AREA = userLocation;

                        var custNewArea = await readingService.UpsertArea(cstObj1);

                        if (!string.IsNullOrEmpty(custNewArea))
                        {
                            await Shell.Current.DisplayAlert("Success!", "Location Updated!", "OK");
                            CustStateErf = custNewArea;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", "Location could not be updated", "OK");
            }
            return "";
        }

        public async Task<(decimal? Latitude, decimal? Longitude)> GetCustomerLocationCoordinatesAsync()
        {
            try
            {
                var location = await geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    location = await geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromSeconds(10),
                        RequestFullAccuracy = true,
                    });
                }

                if (location != null)
                {
                    return ((decimal)location.Latitude, (decimal)location.Longitude);
                }
                else
                {
                    return (null, null);
                }
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Error!", "Coordinates could not be retrieved", "OK");
                return (null, null);
            }
        }

        [RelayCommand]
        public async Task OpenMapPageAsync()
        {
            await Shell.Current.GoToAsync(nameof(CustomerMapPage), true, new Dictionary<string, object>
            {
                { "CustomerNumber", Customer.Custnmbr }
            });
        }

        public bool IsLocationSet => Latitude != 0 || Longitude != 0;

        public bool IsLocationNotSet => Latitude == 0 && Longitude == 0;
        // Override partial setters so bindings update when Lat/Long changes
        partial void OnLatitudeChanged(decimal value)
        {
            OnPropertyChanged(nameof(IsLocationSet));
            OnPropertyChanged(nameof(IsLocationNotSet));
        }

        partial void OnLongitudeChanged(decimal value)
        {
            OnPropertyChanged(nameof(IsLocationSet));
            OnPropertyChanged(nameof(IsLocationNotSet));
        }
    }
}
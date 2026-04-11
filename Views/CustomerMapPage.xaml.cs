using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views
{
    [QueryProperty(nameof(CustomerNumber), "CustomerNumber")]
    public partial class CustomerMapPage : ContentPage
    {
        private readonly CustomerMapViewModel _viewModel;
        private Pin? _selectedPin;
        private string _currentFilter = "All Readings";

        public string CustomerNumber { get; set; }

        public CustomerMapPage(CustomerMapViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            customerMap.MapClicked += OnMapClicked;
        }

        private async void OnBackArrowClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Center on user first — runs in parallel with data load
            _ = CenterOnCurrentLocation();

            if (!string.IsNullOrEmpty(CustomerNumber))
            {
                filterChips.IsVisible = false;
                await LoadSingleCustomerPin();
            }
            else
            {
                filterChips.IsVisible = true;
                await LoadPinsAsync("All Readings");
            }
        }

        // ── Location ───────────────────────────────────────────────
        private async Task CenterOnCurrentLocation()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(5));
                    var loc = await Geolocation.GetLocationAsync(request);
                    if (loc != null)
                        customerMap.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromKilometers(3)));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
            }
        }

        // ── Single customer pin ────────────────────────────────────
        private async Task LoadSingleCustomerPin()
        {
            await _viewModel.LoadCustomersAsync("All Readings");

            var customer = _viewModel.Customers.FirstOrDefault(c => c.CUSTOMER_NUMBER == CustomerNumber);
            if (customer != null && customer.Latitude.HasValue && customer.Longitude.HasValue)
            {
                var location = new Location((double)customer.Latitude.Value, (double)customer.Longitude.Value);
                customerMap.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1)));

                _selectedPin = new Pin
                {
                    Label = $"{customer.ERF_NUMBER ?? customer.CUSTOMER_NUMBER}",
                    Address = $"Meter: {customer.METER_NUMBER}",
                    Type = PinType.Place,
                    Location = location
                };
                customerMap.Pins.Add(_selectedPin);
            }

            UpdatePinCount();
        }

        // ── Multi-customer pins ────────────────────────────────────
        private async Task LoadPinsAsync(string readingStatus)
        {
            _currentFilter = readingStatus;
            UpdateChipStyles(readingStatus);

            try
            {
                await _viewModel.LoadCustomersAsync(readingStatus);

                customerMap.Pins.Clear();

                // Build all pins first, then add — avoids per-pin UI updates
                var pins = new List<Pin>();

                foreach (var customer in _viewModel.Customers)
                {
                    if (!customer.Latitude.HasValue || !customer.Longitude.HasValue)
                        continue;

                    bool isCaptured = customer.ReadingTaken;

                    pins.Add(new Pin
                    {
                        Label = $"{customer.ERF_NUMBER?.Trim() ?? customer.CUSTOMER_NUMBER}",
                        Address = isCaptured
                            ? $"Meter: {customer.METER_NUMBER?.Trim()} · {customer.CURRENT_READING}"
                            : $"Meter: {customer.METER_NUMBER?.Trim()} · Uncaptured",
                        Type = PinType.Place,
                        Location = new Location(
                            (double)customer.Latitude.Value,
                            (double)customer.Longitude.Value)
                    });
                }

                // Add pins in one batch
                foreach (var pin in pins)
                    customerMap.Pins.Add(pin);

                // Center on first pin
                if (pins.Count > 0)
                {
                    customerMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                        pins[0].Location,
                        Distance.FromKilometers(3)));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPinsAsync error: {ex}");
            }

            UpdatePinCount();
        }

        // ── Filter chip handlers ───────────────────────────────────
        private async void OnFilterAll(object sender, EventArgs e)
            => await LoadPinsAsync("All Readings");

        private async void OnFilterUncaptured(object sender, EventArgs e)
            => await LoadPinsAsync("Uncaptured");

        private async void OnFilterCaptured(object sender, EventArgs e)
            => await LoadPinsAsync("Captured");

        private async void OnFilterClicked(object sender, EventArgs e)
        {
            string result = await Shell.Current.DisplayActionSheet(
                "Filter Readings", "Cancel", null,
                "All Readings", "Uncaptured", "Captured");

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
                await LoadPinsAsync(result);
        }

        private void UpdateChipStyles(string active)
        {
            var activeColor = Color.FromArgb("#EE2AABEE");
            var inactiveColor = Color.FromArgb("#88FFFFFF");

            chipAll.BackgroundColor        = active == "All Readings" ? activeColor : inactiveColor;
            chipUncaptured.BackgroundColor = active == "Uncaptured"   ? activeColor : inactiveColor;
            chipCaptured.BackgroundColor   = active == "Captured"     ? activeColor : inactiveColor;

            foreach (var chip in new[] { chipAll, chipUncaptured, chipCaptured })
            {
                if (chip.Content is Label lbl)
                    lbl.TextColor = chip.BackgroundColor == activeColor
                        ? Colors.White : Color.FromArgb("#1A3A5C");
            }
        }

        private void UpdatePinCount()
        {
            int count = customerMap.Pins.Count;
            if (count > 0)
            {
                lblPinCount.Text = $"{count} meter(s) on map";
                pinCountBadge.IsVisible = true;
            }
            else
            {
                lblPinCount.Text = "No meters with GPS data";
                pinCountBadge.IsVisible = true;
            }
        }

        // ── Map tap to relocate (single-customer mode only) ────────
        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(CustomerNumber)) return;

            if (_selectedPin != null)
                customerMap.Pins.Remove(_selectedPin);

            _selectedPin = new Pin
            {
                Label = $"Cust: {CustomerNumber}",
                Type = PinType.Place,
                Location = new Location(e.Location.Latitude, e.Location.Longitude)
            };
            customerMap.Pins.Add(_selectedPin);

            _ = _viewModel.SaveCustomerLocationAsync(
                CustomerNumber,
                (decimal)e.Location.Latitude,
                (decimal)e.Location.Longitude);
        }
    }
}

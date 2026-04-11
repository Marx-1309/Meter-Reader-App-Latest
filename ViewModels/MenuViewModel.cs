using CommunityToolkit.Maui.Core;
using MeterReaderApp.Services;

namespace MeterReaderApp.ViewModels
{
    public partial class MenuViewModel : BaseViewModel
    {
        private readonly ReadingExportService _readingExportService;

        public ObservableCollection<MeterReaderApp.Models.Menu> Menus { get; }

        // Header observables — loaded on appear
        [ObservableProperty] private string greeting   = "Hello 👋";
        [ObservableProperty] private string todayDate  = "";
        [ObservableProperty] private string loggedUser = "";

        public MenuViewModel(ReadingExportService readingExportService)
        {
            _readingExportService = readingExportService
                ?? throw new ArgumentNullException(nameof(readingExportService));

            Menus = new ObservableCollection<MeterReaderApp.Models.Menu>
            {
                new() { Name = "Analytics Dashboard", Image = "analytics_icon.png",
                        Url = "AnalyticsPage",            IsActive = true  },
                new() { Name = "Photo Gallery",        Image = "camera_icon.jpg",
                        Url = "GalleryPage",              IsActive = true  },
                new() { Name = "Abnormal Readings",   Image = "abnormal_use_icon.png",
                        Url = "ExceptionReadingListPage", IsActive = false },
                new() { Name = "My Notes",            Image = "notes_icon.png",
                        Url = "NotesListPage",            IsActive = false },
                new() { Name = "Scan New Customers",  Image = "scan_db_icon.png",
                        Url = "SyncNewCustomersPage",     IsActive = true  },
                new() { Name = "Recycle Readings",    Image = "export_sync.png",
                        Url = "ReflushPage",              IsActive = true  },
                new() { Name = "Google Maps",         Image = "map_sketch_icon.png",
                        Url = "CustomerMapPage",          IsActive = true  },
            };
        }

        public void RefreshHeader()
        {
            int h    = DateTime.Now.Hour;
            Greeting = h < 12 ? "Good Morning ☀️" : h < 17 ? "Good Afternoon 🌤" : "Good Evening 🌙";
            TodayDate  = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            var email  = Preferences.Default.Get("username", "");
            LoggedUser = string.IsNullOrEmpty(email) ? "Meter Reader"
                       : email.Contains('@') ? email.Split('@')[0] : email;
        }

        [RelayCommand]
        private async Task GoToDetails(Models.Menu menu)
        {
            if (menu is null) return;
            try
            {
                if (!string.IsNullOrEmpty(menu.Url))
                    await Shell.Current.GoToAsync(menu.Url);
            }
            catch (Exception ex) { DisplayToast($"Navigation error: {ex.Message}"); }
        }

        [RelayCommand]
        public async Task ConfirmLogout()
        {
            var pending = await _readingExportService.PendingNotSyncedReadings();
            if (pending > 0)
            {
                await Shell.Current.DisplayAlert(
                    $"{pending} reading(s) not uploaded!",
                    "Please sync pending readings first.", "OK");
                return;
            }

            bool confirmed = await Shell.Current.DisplayAlert(
                "Sign Out",
                $"Sign out of {Preferences.Default.Get("username", "user")}?",
                "Sign Out", "Cancel");
            if (!confirmed) return;

            IsBusy = true;
            try
            {
                await Task.Delay(800);
                SecureStorage.Remove("Token");
                Preferences.Default.Clear();
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            finally { IsBusy = false; }
        }
    }
}

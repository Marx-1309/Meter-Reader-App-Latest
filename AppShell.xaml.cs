using MeterReaderApp.Services;

namespace MeterReaderApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(CustomerDetailPage),          typeof(CustomerDetailPage));
        Routing.RegisterRoute(nameof(MonthPage),                   typeof(MonthPage));
        Routing.RegisterRoute(nameof(ListOfReadingByMonthPage),    typeof(ListOfReadingByMonthPage));
        Routing.RegisterRoute(nameof(MonthCustomerTabPage),        typeof(MonthCustomerTabPage));
        Routing.RegisterRoute(nameof(LoginPage),                   typeof(LoginPage));
        Routing.RegisterRoute(nameof(LogoutPage),                  typeof(LogoutPage));
        Routing.RegisterRoute(nameof(OnboardingPage),              typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(CapturedReadingsPage),        typeof(CapturedReadingsPage));
        Routing.RegisterRoute(nameof(UncapturedReadingsPage),      typeof(UncapturedReadingsPage));
        Routing.RegisterRoute(nameof(ReflushPage),                 typeof(ReflushPage));
        Routing.RegisterRoute(nameof(SynchronizationPage),         typeof(SynchronizationPage));
        Routing.RegisterRoute(nameof(LocationPage),                typeof(LocationPage));
        Routing.RegisterRoute(nameof(UncapturedReadingsByAreaPage),typeof(UncapturedReadingsByAreaPage));
        Routing.RegisterRoute(nameof(NotesListPage),               typeof(NotesListPage));
        Routing.RegisterRoute(nameof(NotesDetailsPage),            typeof(NotesDetailsPage));
        Routing.RegisterRoute(nameof(SyncNewCustomersPage),        typeof(SyncNewCustomersPage));
        Routing.RegisterRoute(nameof(ExceptionReadingListPage),    typeof(ExceptionReadingListPage));
        Routing.RegisterRoute(nameof(MenuPage),                    typeof(MenuPage));
        Routing.RegisterRoute(nameof(AppShell),                    typeof(AppShell));
        Routing.RegisterRoute(nameof(CustomerMapPage),             typeof(CustomerMapPage));
        Routing.RegisterRoute(nameof(AnalyticsPage),               typeof(AnalyticsPage));
        Routing.RegisterRoute(nameof(GalleryPage),                typeof(GalleryPage));

        // Apply correct flyout colours for the current theme on first load
        ApplyFlyoutTheme(ThemeService.IsDark);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyFlyoutTheme(ThemeService.IsDark);
        CheckIfValidToken();
    }

    // ── Dynamic sidebar colours ──────────────────────────────────────────────
    private void ApplyFlyoutTheme(bool isDark)
    {
        if (isDark)
        {
            // Deep-ocean gradient for dark mode
            FlyoutBackground = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#0D1F33"), 0f),
                    new GradientStop(Color.FromArgb("#1A3A5C"), 0.5f),
                    new GradientStop(Color.FromArgb("#1A7DB5"), 1f),
                },
                new Point(0, 0), new Point(0, 1));

            if (flyoutHeader  is not null) flyoutHeader.BackgroundColor  = Color.FromArgb("#0D1F33");
            if (flyoutFooter  is not null) flyoutFooter.BackgroundColor  = Color.FromArgb("#071628");
        }
        else
        {
            // Bright Telegram-blue gradient for light mode — white text stays crisp
            FlyoutBackground = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#2AABEE"), 0f),
                    new GradientStop(Color.FromArgb("#1E94D2"), 0.5f),
                    new GradientStop(Color.FromArgb("#1580BC"), 1f),
                },
                new Point(0, 0), new Point(0, 1));

            if (flyoutHeader  is not null) flyoutHeader.BackgroundColor  = Color.FromArgb("#1A8ECC");
            if (flyoutFooter  is not null) flyoutFooter.BackgroundColor  = Color.FromArgb("#1565A8");
        }
    }

    // ── Token validation ────────────────────────────────────────────────────
    public async Task CheckIfValidToken()
    {
        await Task.Delay(50);
        IsBusy = true;
        var token = await SecureStorage.GetAsync("Token");

        if (string.IsNullOrEmpty(token))
        {
            IsBusy = false;
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
        else
        {
            var jwt = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwt is null || jwt.ValidTo < DateTime.UtcNow)
            {
                SecureStorage.Remove("Token");
                Preferences.Default.Clear();
                IsBusy = false;
                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
            else
            {
                lblUsername.Text = jwt.Claims.First(c => c.Type == "email").Value;
                lblUserSite.Text = Preferences.Default.Get("userSite", "");
                IsBusy = false;
            }
        }
    }

    private void BtnFlush_Clicked(object sender, EventArgs e) =>
        Shell.Current.GoToAsync(nameof(SynchronizationPage), true,
            new Dictionary<string, object> { { "Refresh", "Refresh" } });

    private void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        ThemeService.Set(e.Value ? AppTheme.Dark : AppTheme.Light);
        ApplyFlyoutTheme(e.Value);
    }
}

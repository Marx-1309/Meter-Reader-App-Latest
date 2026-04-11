namespace MeterReaderApp.Services;

/// <summary>Persist + apply Light/Dark theme across app restarts.</summary>
public static class ThemeService
{
    private const string Key = "app_theme";

    public static void ApplySaved()
    {
        var saved = (AppTheme)Preferences.Default.Get(Key, (int)AppTheme.Unspecified);
        if (saved != AppTheme.Unspecified)
            Application.Current!.UserAppTheme = saved;
    }

    public static void Set(AppTheme theme)
    {
        Application.Current!.UserAppTheme = theme;
        Preferences.Default.Set(Key, (int)theme);
    }

    public static bool IsDark =>
        Application.Current?.UserAppTheme == AppTheme.Dark ||
        (Application.Current?.UserAppTheme == AppTheme.Unspecified &&
         Application.Current?.RequestedTheme == AppTheme.Dark);
}

using CommunityToolkit.Maui.Core;

namespace MeterReaderApp.Services
{
    /// <summary>
    /// Safe alert helpers. All methods silently swallow exceptions so they
    /// can never cause a secondary crash, even when called before the Shell
    /// navigation stack is fully settled (e.g. on first page load on Android).
    /// </summary>
    public static class AlertService
    {
        public static Task ShowSuccess(string message, int seconds = 4) =>
            ShowSnackbar(message, "#1B6E2D", seconds);

        public static Task ShowInfo(string message, int seconds = 4) =>
            ShowSnackbar(message, "#1A7DB5", seconds);

        public static Task ShowWarning(string message, int seconds = 5) =>
            ShowSnackbar(message, "#B45309", seconds);

        public static Task ShowError(string message, int seconds = 5) =>
            ShowSnackbar(message, "#B91C1C", seconds);

        private static async Task ShowSnackbar(string message, string hexColor, int seconds)
        {
            try
            {
                // Guard: Shell must be present and have a current page
                if (Shell.Current?.CurrentPage is null)
                {
                    Debug.WriteLine($"[AlertService] Shell not ready — skipping: {message}");
                    return;
                }

                var snackbar = Snackbar.Make(
                    message,
                    action:           null,
                    actionButtonText: "OK",
                    duration:         TimeSpan.FromSeconds(seconds),
                    visualOptions: new SnackbarOptions
                    {
                        BackgroundColor       = Color.FromArgb(hexColor),
                        TextColor             = Colors.White,
                        ActionButtonTextColor = Colors.White,
                        CornerRadius          = new CornerRadius(14),
                        Font                  = Microsoft.Maui.Font.SystemFontOfSize(14),
                        CharacterSpacing      = 0.2,
                    });
                await snackbar.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AlertService] Snackbar failed: {ex.Message} — msg: {message}");
            }
        }

        public static async Task<bool> Confirm(string title, string message,
            string accept = "Confirm", string cancel = "Cancel")
        {
            try
            {
                if (Shell.Current?.CurrentPage is null) return false;
                return await Shell.Current.DisplayAlert(title, message, accept, cancel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AlertService] Confirm failed: {ex.Message}");
                return false;
            }
        }

        public static Task Notify(string message, AlertLevel level = AlertLevel.Info, int seconds = 4) =>
            level switch
            {
                AlertLevel.Success => ShowSuccess(message, seconds),
                AlertLevel.Warning => ShowWarning(message, seconds),
                AlertLevel.Error   => ShowError(message, seconds),
                _                  => ShowInfo(message, seconds),
            };
    }

    public enum AlertLevel { Info, Success, Warning, Error }
}

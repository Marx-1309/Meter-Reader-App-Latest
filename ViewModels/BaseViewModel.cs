namespace SampleMauiMvvmApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool isBusy;

        [ObservableProperty] private string title;
        [ObservableProperty] private string statusMessage;

        public bool IsNotBusy => !IsBusy;

        public void DisplayToast(string message) =>
            Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
    }
}

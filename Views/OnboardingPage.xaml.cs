namespace SampleMauiMvvmApp.Views;

public partial class OnboardingPage : ContentPage
{
    private readonly OnboardingViewModel _viewModel;

    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        Preferences.Default.Set(UIConstants.OnboardingShown, string.Empty);
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetInitializationDataCommand.Execute(null);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var parameters = new Dictionary<string, object>
        {
            [nameof(LoginViewModel.IsFirstTime)] = true
        };
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}", parameters);
    }
}

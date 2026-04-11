using SampleMauiMvvmApp.Services;
using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views.SecurityPages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        themeSwitch.IsToggled = ThemeService.IsDark;
    }

    protected override bool OnBackButtonPressed() => true;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Slide-up + fade entrance
        await Task.WhenAll(
            formPanel.FadeTo(1, 400, Easing.CubicOut),
            formPanel.TranslateTo(0, 0, 400, Easing.CubicOut));
    }

    private void OnThemeToggled(object sender, ToggledEventArgs e) =>
        ThemeService.Set(e.Value ? AppTheme.Dark : AppTheme.Light);
}

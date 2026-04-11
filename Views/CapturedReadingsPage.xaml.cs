using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class CapturedReadingsPage : ContentPage
{
    private readonly ReadingViewModel _viewModel;

    public CapturedReadingsPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        capturedList.Opacity = 0;
        capturedList.TranslationY = 14;
        _ = LoadAndAnimateAsync();
    }

    private async Task LoadAndAnimateAsync()
    {
        try
        {
            await _viewModel.GetCapturedReadingsCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CapturedReadingsPage.LoadAndAnimateAsync error: {ex}");
        }
        finally
        {
            await Task.WhenAll(
                capturedList.FadeTo(1, 320, Easing.CubicOut),
                capturedList.TranslateTo(0, 0, 320, Easing.CubicOut));
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _ = capturedList.FadeTo(0, 120, Easing.CubicIn);
    }
}

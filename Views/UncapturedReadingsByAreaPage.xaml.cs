using SampleMauiMvvmApp.Models;

namespace SampleMauiMvvmApp.Views;

[QueryProperty(nameof(Readings), "Readings")]
public partial class UncapturedReadingsByAreaPage : ContentPage
{
    private readonly ReadingViewModel _viewModel;

    public UncapturedReadingsByAreaPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    public List<Reading> Readings
    {
        set => _viewModel.SetReadings(value);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        readingsList.Opacity = 0;
        readingsList.TranslationY = 14;
        _ = AnimateIn();
    }

    private async Task AnimateIn()
    {
        await Task.Delay(100);
        await Task.WhenAll(
            readingsList.FadeTo(1, 300, Easing.CubicOut),
            readingsList.TranslateTo(0, 0, 300, Easing.CubicOut));
    }
}

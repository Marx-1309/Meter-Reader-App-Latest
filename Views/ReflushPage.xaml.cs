namespace MeterReaderApp.Views;

public partial class ReflushPage : ContentPage
{
    public ReadingViewModel viewModel;

    public ReflushPage(ReadingViewModel _viewModel)
    {
        InitializeComponent();
        viewModel = _viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        reflushContent.Opacity = 0;
        reflushContent.TranslationY = 16;
        await Task.WhenAll(
            reflushContent.FadeTo(1, 380, Easing.CubicOut),
            reflushContent.TranslateTo(0, 0, 380, Easing.CubicOut));
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await reflushContent.FadeTo(0, 150, Easing.CubicIn);
    }
}

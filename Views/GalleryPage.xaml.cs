using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class GalleryPage : ContentPage
{
    private readonly GalleryViewModel _vm;

    public GalleryPage(GalleryViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadPhotosCommand.ExecuteAsync(null);
        photoGrid.Opacity = 0;
        photoGrid.TranslationY = 16;
        await Task.WhenAll(
            photoGrid.FadeTo(1, 380, Easing.CubicOut),
            photoGrid.TranslateTo(0, 0, 380, Easing.CubicOut));
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await photoGrid.FadeTo(0, 150, Easing.CubicIn);
    }
}

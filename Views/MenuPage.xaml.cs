namespace SampleMauiMvvmApp.Views;

public partial class MenuPage : ContentPage
{
    private readonly MenuViewModel _vm;

    public MenuPage(MenuViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _vm.RefreshHeader();
        menuGrid.Opacity = 0;
        menuGrid.ScaleX  = 0.96;
        menuGrid.ScaleY  = 0.96;
        await Task.WhenAll(
            menuGrid.FadeTo(1, 350, Easing.CubicOut),
            menuGrid.ScaleTo(1, 350, Easing.CubicOut));
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await menuGrid.FadeTo(0, 150, Easing.CubicIn);
    }
}

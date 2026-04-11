using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class MonthPage : ContentPage
{
    private readonly MonthViewModel _viewModel;

    public MonthPage(MonthViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetMonthsCommand.Execute(null);
        monthList.Opacity = 0;
        monthList.TranslationY = 16;
        await Task.WhenAll(
            monthList.FadeTo(1, 380, Easing.CubicOut),
            monthList.TranslateTo(0, 0, 380, Easing.CubicOut));
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await monthList.FadeTo(0, 150, Easing.CubicIn);
    }
}

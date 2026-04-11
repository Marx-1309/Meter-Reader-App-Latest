using MeterReaderApp.ViewModels;

namespace MeterReaderApp.Views;

public partial class LoadingPage : ContentPage
{
    private readonly LoadingViewModel _viewModel;

    public LoadingPage(LoadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetInitializationDataCommand.Execute(null);
    }
}

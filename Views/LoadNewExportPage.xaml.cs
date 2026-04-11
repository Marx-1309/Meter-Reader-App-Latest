using MeterReaderApp.ViewModels;

namespace MeterReaderApp.Views;

public partial class LoadNewExportPage : ContentPage
{
    private readonly OnboardingViewModel _viewModel;

    public LoadNewExportPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetNewExportDataCommand.Execute(null);
    }
}

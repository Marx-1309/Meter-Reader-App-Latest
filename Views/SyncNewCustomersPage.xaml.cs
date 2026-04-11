namespace SampleMauiMvvmApp.Views;

public partial class SyncNewCustomersPage : ContentPage
{
    private readonly ReadingViewModel _viewModel;

    public SyncNewCustomersPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ScanForNewlyAddedCustomerReadingsCommand.Execute(null);
    }
}

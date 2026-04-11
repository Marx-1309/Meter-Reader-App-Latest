namespace MeterReaderApp.Views;

[QueryProperty("Refresh", "Refresh")]
public partial class SynchronizationPage : ContentPage
{
    private readonly ReadingViewModel _viewModel;

    public SynchronizationPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}

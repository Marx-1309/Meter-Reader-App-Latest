namespace SampleMauiMvvmApp.Views;

public partial class ExceptionReadingListPage : ContentPage
{
    private readonly ReadingViewModel _viewModel;

    public ExceptionReadingListPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GoToExceptionListCommand.Execute(null);
    }
}

namespace MeterReaderApp.Views;

public partial class CustomerDetailPage : ContentPage
{
    private readonly CustomerDetailViewModel _viewModel;

    public CustomerDetailPage(CustomerDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.VmReading = new ReadingWrapper(new ReadingFaker().Generate());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CustDisplayDetailsCommand.Execute(null);
        detailPanel.Opacity = 0;
        detailPanel.TranslationY = 24;
        await Task.WhenAll(
            detailPanel.FadeTo(1, 400, Easing.CubicOut),
            detailPanel.TranslateTo(0, 0, 400, Easing.CubicOut));
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await detailPanel.FadeTo(0, 150, Easing.CubicIn);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args) =>
        base.OnNavigatedTo(args);
}

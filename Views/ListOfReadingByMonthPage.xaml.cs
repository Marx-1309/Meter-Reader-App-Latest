using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class ListOfReadingByMonthPage : ContentPage
{
    public ListOfReadingByMonthPage(MonthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args) =>
        base.OnNavigatedTo(args);
}

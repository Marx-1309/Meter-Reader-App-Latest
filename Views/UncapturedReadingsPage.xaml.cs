namespace SampleMauiMvvmApp.Views;

public partial class UncapturedReadingsPage : ContentPage
{
    private readonly ReadingViewModel _viewModel;
    private bool _firstLoad = true;

    public UncapturedReadingsPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        readingsList.Opacity      = 0;
        readingsList.TranslationY = 14;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            // On first load on Android, yield enough frames so Shell.CurrentPage
            // is fully resolved and the DB is initialised before any access.
            if (_firstLoad)
            {
                _firstLoad = false;
                await Task.Delay(350);
            }

            // Await the command so exceptions surface and the UI waits for data
            await _viewModel.GetUncapturedReadingsCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UncapturedReadingsPage.LoadAsync error: {ex}");
        }
        finally
        {
            await Task.WhenAll(
                readingsList.FadeTo(1, 300, Easing.CubicOut),
                readingsList.TranslateTo(0, 0, 300, Easing.CubicOut));
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _ = readingsList.FadeTo(0, 100, Easing.CubicIn);
    }

    protected override bool OnBackButtonPressed() => true;
}

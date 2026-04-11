namespace MeterReaderApp.Views;

public partial class LocationPage : ContentPage
{
    private readonly ReadingViewModel _viewModel;
    private bool _firstLoad = true;

    public LocationPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        locationsList.Opacity = 0;
        locationsList.TranslationY = 14;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            if (_firstLoad)
            {
                _firstLoad = false;
                await Task.Delay(200);
            }

            await _viewModel.GetLocationsCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LocationPage.LoadAsync error: {ex}");
        }
        finally
        {
            await Task.WhenAll(
                locationsList.FadeTo(1, 300, Easing.CubicOut),
                locationsList.TranslateTo(0, 0, 300, Easing.CubicOut));
        }
    }
}

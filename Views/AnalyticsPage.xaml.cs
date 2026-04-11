using MeterReaderApp.ViewModels;

namespace MeterReaderApp.Views;

public partial class AnalyticsPage : ContentPage
{
    private readonly AnalyticsViewModel _vm;

    public AnalyticsPage(AnalyticsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        dashPanel.Opacity = 0;
        dashPanel.TranslationY = 20;
        await Task.WhenAll(
            dashPanel.FadeTo(1, 420, Easing.CubicOut),
            dashPanel.TranslateTo(0, 0, 420, Easing.CubicOut));

        await _vm.LoadAnalyticsCommand.ExecuteAsync(null);
        await AnimateBars();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await dashPanel.FadeTo(0, 150, Easing.CubicIn);
    }

    private async Task AnimateBars()
    {
        double trackWidth = Math.Max(Width - 60, 260);
        double capTarget  = trackWidth * (_vm.CaptureRate / 100.0);
        double syncTarget = trackWidth * (_vm.SyncRate    / 100.0);

        new Animation(v => captureBar.WidthRequest = v, 0, capTarget,  Easing.CubicOut)
            .Commit(this, "CaptureBar", length: 700);
        new Animation(v => syncBar.WidthRequest    = v, 0, syncTarget, Easing.CubicOut)
            .Commit(this, "SyncBar",    length: 700);

        await Task.Delay(720);
    }
}

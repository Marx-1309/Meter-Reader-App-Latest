using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

[QueryProperty("NoteDetails", "NoteDetails")]
public partial class NotesDetailsPage : ContentPage
{
    public NotesDetailsPage(NotesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        noteForm.Opacity = 0;
        noteForm.TranslationY = 20;
        await Task.WhenAll(
            noteForm.FadeTo(1, 380, Easing.CubicOut),
            noteForm.TranslateTo(0, 0, 380, Easing.CubicOut));
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await noteForm.FadeTo(0, 150, Easing.CubicIn);
    }
}

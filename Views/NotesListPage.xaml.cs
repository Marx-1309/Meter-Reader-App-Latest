namespace SampleMauiMvvmApp.Views;

public partial class NotesListPage : ContentPage
{
    private readonly NotesViewModel _vm;

    public NotesListPage(NotesViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _vm.GetNotesListCommand.Execute(null);
        notesList.Opacity = 0;
        notesList.TranslationY = 14;
        await Task.WhenAll(
            notesList.FadeTo(1, 350, Easing.CubicOut),
            notesList.TranslateTo(0, 0, 350, Easing.CubicOut));
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await notesList.FadeTo(0, 150, Easing.CubicIn);
    }
}

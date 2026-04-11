namespace SampleMauiMvvmApp.Interfaces
{
    public interface INotesService
    {
        Task<int> AddNote(Notes note);

        Task<bool> CheckExistingNoteListById(int Id);

        Task<int> DeleteNote(Notes note);

        Task<List<Notes>> GetNotesList();

        Task<int> UpdateNote(Notes note);
    }
}
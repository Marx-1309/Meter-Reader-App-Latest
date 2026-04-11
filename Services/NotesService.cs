using SampleMauiMvvmApp.Interfaces;

namespace SampleMauiMvvmApp.Services
{
    public class NotesService : INotesService
    {
        protected readonly DbContext _dbConnection;

        public NotesService(DbContext dbContext)
        {
            this._dbConnection = dbContext;
        }

        public async Task<int> AddNote(Notes note)
        {
            return await _dbConnection.Database.InsertAsync(note);
        }

        public async Task<int> DeleteNote(Notes note)
        {
            return await _dbConnection.Database.DeleteAsync(note);
        }

        public async Task<List<Notes>> GetNotesList()
        {
            List<Notes> readingList = await _dbConnection.Database.Table<Notes>().ToListAsync();
            return readingList;
        }

        public async Task<bool> CheckExistingNoteListById(int Id)
        {
            var readingListById = await _dbConnection.Database
                .Table<Notes>()
                .Where(r => r.NoteID == Id)
                .ToListAsync();

            if (readingListById.Any())
            {
                return true;
            }
            ;
            return false;
        }

        public async Task<int> UpdateNote(Notes note)
        {
            return await _dbConnection.Database.UpdateAsync(note);
        }
    }
}
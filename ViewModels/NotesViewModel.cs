namespace SampleMauiMvvmApp.ViewModels
{
    [QueryProperty(nameof(NoteDetails), "NoteDetails")]
    public partial class NotesViewModel : ObservableObject
    {
        [ObservableProperty] private Notes     _noteDetails   = new Notes();
        [ObservableProperty] private ImageSource linkedPhotoSource;
        [ObservableProperty] private string    linkedPhotoTitle = "";
        [ObservableProperty] private string    linkedPhotoDate  = "";
        [ObservableProperty] private bool      hasLinkedPhoto;
        [ObservableProperty] private bool      isPhotoViewerOpen;
        [ObservableProperty] private bool      isBusy;

        public static List<Notes> NotesListForSearch { get; private set; } = new();
        public ObservableCollection<Notes> Notes { get; } = new();

        private readonly NotesService _notesService;
        private readonly DbContext    _db;

        public NotesViewModel(NotesService notesService, DbContext db)
        {
            _notesService = notesService;
            _db           = db;
        }

        // ── Converter stub so XAML NullToBoolConverter doesn't break ──────────
        // The XAML uses a converter that doesn't exist yet — we handle visibility
        // through HasLinkedPhoto instead. Keep the XAML clean via code-behind.

        // ── When NoteDetails is set via query property, load its linked photo ──
        partial void OnNoteDetailsChanged(Notes value)
        {
            if (value is not null)
                _ = LoadLinkedPhotoAsync(value);
        }

        private async Task LoadLinkedPhotoAsync(Notes note)
        {
            HasLinkedPhoto    = false;
            LinkedPhotoSource = null;
            LinkedPhotoTitle  = "";
            LinkedPhotoDate   = "";

            if (note.NoteID <= 0) return;

            // Ensure DB is ready (NotesViewModel is Transient — DB may not be initialised yet)
            if (_db.Database is null)
            {
                try
                {
                    _db.Database = new SQLiteAsyncConnection(
                        DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
                }
                catch { return; }
            }

            try
            {
                ReadingMedia match = null;

                // ── Strategy 1: Note was created by a meter-comment ──────────
                // Title format: "Meter Issues : Erf {ERF_NUMBER}"
                if (note.NoteTitle?.StartsWith("Meter Issues") == true)
                {
                    var erf = note.NoteTitle
                        .Replace("Meter Issues : Erf ", "")
                        .Replace("Meter Issues : Erf", "")
                        .Trim();

                    if (!string.IsNullOrEmpty(erf))
                    {
                        // Load readings and match with trimming (ERF_NUMBER often
                        // has trailing spaces from SQL Server imports)
                        var allReadings = await _db.Database.Table<Reading>().ToListAsync();
                        var reading = allReadings
                            .Where(r => r.ERF_NUMBER != null
                                     && r.ERF_NUMBER.Trim() == erf)
                            .OrderByDescending(r => r.Id)
                            .FirstOrDefault();

                        if (reading != null && reading.WaterReadingExportDataID > 0)
                        {
                            match = await _db.Database.Table<ReadingMedia>()
                                .Where(m => m.WaterReadingExportDataId == reading.WaterReadingExportDataID)
                                .OrderByDescending(m => m.Id)
                                .FirstOrDefaultAsync();
                        }

                        // Fallback: check if any ReadingMedia title contains the ERF
                        if (match is null)
                        {
                            var allMedia = await _db.Database.Table<ReadingMedia>()
                                .OrderByDescending(m => m.Id)
                                .ToListAsync();

                            match = allMedia.FirstOrDefault(m =>
                                m.Title != null &&
                                m.Title.Contains(erf, StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }

                // ── Strategy 2: Note has its own base64 Image field ──────────
                if (match is null && !string.IsNullOrEmpty(note.Image))
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(note.Image);
                        LinkedPhotoSource = ImageSource.FromStream(() => new MemoryStream(bytes));
                        LinkedPhotoTitle  = note.NoteTitle ?? "Note Photo";
                        LinkedPhotoDate   = note.Date;
                        HasLinkedPhoto    = true;
                        return;
                    }
                    catch { /* not valid base64, skip */ }
                }

                // ── Strategy 3: Try title-based match against ReadingMedia ───
                if (match is null && !string.IsNullOrEmpty(note.NoteTitle))
                {
                    var allMedia = await _db.Database.Table<ReadingMedia>()
                        .OrderByDescending(m => m.Id)
                        .ToListAsync();

                    match = allMedia.FirstOrDefault(m =>
                        m.Title != null &&
                        m.Title.Contains(note.NoteTitle, StringComparison.OrdinalIgnoreCase));
                }

                if (match is null || string.IsNullOrEmpty(match.MeterImage)) return;

                var photoBytes = Convert.FromBase64String(match.MeterImage);
                LinkedPhotoSource = ImageSource.FromStream(() => new MemoryStream(photoBytes));
                LinkedPhotoTitle  = match.Title ?? "Meter Photo";
                LinkedPhotoDate   = match.DateTaken;
                HasLinkedPhoto    = true;
            }
            catch (Exception ex) { Debug.WriteLine($"Photo load: {ex}"); }
        }

        [RelayCommand]
        public void ViewLinkedPhoto() => IsPhotoViewerOpen = true;

        [RelayCommand]
        public void ClosePhotoViewer() => IsPhotoViewerOpen = false;

        // ── CRUD ──────────────────────────────────────────────────────────────
        [RelayCommand]
        public async Task UpsertNote()
        {
            int response = -1;
            if (NoteDetails.NoteID > 0)
            {
                if (string.IsNullOrEmpty(NoteDetails.NoteTitle))
                {
                    await Shell.Current.DisplayAlert("Empty Field", "Please enter a title.", "OK");
                    return;
                }
                response = await _notesService.UpdateNote(NoteDetails);
            }
            else
            {
                response = await _notesService.AddNote(new Notes
                {
                    Date        = DateTime.Now.ToString("dd MMM yyyy h:mm tt"),
                    NoteTitle   = NoteDetails.NoteTitle,
                    NoteContent = NoteDetails.NoteContent,
                    Image       = NoteDetails.Image,
                });
            }

            if (response > 0)
            {
                await Shell.Current.DisplayAlert("Saved", "Note saved successfully.", "OK");
                NoteDetails = new Notes();
                await Shell.Current.GoToAsync("../");
            }
            else
            {
                await AlertService.ShowError("Could not save the note.");
            }
        }

        [RelayCommand]
        public async Task GetNotesList()
        {
            IsBusy = true;
            try
            {
                Notes.Clear();
                var list = await _notesService.GetNotesList();
                if (list?.Count > 0)
                {
                    foreach (var n in list.OrderByDescending(n => n.NoteID))
                        Notes.Add(n);
                    NotesListForSearch.Clear();
                    NotesListForSearch.AddRange(list);
                }
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public async Task AddUpdateNote() =>
            await Shell.Current.GoToAsync(nameof(NotesDetailsPage));

        [RelayCommand]
        public async Task DisplayAction(Notes note)
        {
            var action = await Shell.Current.DisplayActionSheet(
                "Select Option", "Cancel", null, "Edit", "Delete");

            if (action == "Edit")
            {
                NoteDetails = note;
                await Shell.Current.GoToAsync(nameof(NotesDetailsPage),
                    new Dictionary<string, object> { { "NoteDetails", note } });
            }
            else if (action == "Delete")
            {
                bool confirmed = await Shell.Current.DisplayAlert(
                    "Delete Note", "Delete this note permanently?", "Delete", "Cancel");
                if (!confirmed) return;
                var r = await _notesService.DeleteNote(note);
                if (r > 0) await GetNotesList();
            }
        }

        [RelayCommand]
        public async Task DeleteNote(Notes note)
        {
            var r = await _notesService.DeleteNote(note);
            if (r > 0) await GetNotesList();
        }

        [RelayCommand]
        public async Task GoBackAsync() =>
            await Shell.Current.GoToAsync("../");
    }
}

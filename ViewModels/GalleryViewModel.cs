using Microsoft.Maui.Graphics;

namespace MeterReaderApp.ViewModels
{
    public partial class GalleryViewModel : BaseViewModel
    {
        private readonly DbContext _db;

        public GalleryViewModel(DbContext db)
        {
            _db = db;
            Title = "Photo Gallery";
        }

        public ObservableCollection<GalleryItem> Photos { get; } = new();

        [ObservableProperty] private GalleryItem selectedPhoto;
        [ObservableProperty] private bool isViewerOpen;

        [RelayCommand]
        public async Task LoadPhotos()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var media = await _db.Database.Table<ReadingMedia>()
                    .OrderByDescending(r => r.Id)
                    .ToListAsync();

                Photos.Clear();
                foreach (var m in media)
                {
                    if (string.IsNullOrEmpty(m.MeterImage)) continue;
                    try
                    {
                        var bytes  = Convert.FromBase64String(m.MeterImage);
                        var source = ImageSource.FromStream(() => new MemoryStream(bytes));
                        Photos.Add(new GalleryItem
                        {
                            Id          = m.Id,
                            Title       = m.Title ?? "Meter Photo",
                            DateTaken   = m.DateTaken,
                            IsSynced    = m.IsSynced,
                            Source      = source,
                            Base64Data  = m.MeterImage,
                            ExportDataId = m.WaterReadingExportDataId,
                        });
                    }
                    catch { /* skip corrupt entries */ }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public void OpenPhoto(GalleryItem item)
        {
            SelectedPhoto = item;
            IsViewerOpen  = true;
        }

        [RelayCommand]
        public void CloseViewer() => IsViewerOpen = false;

        [RelayCommand]
        public async Task DeletePhoto(GalleryItem item)
        {
            if (item is null) return;
            bool confirmed = await Shell.Current.DisplayAlert(
                "Delete Photo", "Remove this photo permanently?", "Delete", "Cancel");
            if (!confirmed) return;

            await _db.Database.Table<ReadingMedia>()
                     .DeleteAsync(r => r.Id == item.Id);
            Photos.Remove(item);
            if (IsViewerOpen && SelectedPhoto?.Id == item.Id)
                IsViewerOpen = false;
        }
    }

    public class GalleryItem
    {
        public int         Id           { get; set; }
        public string      Title        { get; set; }
        public string      DateTaken    { get; set; }
        public bool        IsSynced     { get; set; }
        public ImageSource Source       { get; set; }
        public string      Base64Data   { get; set; }
        public int         ExportDataId { get; set; }
        public string      SyncLabel    => IsSynced ? "Synced" : "Pending";
        public Color       SyncColor    => IsSynced
            ? Color.FromArgb("#1B6E2D")
            : Color.FromArgb("#B45309");
    }
}

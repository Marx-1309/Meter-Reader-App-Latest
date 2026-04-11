namespace SampleMauiMvvmApp.Interfaces
{
    public interface IReadingExportService
    {
        Task<List<ReadingExport>> CheckForNewExportInSql();

        Task CheckNewExports();

        Task DeleteOldReadings();

        Task FlushAndSeed();

        Task GetLatestExportItemIntoSqlite();

        Task GoBackAsync();

        Task<int> PendingNotSyncedReadings();

        Task ScanForNewItems();

        Task<List<Reading>> ScanNewLocationsFromSql();
    }
}
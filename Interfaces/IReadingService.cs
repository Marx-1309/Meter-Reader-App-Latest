namespace SampleMauiMvvmApp.Interfaces
{
    public interface IReadingService
    {
        Task<int> CountReadingsByCustomerId(string customerId);

        Task<Reading> DeleteReading(Reading reading);

        Task<List<Reading>> GetAllCaptureAndUncapturedReadings();

        Task<List<Reading>> GetAllCapturedReadings();

        Task<List<Reading>> GetAllUncapturedByIdAsync(Customer customerId);

        Task<List<Reading>> GetAllUncapturedReadings();

        Task<Reading> GetCurrentMonthReadingByCustIdAsync(string Id);

        Task<Reading> GetLastReadingByIdAsync(string Id);

        Task<int?> GetLatestExportItemId();

        Task<int?> GetLatestExportItemMonthId();

        Task<int?> GetLatestExportItemYear();

        Task<List<Reading>> GetListOfCapturedReadings();

        Task<List<LocationReadings>> GetListOfLocations();

        Task<List<ReadingDto>> GetListOfPrevMonthReadingFromSql();

        Task<List<ReadingExport>> GetListOfReadingExportFromSql();

        Task<List<ReadingDto>> GetListOfReadingFromSql();

        Task<List<Reading>> GetListOfReadingsNotSynced();

        Task<List<Reading>> GetListOfUncapturedReadings();

        Task<List<Reading>> GetListOfUncapturedReadingsByMonthId(int MonthId);

        Task<List<Reading>> GetReadingsByCustomerId(string customerId);

        Task<List<Reading>> GetReadingsByMonthId(int monthId);

        Task<List<Reading>> GetUncapturedReadingsByArea(LocationReadings x, string c = "CBD");

        Task<Reading> InsertReading(Reading reading);

        Task<bool> IsPrevMonthReadingsExist();

        Task<bool> IsReadingExistForMonthId(string customer);

        Task<List<Reading>> ScanNewCustomersReadingsFromSql();

        Task<int> SyncImages();

        Task<int> SyncReadingsByMonthIdAsync(int Id);

        List<Reading> TrimObjProperties(List<Reading> readings);

        Task<Reading> UpdateReading(Reading reading);

        Task<string> UpsertArea(Reading reading1);

        Task<string> UpsertMeter(Reading reading2);
    }
}
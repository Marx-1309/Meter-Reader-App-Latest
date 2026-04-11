namespace SampleMauiMvvmApp.Interfaces
{
    public interface IMonthService
    {
        Task<string> GetCurrentMonthNameById(int Id);

        Task<int?> GetLatestExportItemMonthId();

        Task<string> GetLatestExportItemMonthName();

        Task<List<Month>> GetListOfMonthsFromSql();

        Task<List<Month>> GetListOfMonthsFromSqlite();

        Task<string> GetMonthNameById();

        Task<List<Month>> GetMonths();

        Task<List<Reading>> GetReadingsByMonthIdAsync(int MonthId);

        Task<bool> IsMonthPopulated(Month month);
    }
}
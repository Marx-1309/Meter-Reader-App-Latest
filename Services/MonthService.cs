using SampleMauiMvvmApp.Interfaces;

namespace SampleMauiMvvmApp.Services
{
    public partial class MonthService : BaseService, IMonthService
    {
        public MonthService(DbContext dbContext) : base(dbContext)
        {
        }

        // ── Hard-coded month list — no API or DB call needed ──────────
        private static readonly List<Month> HardCodedMonths = new()
        {
            new Month { MonthID = 1,  MonthName = "January" },
            new Month { MonthID = 2,  MonthName = "February" },
            new Month { MonthID = 3,  MonthName = "March" },
            new Month { MonthID = 4,  MonthName = "April" },
            new Month { MonthID = 5,  MonthName = "May" },
            new Month { MonthID = 6,  MonthName = "June" },
            new Month { MonthID = 7,  MonthName = "July" },
            new Month { MonthID = 8,  MonthName = "August" },
            new Month { MonthID = 9,  MonthName = "September" },
            new Month { MonthID = 10, MonthName = "October" },
            new Month { MonthID = 11, MonthName = "November" },
            new Month { MonthID = 12, MonthName = "December" },
        };

        public Task<List<Month>> GetMonths()
        {
            return Task.FromResult(new List<Month>(HardCodedMonths));
        }

        public Task<string> GetMonthNameById()
        {
            return GetMonthNameByIdAsync();
        }

        private async Task<string> GetMonthNameByIdAsync()
        {
            try
            {
                var latestExportItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

                if (latestExportItem == null) return null;

                return HardCodedMonths
                    .FirstOrDefault(m => m.MonthID == latestExportItem.MonthID)?.MonthName;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public Task<string> GetCurrentMonthNameById(int id)
        {
            var name = HardCodedMonths
                .FirstOrDefault(m => m.MonthID == id)?.MonthName ?? "";
            return Task.FromResult(name);
        }

        public async Task<List<Reading>> GetReadingsByMonthIdAsync(int monthId)
        {
            try
            {
                return await dbContext.Database.Table<Reading>()
                    .Where(x => x.MonthID == monthId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
                return new List<Reading>();
            }
        }

        public async Task<List<Month>> GetListOfMonthsFromSql()
        {
            // No longer needed — months are hardcoded.
            // Kept for interface compatibility; returns the hardcoded list.
            return await Task.FromResult(new List<Month>(HardCodedMonths));
        }

        public async Task<List<Month>> GetListOfMonthsFromSqlite()
        {
            try
            {
                // Get the current month from the latest reading export
                var reading = await dbContext.Database.Table<Reading>()
                    .FirstOrDefaultAsync(r => r.WaterReadingExportID != 0);

                if (reading == null)
                {
                    StatusMessage = "No reading found.";
                    return null;
                }

                int currentMonthId = (int)reading.MonthID;

                int previousMonthId = currentMonthId - 1;
                int nextMonthId = currentMonthId + 1;

                if (previousMonthId == 0) previousMonthId = 12;
                if (nextMonthId == 13) nextMonthId = 1;

                // Filter from hardcoded list — no DB or API call
                var months = HardCodedMonths
                    .Where(m => m.MonthID == previousMonthId
                             || m.MonthID == currentMonthId
                             || m.MonthID == nextMonthId)
                    .ToList();

                return months.Count == 3 ? months : null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public Task<string> GetLatestExportItemMonthName()
        {
            return GetLatestExportItemMonthNameAsync();
        }

        private async Task<string> GetLatestExportItemMonthNameAsync()
        {
            try
            {
                var lastItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

                if (lastItem == null) return null;

                return HardCodedMonths
                    .FirstOrDefault(m => m.MonthID == lastItem.MonthID)?.MonthName;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public async Task<int?> GetLatestExportItemMonthId()
        {
            try
            {
                var lastItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

                return lastItem?.MonthID;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public Task<bool> IsMonthPopulated(Month month)
        {
            return IsMonthPopulatedAsync(month);
        }

        private async Task<bool> IsMonthPopulatedAsync(Month month)
        {
            try
            {
                int count = await dbContext.Database.Table<Reading>()
                    .Where(r => r.MonthID == month.MonthID)
                    .CountAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }
    }
}

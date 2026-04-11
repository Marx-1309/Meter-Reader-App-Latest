namespace SampleMauiMvvmApp.Services
{
    public partial class CustomerMapService : BaseService
    {
        protected DbContext _dbContext;
        private readonly IMapper _mapper;

        public CustomerMapService(DbContext dbContext, IMapper mapper) : base(dbContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        private async Task EnsureDbAsync()
        {
            if (_dbContext.Database is not null) return;
            _dbContext.Database = new SQLiteAsyncConnection(
                DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
            await _dbContext.Database.CreateTablesAsync(CreateFlags.None,
                typeof(SampleMauiMvvmApp.Models.Reading),
                typeof(SampleMauiMvvmApp.Models.Customer));
        }

        /// <summary>
        /// Returns ONLY readings that have GPS coordinates, filtered by status.
        /// Queries are scoped to the latest export and capped at 300 pins max.
        /// </summary>
        public async Task<List<ReadingDto>> GetCustomersWithCoordinatesAsync(string readingStatus)
        {
            try
            {
                await EnsureDbAsync();

                // Get latest export so we only show current-period readings
                var latestExport = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

                if (latestExport is null)
                    return new List<ReadingDto>();

                int exportId = latestExport.WaterReadingExportID;

                // Query only readings for the current export period
                List<Reading> allForExport;

                if (readingStatus == "Captured")
                {
                    allForExport = await dbContext.Database.Table<Reading>()
                        .Where(r => r.WaterReadingExportID == exportId && r.ReadingTaken == true)
                        .ToListAsync();
                }
                else if (readingStatus == "Uncaptured")
                {
                    allForExport = await dbContext.Database.Table<Reading>()
                        .Where(r => r.WaterReadingExportID == exportId && r.ReadingTaken == false)
                        .ToListAsync();
                }
                else
                {
                    allForExport = await dbContext.Database.Table<Reading>()
                        .Where(r => r.WaterReadingExportID == exportId)
                        .ToListAsync();
                }

                // Filter to only those with valid coordinates (in memory — fast on the smaller set)
                // and cap at 300 to keep the map responsive
                var withCoords = allForExport
                    .Where(r => r.Latitude.HasValue && r.Longitude.HasValue
                             && r.Latitude != 0 && r.Longitude != 0)
                    .Take(300)
                    .ToList();

                return _mapper.Map<List<ReadingDto>>(withCoords);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
                return new List<ReadingDto>();
            }
        }

        public async Task<bool> UpdateCustomerLocationAsync(string customerNo, decimal latitude, decimal longitude)
        {
            try
            {
                await EnsureDbAsync();

                var reading = await dbContext.Database.Table<Reading>()
                    .FirstOrDefaultAsync(r => r.CUSTOMER_NUMBER == customerNo);

                if (reading == null)
                {
                    StatusMessage = "Customer not found.";
                    return false;
                }

                reading.ReadingSync = false;
                reading.Latitude = latitude;
                reading.Longitude = longitude;
                reading.CoordinatesUpdated = true;

                await dbContext.Database.UpdateAsync(reading);
                StatusMessage = "Customer location updated successfully.";
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating location: {ex.Message}";
                return false;
            }
        }
    }
}

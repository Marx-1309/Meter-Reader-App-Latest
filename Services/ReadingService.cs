using MeterReaderApp.Interfaces;

namespace MeterReaderApp.Services
{
    public class ReadingService : BaseService, IReadingService
    {
        private HttpClient _httpClient;
        private readonly IMapper _mapper;
        private AuthenticationService _authenticationService;
        private MonthService _monthService;
        private IConnectivity connectivity;
        private DbContext dbContext;

        public ReadingService(DbContext dbContext, IMapper mapper,
            AuthenticationService authenticationService, MonthService monthService, IConnectivity _connectivity) : base(dbContext)
        {
            this._httpClient = new HttpClient();
            this._mapper = mapper;
            this._authenticationService = authenticationService;
            this._monthService = monthService;
            string StatusMessage = String.Empty;
            this.connectivity = _connectivity;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Ensures the SQLite connection is open. Safe to call multiple times —
        /// returns immediately if already initialised. Prevents NullReferenceException
        /// on Android when the page loads before BaseService.Init() has run.
        /// </summary>
        private async Task EnsureDbAsync()
        {
            if (dbContext.Database is not null) return;
            dbContext.Database = new SQLiteAsyncConnection(
                DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
            await dbContext.Database.CreateTablesAsync(CreateFlags.None,
                typeof(MeterReaderApp.Models.Reading),
                typeof(MeterReaderApp.Models.Month),
                typeof(MeterReaderApp.Models.ReadingExport),
                typeof(MeterReaderApp.Models.ReadingMedia),
                typeof(MeterReaderApp.Models.Customer),
                typeof(MeterReaderApp.Models.Notes));
        }

        public async Task<List<Reading>> GetReadingsByCustomerId(string customerId)
        {
            try
            {
                await EnsureDbAsync();

                var readings = await dbContext.Database.Table<Reading>()
                                         .Where(x => x.CUSTOMER_NUMBER == customerId)
                                         .ToListAsync();

                return readings;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
                return new List<Reading>(); // return empty list instead of null
            }
        }


        public async Task<List<Reading>> GetReadingsByMonthId(int monthId)
        {
            try
            {
                await EnsureDbAsync();
                await _authenticationService.SetAuthToken();
                var readingslist = await dbContext.Database.Table<Reading>().Where(x => x.MonthID == monthId && x.CURRENT_READING >= 0 && x.ReadingTaken == true).ToListAsync();
                return readingslist;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }

            return null;
        }

        public async Task<List<Reading>> GetUncapturedReadingsByArea(LocationReadings? x, string c = "CBD")
        {
            try
            {
                if (x.AREANAME != null)
                {
                    //Check if the area is unknown

                    var trimmedArea = x.AREANAME.Trim();

                    var b = await dbContext.Database.Table<Reading>()
                        .ToListAsync();

                    List<Reading> trimmedR = TrimObjProperties(b);
                    //List<Reading> readings = trimmedArea;

                    var areas = trimmedR
                        .Where(x => x.AREA == trimmedArea &&
                               x.CURRENT_READING == 0 &&
                               x.ReadingNotTaken == true &
                               x.ReadingTaken == false).ToList();

                    if (areas.Count == 0)
                    {
                        StatusMessage = $"All reading captured for {trimmedArea}";
                    }

                    return areas;
                }
                else
                {
                    StatusMessage = "Invalid area parameter (null).";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }

            return new List<Reading>();
        }

        public List<Reading> TrimObjProperties(List<Reading> readings)
        {
            try
            {
                foreach (var i in readings)
                {
                    i.AREA = i.AREA?.Trim();
                }
                return readings;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }

            return new List<Reading>();
        }

        public async Task<int> CountReadingsByCustomerId(string customerId)
        {
            try
            {
                await EnsureDbAsync();

                return await dbContext.Database
                    .Table<Reading>()
                    .Where(x => x.CUSTOMER_NUMBER == customerId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }

            return 0;
        }

        public async Task<Reading> InsertReading(Reading reading)
        {
            try
            {
                await EnsureDbAsync();
                var meterReader = Preferences.Default.Get("username", "Unknown");

                if (reading.CURRENT_READING == 0)
                {
                    reading.ReadingNotTaken = false;
                    reading.ReadingTaken = true;
                }

                if (reading.CURRENT_READING == reading.PREVIOUS_READING)
                {
                    reading.ReadingNotTaken = false;
                    reading.ReadingTaken = true;
                    reading.ReadingSync = false;
                }

                if (reading.CURRENT_READING != 0 && reading.CURRENT_READING > reading.PREVIOUS_READING)
                {
                    reading.ReadingNotTaken = false;
                    reading.ReadingTaken = true;
                }
                if (!string.IsNullOrEmpty(reading.Comment))
                {
                    await dbContext.Database.InsertAsync(new Notes
                    {
                        Date = DateTime.Now.ToString("dd MMM yyyy h:mm tt"),
                        NoteTitle = $"Meter Issues : Erf {reading.ERF_NUMBER}",
                        NoteContent = reading.Comment,
                    });
                }
                //reading.METER_READER = meterReader;
                reading.ReadingDate = DateTime.Now.ToString("dd MMM yyyy h:mm tt");

                await dbContext.Database.UpdateAsync(reading);

                return reading;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to insert record. {ex.Message}";
            }

            return null;
        }

        public async Task<string> UpsertArea(Reading reading1)
        {
            try
            {
                await EnsureDbAsync();
                reading1.ReadingSync = false;
                reading1.AreaUpdated = true;
                await dbContext.Database.UpdateAsync(reading1);

                return reading1.AREA;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to insert record. {ex.Message}";
            }

            return "";
        }

        public async Task<string> UpsertMeter(Reading reading2)
        {
            try
            {
                await EnsureDbAsync();
                reading2.ReadingSync = false;
                reading2.AreaUpdated = true;
                await dbContext.Database.UpdateAsync(reading2);

                return reading2.METER_NUMBER;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to insert record. {ex.Message}";
            }

            return "";
        }

        public async Task<Reading> UpdateReading(Reading reading)
        {
            try
            {
                await EnsureDbAsync();
                await dbContext.Database.UpdateAsync(reading);

                return reading;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to update record. {ex.Message}";
            }

            return null;
        }

        public async Task<Reading> DeleteReading(Reading reading)
        {
            try
            {
                await EnsureDbAsync();
                await dbContext.Database.DeleteAsync(reading);

                return reading;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to delete record. {ex.Message}";
            }

            return null;
        }

        public async Task<List<Reading>> GetAllUncapturedByIdAsync(Customer customerId)
        {
            try
            {
                await EnsureDbAsync();
                return await dbContext.Database.Table<Reading>()
                    .Where(x => x.CURRENT_READING <= 0 && x.CUSTOMER_NUMBER == customerId.CUSTNMBR)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }

            return null;
        }

        public async Task<List<Reading>> GetListOfUncapturedReadingsByMonthId(int MonthId)
        {
            try
            {
                await EnsureDbAsync();
                var response = await dbContext.Database.Table<Reading>()
                    .Where(r => r.CURRENT_READING <= 0 && r.MonthID == MonthId)
                    .ToListAsync();
                if (response.Count > 0)
                { return response; }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }
            return null;
        }

        public async Task<List<Reading>> GetListOfUncapturedReadings()
        {
            try
            {
                await EnsureDbAsync();
                var response = await dbContext.Database.Table<Reading>()
                    .Where(r => r.CURRENT_READING <= 0 && r.MonthID > 0)
                    .OrderBy(r => r.MonthID)
                    .ThenBy(r => r.CUSTOMER_NUMBER)
                    .ToListAsync();

                if (response.Count > 0)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }

            return null;
        }

        public async Task<List<Reading>> GetListOfCapturedReadings()
        {
            try
            {
                await EnsureDbAsync();
                var response = await dbContext.Database.Table<Reading>()
                    .Where(r => r.CURRENT_READING > 0 && r.MonthID > 0)
                    .OrderBy(r => r.MonthID)
                    .ThenBy(r => r.CUSTOMER_NUMBER)
                    .ToListAsync();

                if (response.Count > 0)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }

            return null;
        }

        public async Task<Reading> GetLastReadingByIdAsync(string Id)
        {
            await EnsureDbAsync();

            var lastExportItem = await dbContext.Database.Table<ReadingExport>()
                                      .OrderByDescending(r => r.WaterReadingExportID)
                                      .FirstOrDefaultAsync();

            //var currentMonthId = await dbContext.Database.Table<ReadingExport>()
            //.OrderByDescending(r => r.MonthID)
            //.FirstOrDefaultAsync();

            //var yearOfLastMonth = await dbContext.Database.Table<ReadingExport>()
            //.OrderByDescending(r => r.Year)
            //.FirstOrDefaultAsync();

            if (lastExportItem is null) return null;

            int prevMonthId = lastExportItem.MonthID;

            if (prevMonthId == 0) // If current month is January, adjust to December of previous year
            {
                prevMonthId = 12;
            }

            var PreviousReading = await dbContext.Database.Table<Reading>()
                .Where(r => r.CUSTOMER_NUMBER == Id && r.MonthID == prevMonthId)
                .FirstOrDefaultAsync();

            return PreviousReading;
        }

        public async Task<Reading> GetCurrentMonthReadingByCustIdAsync(string Id)
        {
            try
            {
                await EnsureDbAsync();
                var currentMonth = await _monthService.GetLatestExportItemMonthId();

                if (currentMonth != null)
                {
                    var zeroMonthReading = await dbContext.Database.Table<Reading>()
                        .Where(r => r.CUSTOMER_NUMBER == Id && r.MonthID == 0)
                        .FirstOrDefaultAsync();

                    if (zeroMonthReading != null)
                    {
                        return zeroMonthReading;
                    }
                    else
                    {
                        var currentMonthReading = await dbContext.Database.Table<Reading>()
                            .Where(r => r.CUSTOMER_NUMBER == Id && r.MonthID == currentMonth)
                            .FirstOrDefaultAsync();

                        return currentMonthReading;
                    }
                }
                else
                {
                    throw new Exception("Could not retrieve current month ID.");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public async Task<List<Reading>> GetListOfReadingsNotSynced()
        {
            try
            {
                await EnsureDbAsync();
                return await dbContext.Database.Table<Reading>().Where(r => r.ReadingSync == false).ToListAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }
            return null;
        }

        public async Task<int> SyncReadingsByMonthIdAsync(int Id)
        {
            int allReadingsItemsByCount = 0;
            int itemCount = 0;
            try
            {
                await EnsureDbAsync();
                if (Id != 0 || Id < 0)
                {
                    var r = await dbContext.Database.Table<Reading>()
                        .Where(r => (r.MonthID == Id && r.ReadingSync == false && r.ReadingTaken == true && r.CURRENT_READING >= 0 && r.WaterReadingExportDataID > 0) || (r.AreaUpdated == true && r.ReadingSync == false) || (r.CoordinatesUpdated == true && r.ReadingSync == false))
                        .OrderBy(r => r.ReadingDate).ToListAsync();

                    //var loggedInUser = await dbContext.Database.Table<LoginHistory>().OrderByDescending(r => r.LoginId).FirstAsync();

                    if (r.Count > 0)
                    {
                        var response = _mapper.Map<List<UpdateReadingDto>>(r);

                        if (response.Count > 0)
                        {
                            var iii = await dbContext.Database.Table<ReadingExport>().ToListAsync();
                            foreach (var item in response)
                            {
                                string email = Preferences.Default.Get("username", "Unknown");
                                string[] parts = email.Split('@');

                                if (parts.Length > 0)
                                {
                                    item.METER_READER = parts[0];
                                }
                                else
                                {
                                    item.METER_READER = "Unknown";
                                }

                                item.Comment = item.Comment;

                                var IsSyncSuccess = await _httpClient.PutAsJsonAsync(Constants.PutReading, item);
                                StatusMessage = IsSyncSuccess.RequestMessage.ToString();
                                if (IsSyncSuccess.IsSuccessStatusCode)
                                {
                                    var updatedItem = await dbContext.Database.Table<Reading>()
                                        .Where(r => r.WaterReadingExportDataID == item.WaterReadingExportDataID)
                                        .FirstOrDefaultAsync();
                                    if (updatedItem != null)
                                    {
                                        if (updatedItem.AreaUpdated == true && updatedItem.ReadingTaken == false)
                                        {
                                            updatedItem.AreaUpdated = false;
                                        }
                                        if (updatedItem.AreaUpdated == true && updatedItem.ReadingTaken == true)
                                        {
                                            updatedItem.AreaUpdated = false;
                                            updatedItem.ReadingSync = true;
                                        }

                                        updatedItem.ReadingSync = true;

                                        await dbContext.Database.UpdateAsync(updatedItem);
                                    }

                                    itemCount++;
                                }
                                else
                                {
                                    StatusMessage = IsSyncSuccess.IsSuccessStatusCode.ToString();
                                    await Shell.Current.DisplayAlert($"Uups ,something went wrong while syncing readings.", $"{StatusMessage}", "OK");
                                }
                            }

                            List<ReadingMedia> readingMedias = new();
                            readingMedias = await dbContext.Database.Table<ReadingMedia>().ToListAsync();
                            if (readingMedias.Any())
                            {
                                await SyncImages();
                            }
                            allReadingsItemsByCount = itemCount;
                            await Shell.Current.DisplayAlert($"{allReadingsItemsByCount} Reading(s) Synced ", "Successfully", "OK");
                            await Shell.Current.DisplayAlert($"{readingMedias.Count} Image(s) Synced ", "Successfully", "OK");
                            await Task.Delay(500);
                            await Shell.Current.GoToAsync("..");
                            return allReadingsItemsByCount;
                        }
                    }
                    await Shell.Current.DisplayAlert("No readings to be synced. ", "Add new readings and try again !", "OK");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message.ToString();
            }
            return allReadingsItemsByCount;
        }

        public static int allImageItemsByCount = 0;

        public async Task<int> SyncImages()
        {
            try
            {
                await EnsureDbAsync();
                var lastExportItem = await dbContext.Database.Table<ReadingExport>()
                                     .OrderByDescending(r => r.WaterReadingExportID)
                                     .FirstOrDefaultAsync();

                var images = await dbContext.Database
                    .Table<ReadingMedia>()
                    .Where(r => r.WaterReadingExportId == lastExportItem.WaterReadingExportID && r.IsSynced != true)
                    .ToArrayAsync();

                var response = _mapper.Map<List<ImageSyncDto>>(images);

                if (response.Count > 0)
                {
                    int itemCount = 0;

                    foreach (var item in response)
                    {
                        var IsSyncSuccess = await _httpClient.PutAsJsonAsync(Constants.SyncImages, item);
                        StatusMessage = IsSyncSuccess.ReasonPhrase.ToString();

                        if (IsSyncSuccess.IsSuccessStatusCode)
                        {
                            var updatedItem = await dbContext.Database.Table<ReadingMedia>()
                                .Where(r => r.WaterReadingExportDataId == item.WaterReadingExportDataId)
                                .FirstOrDefaultAsync();

                            if (updatedItem != null)
                            {
                                updatedItem.IsSynced = true;
                                await dbContext.Database.UpdateAsync(updatedItem);
                            }

                            itemCount++;
                        }
                        else
                        {
                            StatusMessage = IsSyncSuccess.IsSuccessStatusCode.ToString();
                        }
                    }
                    var imagesToDelete = await dbContext.Database.Table<ReadingMedia>().Where(i => i.IsSynced == true).ToListAsync();

                    foreach (var i in imagesToDelete)
                    {
                        await dbContext.Database.DeleteAsync(i);
                    }

                    allImageItemsByCount = itemCount;
                    return allImageItemsByCount;
                }
                return allImageItemsByCount;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message.ToString();
            }
            return allImageItemsByCount;
        }

        public async Task<bool> IsReadingExistForMonthId(string customer)
        {
            await EnsureDbAsync();
            var latestMonthName = await _monthService.GetLatestExportItemMonthId();

            var response = await dbContext.Database.Table<Reading>()
                                 .Where(r => r.MonthID == latestMonthName && r.CUSTOMER_NUMBER == customer)
                                 .ToListAsync();
            if (response.Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<List<Reading>> GetAllUncapturedReadings()
        {
            try
            {
                await EnsureDbAsync();

                var lastExportItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

                // Guard: no export data yet — return empty list, never crash
                if (lastExportItem is null)
                    return new List<Reading>();

                // Original filter — ReadingNotTaken is persisted to DB during seed/import
                var list = await dbContext.Database.Table<Reading>()
                    .Where(r => r.WaterReadingExportID == lastExportItem.WaterReadingExportID
                             && r.MonthID == lastExportItem.MonthID
                             && r.CURRENT_READING == 0
                             && r.ReadingNotTaken == true)
                    .ToListAsync();

                if (list.Count > 0)
                    return list;

                return new List<Reading>();
            }
            catch (Exception ex)
            {
                StatusMessage = $"GetAllUncapturedReadings error: {ex.Message}";
                return new List<Reading>();
            }
        }

        public async Task<List<Reading>> GetAllCapturedReadings()
        {
            try
            {
                await EnsureDbAsync();

                var lastExportItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

                if (lastExportItem is null)
                    return new List<Reading>();

                var list = await dbContext.Database.Table<Reading>()
                    .Where(r => r.WaterReadingExportID == lastExportItem.WaterReadingExportID
                             && r.MonthID == lastExportItem.MonthID
                             && r.CURRENT_READING >= 0
                             && r.ReadingTaken == true)
                    .OrderBy(r => r.ERF_NUMBER)
                    .ToListAsync();

                return list ?? new List<Reading>();
            }
            catch (Exception ex)
            {
                StatusMessage = $"GetAllCapturedReadings error: {ex.Message}";
                return new List<Reading>();
            }
        }

        #region GetListOfReadingFromSql

        private List<Reading> readings;

        public async Task<List<ReadingDto>> GetListOfReadingFromSql()
        {
            try
            {
                await EnsureDbAsync();
                var readingsCount = await dbContext.Database.Table<Reading>().Where(c => c.WaterReadingExportDataID >= 1).ToListAsync();
                if (readingsCount.Count > 0)
                {
                    var existingIds = readingsCount.Select(r => r.WaterReadingExportDataID).ToList();

                    var response = await _httpClient.GetAsync(MeterReaderApp.API_URL_s.Constants.GetWaterReadingExportDataID);

                    if (response.IsSuccessStatusCode)
                    {
                        var newReadings = await response.Content.ReadFromJsonAsync<List<Reading>>();

                        var newItemsToInsert = newReadings.Where(r => !existingIds.Contains(r.WaterReadingExportDataID)).ToList();

                        if (newItemsToInsert.Any())
                        {
                            var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                            foreach (var item in newItemsToInsert)
                            {
                                readings?.Add(item);
                            }
                        }
                    }
                    else
                    {
                        StatusMessage = $"Failed :." + response.StatusCode;
                    }
                }
                else
                {
                    var response = await _httpClient.GetAsync(MeterReaderApp.API_URL_s.Constants.ReadingExport);

                    if (response.IsSuccessStatusCode)
                    {
                        readings = await response.Content.ReadFromJsonAsync<List<Reading>>();
                        var response2 = await dbContext.Database.InsertAllAsync(readings);
                    }
                    else
                    {
                        StatusMessage = $"Failed :." + response.StatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error." + ex.Message;
            }

            var readingDtos = _mapper.Map<List<ReadingDto>>(readings);

            return readingDtos;
        }

        #endregion GetListOfReadingFromSql

        #region Get ReadingExport

        private List<ReadingExport> readingExports;

        public async Task<List<ReadingExport>> GetListOfReadingExportFromSql()
        {
            try
            {
                await EnsureDbAsync();
                var readingsCount = await dbContext.Database.Table<ReadingExport>().Where(c => c.WaterReadingExportID >= 1).ToListAsync();
                if (readingsCount.Any())
                {
                    // Retrieve all the IDs of the existing ReadingExport items in the SQLite database
                    var existingIds = readingsCount.Select(r => r.WaterReadingExportID).ToList();

                    var response = await _httpClient.GetAsync(MeterReaderApp.API_URL_s.Constants.ReadingExport);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and deserialize the response to a List<ReadingExport>
                        var newReadingExports = await response.Content.ReadFromJsonAsync<List<ReadingExport>>();

                        // Filter the new ReadingExport items to get only the ones that do not exist in the SQLite database
                        var newItemsToInsert = newReadingExports.Where(r => !existingIds.Contains(r.WaterReadingExportID)).ToList();

                        if (newItemsToInsert.Any())
                        {
                            // Insert the new items into the SQLite database
                            var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                            // Update the readingExports list to include both existing items and new items
                            foreach (var items in newItemsToInsert)
                            {
                                readingExports?.Add(items);
                            }
                        }
                    }
                    else
                    {
                        // Handle unsuccessful response, maybe throw an exception or log an error
                        StatusMessage = $"Failed :." + response.StatusCode;
                    }
                }
                else
                {
                    var response = await _httpClient.GetAsync(MeterReaderApp.API_URL_s.Constants.ReadingExport);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and deserialize the response to a List<ReadingExport>
                        readingExports = await response.Content.ReadFromJsonAsync<List<ReadingExport>>();

                        // Insert all items into the SQLite database since there are no existing items
                        var response2 = await dbContext.Database.InsertAllAsync(readingExports);
                    }
                    else
                    {
                        // Handle unsuccessful response, maybe throw an exception or log an error
                        StatusMessage = $"Failed :." + response.StatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any other exception that might occur during the API call
                StatusMessage = $"Error." + ex.Message;
            }
            // Return the ReadingExport list, even if it's null (client code should handle this)
            return null;
        }

        #endregion Get ReadingExport

        public async Task<int?> GetLatestExportItemId()
        {
            try
            {
                await EnsureDbAsync();
                var lastItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();
                return lastItem?.WaterReadingExportID;
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
                await EnsureDbAsync();
                var lastItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.MonthID)
                    .FirstOrDefaultAsync();

                return lastItem?.MonthID;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public async Task<int?> GetLatestExportItemYear()
        {
            try
            {
                await EnsureDbAsync();
                var lastItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.Year)
                    .FirstOrDefaultAsync();

                return lastItem?.Year;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        #region Check if there are existing PrevMonth readings in the Sqlite database

        public async Task<bool> IsPrevMonthReadingsExist()
        {
            try
            {
                await EnsureDbAsync();
                var lastExportItemTask = dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

                var lastExportItem = await lastExportItemTask;
                var lastExportMonth = await lastExportItemTask;
                var lastExportYear = await lastExportItemTask;

                var ItemID = lastExportItem.WaterReadingExportID;
                var monthID = lastExportMonth.MonthID;
                var yearId = lastExportYear.Year;

                int ii = monthID - 1;
                int xx = lastExportYear.Year;

                if (ii == 0)
                {
                    ii = 12;
                    xx = xx - 1;
                }

                var results = await dbContext.Database.Table<Reading>()
                    .Where(x => x.MonthID == ii && x.Year == xx)
                    .ToListAsync();

                if (results == null || results.Count == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        #endregion Check if there are existing PrevMonth readings in the Sqlite database

        #region Download Prev Month Readings

        private List<Reading> NonMatchingReadings = new();

        public async Task<List<ReadingDto>> GetListOfPrevMonthReadingFromSql()
        {
            await EnsureDbAsync();
            bool result = (bool)await IsPrevMonthReadingsExist();
            if (!result)
            {
                try
                {
                    var lastExportItem = await dbContext.Database.Table<ReadingExport>()
                           .OrderByDescending(r => r.WaterReadingExportID)
                           .FirstOrDefaultAsync();

                    int prevMonthId = lastExportItem.MonthID;
                    int prevYearId = lastExportItem.Year;
                    if (prevMonthId == 0)
                    {
                        prevMonthId = 12;
                        lastExportItem.Year -= 1;
                    }

                    var readingsCount = await dbContext.Database.Table<Reading>().Where(c => c.WaterReadingExportDataID >= 1
                    && c.MonthID == prevMonthId
                    && c.Year == prevYearId).ToListAsync();

                    if (readingsCount.Count < 0)
                    {
                        StatusMessage = $"Previous Readings Exist!";
                        return null;
                    }
                    else
                    {
                        string userSite = Preferences.Default.Get("userSite", "");

                        string baseUrl = MeterReaderApp.API_URL_s.Constants.GetReading;
                        string requestUrl = $"{baseUrl}?billingSite={Uri.EscapeDataString(userSite)}";
                        var response = await _httpClient.GetAsync(requestUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            var readingsFromSqlServer = await response.Content.ReadAsStringAsync();
                            //var readingsFromSqlServer = await response.Content.ReadFromJsonAsync<List<ReadingDto>>();

                            var DeserializedReadingsFromSqlServer = JsonConvert.DeserializeObject<List<ReadingDto>>(readingsFromSqlServer);

                            var p = DeserializedReadingsFromSqlServer.Where(r => r.METER_NUMBER != null).ToList();
                            var lastExportItemx = await dbContext.Database.Table<ReadingExport>()
                          .OrderByDescending(r => r.WaterReadingExportID)
                          .FirstOrDefaultAsync();

                            // If current month is January, adjust to December of previous year
                            int currentMonth = lastExportItem.MonthID;
                            int prevYearIdx = lastExportItemx.Year;
                            if (currentMonth == 0)
                            {
                                currentMonth = 12;
                                lastExportItemx.Year -= 1;
                            }
                            var currentExportReadings = DeserializedReadingsFromSqlServer.Where(r => r.MonthID == currentMonth && r.PREVIOUS_READING >= 0).ToList();

                            List<Reading> readingsToUpdateToSqlite = new();
                            List<Reading> readingsToDeleteFromSqlite = new();
                            var x = await dbContext.Database.Table<Reading>().ToListAsync();
                            foreach (var readingDto in currentExportReadings)
                            {
                                // Find matching records in SQLite
                                var matchingRecords = await dbContext.Database.Table<Reading>()
                                    .Where(r => r.CUSTOMER_NUMBER == readingDto.CUSTOMER_NUMBER && r.MonthID == currentMonth /*&& r.CURRENT_READING == 0*/)
                                    .ToListAsync();
                                if (matchingRecords.Count != 0)
                                {
                                    // Update the matching records
                                    foreach (var record in matchingRecords)
                                    {
                                        if (readingDto.METER_NUMBER == null)
                                        {
                                            readingDto.METER_NUMBER = "";
                                        }
                                        if (readingDto.AREA == null)
                                        {
                                            readingDto.AREA = "";
                                        }
                                        record.WaterReadingExportDataID = readingDto.WaterReadingExportDataID;
                                        record.METER_NUMBER = readingDto.METER_NUMBER.Trim() ?? "";
                                        record.PREVIOUS_READING = readingDto.PREVIOUS_READING;
                                        record.CURRENT_READING = (decimal)readingDto.CURRENT_READING;
                                        record.AREA = readingDto.AREA ?? "";
                                        record.ReadingDate = readingDto.ReadingDate;
                                        record.METER_READER = readingDto.METER_READER ?? "";
                                        record.ERF_NUMBER = readingDto.ERF_NUMBER ?? "";
                                        record.Latitude = readingDto.Latitude ?? null;
                                        record.Longitude = readingDto.Longitude ?? null;

                                        if (readingDto.CURRENT_READING > 0)
                                        {
                                            record.ReadingSync = true;
                                            record.ReadingTaken = true;
                                            record.ReadingNotTaken = false;
                                        }
                                        if (readingDto.CURRENT_READING == 0)
                                        {
                                            record.ReadingTaken = false;
                                            record.ReadingNotTaken = true;
                                        }
                                        if (record.ReadingDate is not null && record.CURRENT_READING == 0)
                                        {
                                            record.ReadingSync = true;
                                            record.ReadingTaken = true;
                                            record.ReadingNotTaken = false;
                                        }

                                        readingsToUpdateToSqlite.Add(record);
                                    }
                                }
                            }
                            var r = await dbContext.Database.UpdateAllAsync(readingsToUpdateToSqlite);
                            readingsToUpdateToSqlite.Clear();

                            List<Reading> nonMatchingRecords = await dbContext.Database.Table<Reading>().Where(r => r.WaterReadingExportDataID == 0).ToListAsync();

                            foreach (var item in nonMatchingRecords)
                            {
                                await dbContext.Database.DeleteAsync(item);
                            }
                            //var response2 = await dbContext.Database.InsertAllAsync(readingsFromSqlServer);
                        }
                        else
                        {
                            StatusMessage = $"Error :." + response.StatusCode;
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error." + ex.Message;
                }
            }
            return null;
        }

        #endregion Download Prev Month Readings

        public async Task<List<Reading>> GetAllCaptureAndUncapturedReadings()
        {
            await EnsureDbAsync();

            var lastExportItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

            var ListOfAllReading = await dbContext.Database.Table<Reading>()
                .Where(r => r.WaterReadingExportID == lastExportItem.WaterReadingExportID
                && r.MonthID == lastExportItem.MonthID && r.WaterReadingExportDataID != 0
               )
                .OrderBy(r => r.ReadingDate)
                .ThenBy(r => r.CUSTOMER_NUMBER)
                .ToListAsync();
            if (ListOfAllReading.Count > 0)
            {
                return ListOfAllReading;
            }
            return null;
        }

        public async Task<List<LocationReadings>> GetListOfLocations()
        {
            try
            {
                await EnsureDbAsync();

                var allReadings = await dbContext.Database.Table<Reading>().Where(r => r.CURRENT_READING == 0 && r.ReadingTaken == false).ToListAsync();

                var distinctLocations = allReadings.Select(r => r.AREA?.Trim()).Distinct().ToList();

                var filteredDistincLocations = distinctLocations
                    .Where(r => !string.IsNullOrWhiteSpace(r)
                             && !r.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var listOfLocations = new List<LocationReadings>();
                foreach (var location in filteredDistincLocations)
                {
                    var count = allReadings.Count(r => r.AREA?.Trim() == location && r.CURRENT_READING >= 0);

                    LocationReadings loc = new LocationReadings();

                    if (string.IsNullOrEmpty(location) || location.Equals("NULL") || location == null)
                    {
                        loc.AREANAME = "Unknown Area";
                        loc.NumberOfReadings = count;
                    }
                    else
                    {
                        loc.AREANAME = location;
                        loc.NumberOfReadings = count;
                    }

                    loc.IsAllCaptured = false;
                    loc.IsAllNotCaptured = !loc.IsAllCaptured;

                    listOfLocations.Add(loc);

                    StatusMessage = "Looks like all the locations are captured!";
                }
                //var v = listOfLocations.Count();
                Task.Delay(50);
                return listOfLocations;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                return new List<LocationReadings>();
            }
        }

        public async Task<List<Reading>> ScanNewCustomersReadingsFromSql()
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Failed to scan for new readings!",
                    $"Please ensure connectivity and try again.", "OK");
                return null;
            }
            try
            {
                await EnsureDbAsync();
                string userSite = Preferences.Default.Get("userSite", "");
                string baseUrl = MeterReaderApp.API_URL_s.Constants.GetReading; // e.g., "https://localhost:7231/api/Reading"
                string requestUrl = $"{baseUrl}?billingSite={Uri.EscapeDataString(userSite)}";

                var responseSql = await _httpClient.GetAsync(requestUrl);

                var readingList = await dbContext.Database.Table<Reading>().ToListAsync();

                var existingCustomerNo = readingList
                       .Select(r => r.CUSTOMER_NUMBER)
                       .ToList();

                #region Getting the latest export values(Id,Month & Year)

                var latestExportItem = await dbContext.Database.Table<ReadingExport>()
                           .OrderByDescending(r => r.WaterReadingExportID)
                           .FirstOrDefaultAsync();

                int currentExportId = latestExportItem.WaterReadingExportID;
                int currentMonthId = latestExportItem.MonthID;
                int currentYearId = latestExportItem.Year;
                if (currentMonthId == 0)
                {
                    currentMonthId = 12;
                    latestExportItem.Year -= 1;
                }

                #endregion Getting the latest export values(Id,Month & Year)

                if (responseSql.IsSuccessStatusCode)
                {
                    var responseContent = await responseSql.Content.ReadAsStringAsync();
                    var newApiReadings = JsonConvert.DeserializeObject<List<Reading>>(responseContent);

                    var newReadings = newApiReadings
                            .Where(r => !existingCustomerNo.Contains(r.CUSTOMER_NUMBER) && r.WaterReadingExportID == currentExportId)
                            .ToList();
                    if (newReadings.Any())
                    {
                        foreach (var reading in newReadings)
                        {
                            await dbContext.Database.InsertAsync(new Customer
                            {
                                CUSTNAME = reading.CUSTOMER_NAME,
                                CUSTNMBR = reading.CUSTOMER_NUMBER,
                                CUSTCLAS = reading?.CUSTOMER_ZONING,
                                ZIP = reading?.ERF_NUMBER,
                                PHONE1 = reading?.PHONE1
                            });

                            reading.Comment = string.Empty;
                            reading.MonthID = currentMonthId;
                            reading.Year = currentYearId;
                            //reading.Year = await _readingService.GetLatestExportItemYear() ?? reading.Year;
                            //reading.READING_DATE = DateTime.UtcNow.ToLongDateString();
                            reading.WaterReadingExportID = currentExportId;
                            reading.METER_READER = string.Empty;
                            reading.ReadingSync = false;
                            if (reading.CURRENT_READING == 0)
                            {
                                reading.ReadingNotTaken = true;
                                reading.ReadingTaken = false;
                                reading.ReadingSync = false;
                            }
                            if (reading.CURRENT_READING > 0)
                            {
                                reading.ReadingNotTaken = false;
                                reading.ReadingTaken = true;
                                reading.ReadingSync = true;
                            }

                            await dbContext.Database.InsertAsync(reading);
                        }

                        await Shell.Current.DisplayAlert("New customers found", $"{newReadings.Count} were found and successfully inserted", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("No new customers found", "Ensure new customers are inserted in the database and try again.", "OK");
                    }

                    string tstMsg = "You Can Proceed Using The App! ";
                    Toast.Make(tstMsg, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
                    //await Shell.Current.GoToAsync(nameof(UncapturedReadingsPage));
                    return new List<Reading>();
                }
                return new List<Reading>();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                return new List<Reading>();
            }
        }
    }
}
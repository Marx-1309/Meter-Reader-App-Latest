using System.Net.NetworkInformation;

namespace SampleMauiMvvmApp.Services
{
    public class ReadingExportService
    {
        private HttpClient _httpClient;
        public string StatusMessage;
        public DbContext dbContext;
        private ReadingService readingService;
        private CustomerService customerService;
        private IConnectivity connectivity;

        public ReadingExportService(DbContext _dbContext, ReadingService _readingService, CustomerService _customerService, IConnectivity _connectivity)
        {
            this._httpClient = new HttpClient();
            this.dbContext = _dbContext;
            this.readingService = _readingService;
            this.customerService = _customerService;
            this.connectivity = _connectivity;
        }

        public ReadingExportService()
        {
        }

        #region Get ReadingExport

        private List<ReadingExport> readingExports;

        #region Check for Newly Added Exports

        public async Task CheckNewExports()
        {
            try
            {
                if (connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("No connectivity!",
                        $"Please check internet and try again.", "OK");
                    return;
                }

                var responseSql = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.ReadingExport);
                var exportsList = await dbContext.Database.Table<ReadingExport>().ToListAsync();

                var existingIds = exportsList
                           .Select(r => r.WaterReadingExportID)
                           .ToList();
                if (responseSql.IsSuccessStatusCode)
                {
                    var newReadingExports = await responseSql.Content.ReadFromJsonAsync<List<ReadingExport>>();
                    var newItemsToInsert = newReadingExports
                        .Where(r => !existingIds.Contains(r.WaterReadingExportID))
                        .ToList();

                    if (newItemsToInsert.Any())
                    {
                        latestExport.Clear();
                        latestExport.AddRange(newItemsToInsert);
                        //var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                        foreach (var item in latestExport)
                        {
                            ReadingExport readingExport = new()
                            {
                                WaterReadingExportID = item.WaterReadingExportID,
                                MonthID = item.MonthID,
                                Year = item.Year,
                            };

                            await dbContext.Database.InsertAsync(readingExport);
                        }
                    }

                    var customerInSqlite = await dbContext.Database.Table<Customer>().ToListAsync();
                    var customerFromSqlServer = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetCustomer);

                    if (customerFromSqlServer.IsSuccessStatusCode)
                    {
                        var responseContent = await customerFromSqlServer.Content.ReadAsStringAsync();
                        var sqlServerCustomerList = JsonConvert.DeserializeObject<List<Customer>>(responseContent);
                        var sqliteReadingsList = await dbContext.Database.Table<Reading>().ToListAsync();

                        var matchingCustomerIDs = sqlServerCustomerList
                            .Where(customer => sqliteReadingsList.Any(reading => reading.CUSTOMER_NUMBER == customer.CUSTNMBR))
                            .Select(customer => customer.CUSTNMBR)
                            .ToList();
                        var newCustomersToInsert = sqlServerCustomerList
                            .Where(customer => matchingCustomerIDs.Contains(customer.CUSTNMBR))
                            .ToList();

                        var sqliteCustomerList = await dbContext.Database.Table<Customer>().Where(c => c.CUSTNMBR != null).ToListAsync();

                        var newCustomersNotInSQLite = newCustomersToInsert
                            .Where(customer => !sqliteCustomerList.Any(sqliteCustomer => sqliteCustomer.CUSTNMBR == customer.CUSTNMBR))
                            .ToList();

                        if (newCustomersNotInSQLite.Count > 0)
                        {
                            await dbContext.Database.InsertAllAsync(newCustomersNotInSQLite);
                        }
                        //CustomerList = sqliteCustomerList.Concat(newCustomersNotInSQLite).ToList();
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        #endregion Check for Newly Added Exports

        private List<ReadingExport> LatestExportList { get; set; } = new();
        private List<Customer> LatestCustomerList { get; set; } = new();
        private List<Reading> LatestReadingList { get; set; } = new();

        public async Task ScanForNewItems()
        {
            if (readingExports == null)
            {
                try
                {
                    if (connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        await Shell.Current.DisplayAlert("No connectivity!",
                            $"Please check internet and try again.", "OK");
                        return;
                    }

                    var responseSql1 = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.ReadingExport);
                    var responseSql2 = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetCustomer);
                    var responseSql3 = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetReading);

                    var exportsList = await dbContext.Database.Table<ReadingExport>().ToListAsync();
                    var customerList = await dbContext.Database.Table<Customer>().ToListAsync();
                    var readingList = await dbContext.Database.Table<Reading>().ToListAsync();

                    var existingExportIds = exportsList
                           .Select(r => r.WaterReadingExportID)
                           .ToList();

                    var existingCustomerIds = customerList
                           .Select(r => r.CUSTNMBR)
                           .ToList();

                    var existingReadingIds = readingList
                           .Select(r => r.WaterReadingExportDataID)
                           .ToList();

                    var existingCustomerReadingIds = readingList
                           .Select(r => r.CUSTOMER_NUMBER)
                           .ToList();

                    if (responseSql1.IsSuccessStatusCode && responseSql2.IsSuccessStatusCode && responseSql3.IsSuccessStatusCode)
                    {
                        var responseContent1 = await responseSql1.Content.ReadAsStringAsync();
                        var responseContent2 = await responseSql2.Content.ReadAsStringAsync();
                        var responseContent3 = await responseSql3.Content.ReadAsStringAsync();

                        var newApiReadingExports = JsonConvert.DeserializeObject<List<ReadingExport>>(responseContent1);
                        var newApiCustomers = JsonConvert.DeserializeObject<List<Customer>>(responseContent2);
                        var newApiReadings = JsonConvert.DeserializeObject<List<Reading>>(responseContent3);

                        ReadingExport currentExportItem = await dbContext.Database.Table<ReadingExport>().FirstOrDefaultAsync();

                        var newExportToInsert = newApiReadingExports
                            .Where(r => !existingExportIds.Contains(r.WaterReadingExportID) && r.WaterReadingExportID > currentExportItem?.WaterReadingExportID)
                            .ToList();

                        var newCustomers = newApiCustomers
                            .Where(r => !existingCustomerIds.Contains(r.CUSTNMBR))
                            .ToList();

                        var newCustomersToInsert = newCustomers.Where(r => existingCustomerReadingIds.Contains(r.CUSTNMBR)).ToList();

                        var newReadingToInsert = newApiReadings
                            .Where(r => !existingReadingIds.Contains(r.WaterReadingExportDataID))
                            .ToList();

                        var newReadingToUpdate = newApiReadings
                            .Where(r => existingReadingIds.Contains(r.WaterReadingExportDataID))
                            .ToList();

                        if (newExportToInsert.Any())
                        {
                            await Shell.Current.DisplayAlert("New Reading Exports Found!", $"We Are Updating The App!", "OK");
                            await Shell.Current.GoToAsync($"{nameof(SynchronizationPage)}");

                            List<ReadingExport?> exportsItemsToDelete = await dbContext.Database.Table<ReadingExport>().ToListAsync();

                            if (exportsItemsToDelete.Any())
                            {
                                await dbContext.Database.Table<ReadingExport>().DeleteAsync(r => r.WaterReadingExportID > 0);
                            }

                            List<Notes> notesToDelete = await dbContext.Database.Table<Notes>().ToListAsync();

                            if (notesToDelete.Any())
                            {
                                await dbContext.Database.Table<Notes>().DeleteAsync(r => r.NoteID > 0);
                            }

                            LatestExportList.Clear();
                            LatestExportList.AddRange(newExportToInsert);

                            foreach (var item in LatestExportList)
                            {
                                ReadingExport readingExport = new()
                                {
                                    WaterReadingExportID = item.WaterReadingExportID,
                                    MonthID = item.MonthID,
                                    Year = item.Year,
                                };

                                await dbContext.Database.InsertAsync(readingExport);
                            }

                            if (newCustomersToInsert.Any())
                            {
                                LatestCustomerList.Clear();
                                LatestCustomerList.AddRange(newCustomersToInsert);

                                List<Customer> CustomerList = new();
                                foreach (var item in newCustomersToInsert)
                                {
                                    Customer customer = new()
                                    {
                                        CUSTNMBR = item.CUSTNMBR,
                                        CUSTNAME = item.CUSTNAME,
                                        CUSTCLAS = item.CUSTCLAS,
                                        PHONE1 = item.PHONE1,
                                        STATE = item.STATE,
                                        ZIP = item.ZIP,
                                    };
                                    CustomerList.Clear();
                                    CustomerList.Add(customer);
                                }
                                await dbContext.Database.InsertAllAsync(CustomerList);
                            }

                            if (newReadingToInsert.Any())
                            {
                                LatestReadingList.Clear();
                                LatestReadingList.AddRange(newReadingToInsert);

                                List<Reading> ReadingList = new List<Reading>();

                                foreach (var item in newReadingToInsert)
                                {
                                    Reading reading = new Reading
                                    {
                                        WaterReadingExportDataID = item.WaterReadingExportDataID,
                                        CURRENT_READING = item.CURRENT_READING,
                                        PREVIOUS_READING = item.PREVIOUS_READING,
                                        Comment = item.Comment,
                                        ReadingDate = item.ReadingDate,
                                        MonthID = item.MonthID,
                                        Year = item.Year
                                    };

                                    ReadingList.Add(reading);
                                }

                                await dbContext.Database.InsertAllAsync(ReadingList);
                            }

                            if (newReadingToUpdate.Any())
                            {
                                List<Reading> ReadingList = new List<Reading>();

                                var lastExportItem = await dbContext.Database.Table<ReadingExport>()
                                          .OrderByDescending(r => r.WaterReadingExportID)
                                          .FirstOrDefaultAsync();

                                foreach (var item in newReadingToUpdate)
                                {
                                    Reading recordToUpdate = await dbContext.Database.Table<Reading>()
                                        .Where(r => r.WaterReadingExportDataID == item.WaterReadingExportDataID)
                                        .FirstOrDefaultAsync();

                                    if (recordToUpdate != null)
                                    {
                                        recordToUpdate.WaterReadingExportID = lastExportItem.WaterReadingExportID;
                                        recordToUpdate.CUSTOMER_NUMBER = item.CUSTOMER_NUMBER;
                                        recordToUpdate.CUSTOMER_NAME = item.CUSTOMER_NAME;
                                        recordToUpdate.AREA = item.AREA?.Trim();
                                        recordToUpdate.PHONE1 = item.PHONE1;
                                        recordToUpdate.CUSTOMER_ZONING = item.CUSTOMER_ZONING;
                                        recordToUpdate.ERF_NUMBER = item.ERF_NUMBER;
                                        recordToUpdate.RouteNumber = item.RouteNumber;
                                        recordToUpdate.METER_NUMBER = item.METER_NUMBER;
                                        recordToUpdate.CURRENT_READING = item.CURRENT_READING;
                                        recordToUpdate.PREVIOUS_READING = item.PREVIOUS_READING;
                                        recordToUpdate.ReadingSync = false;
                                        recordToUpdate.ReadingTaken = false;
                                        recordToUpdate.Comment = string.Empty;
                                        //recordToUpdate.READING_DATE = string.Empty;
                                        recordToUpdate.MonthID = lastExportItem.MonthID;
                                        recordToUpdate.Year = lastExportItem.Year;

                                        ReadingList.Add(recordToUpdate);
                                    }
                                }
                                await dbContext.Database.UpdateAllAsync(ReadingList);
                                await Shell.Current.DisplayAlert("Done!", $"finished downloading new data!", "OK");
                                Page pg = await Shell.Current.Navigation.PopAsync();
                                Shell.Current.Navigation.RemovePage(pg);
                                await Shell.Current.GoToAsync($"{nameof(UncapturedReadingsPage)}");
                            }
                        }
                        else
                        {
                            //await Shell.Current.DisplayAlert("No New Exports Found!", $"You Can Proceed Using The App! ", "OK");
                            string tstMsg = "No New Exports Found! You Can Proceed Using The App! ";
                            Toast.Make(tstMsg, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error." + ex.Message;
                }
            }
            return;
        }

        private List<ReadingExport> latestExport { get; set; } = new List<ReadingExport>();

        public async Task<List<ReadingExport>> CheckForNewExportInSql()
        {
            if (latestExport.Count == 0)
            {
                try
                {
                    //if (connectivity.NetworkAccess != NetworkAccess.Internet)
                    //{
                    //    await Shell.Current.DisplayAlert("No connectivity!",
                    //        $"Please check internet and try again.", "OK");
                    //    return null;
                    //}
                    //Get lists from APi
                    var responseSql1 = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.ReadingExport);

                    if (responseSql1.IsSuccessStatusCode)
                    {
                        var responseContent1 = await responseSql1.Content.ReadAsStringAsync();

                        var newApiReadingExports = JsonConvert.DeserializeObject<List<ReadingExport>>(responseContent1);

                        var newExportToInsert = newApiReadingExports.ToList();

                        if (newExportToInsert.Any())
                        {
                            LatestExportList.Clear();
                            LatestExportList.AddRange(newExportToInsert);
                            // Insert the new items into the SQLite database
                            //var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                            foreach (var item in LatestExportList)
                            {
                                ReadingExport readingExport = new()
                                {
                                    WaterReadingExportID = item.WaterReadingExportID,
                                    MonthID = item.MonthID,
                                    Year = item.Year,
                                };

                                await dbContext.Database.InsertAsync(readingExport);
                            }
                        }
                        else { return null; }
                        ;
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error." + ex.Message;
                }
            }
            return LatestExportList;
        }

        public async Task FlushAndSeed()
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Failed to recycle readings!",
                    $"Please ensure connectivity and try again.", "OK");
                return;
            }
            var i = await PendingNotSyncedReadings();
            if (i > 0)
            {
                await Shell.Current.DisplayAlert($"{i} reading(s) not uploaded!",
                    $"Please sync the pending readings,and try again!", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(SynchronizationPage)}");
            try
            {
                //await customerService.GetListOfCustomerFromSql();

                #region deleting existing db data

                List<ReadingExport> result1 = await dbContext.Database.Table<ReadingExport>().Where(i => i.WaterReadingExportID > 0).ToListAsync();
                List<Reading> result2 = await dbContext.Database.Table<Reading>().Where(i => i.Id > 0).ToListAsync();
                List<Customer> result3 = await dbContext.Database.Table<Customer>().Where(i => i.CUSTNMBR != null).ToListAsync();
                List<Month> result4 = await dbContext.Database.Table<Month>().Where(i => i.MonthID > 0).ToListAsync();
                List<ReadingMedia> result5 = await dbContext.Database.Table<ReadingMedia>().Where(i => i.Id > 0).ToListAsync();

                if (result1.Count > 0 || result1.Any())
                {
                    await dbContext.Database.Table<ReadingExport>().DeleteAsync(r => r.WaterReadingExportID > 0);
                }

                if (result2.Count > 0 || result2.Any())
                {
                    await dbContext.Database.Table<Reading>().DeleteAsync(r => r.Id > 0);
                }

                if (result3.Count > 0 || result3.Any())
                {
                    await dbContext.Database.Table<Customer>().DeleteAsync(r => r.CUSTNMBR != null);
                }

                if (result4.Count > 0 || result4.Any())
                {
                    await dbContext.Database.Table<Month>().DeleteAsync(r => r.MonthID > 0);
                }

                if (result5.Count > 0 || result5.Any())
                {
                    await dbContext.Database.Table<ReadingMedia>().DeleteAsync(r => r.Id > 0);
                }

                #endregion deleting existing db data

                await ScanNewLocationsFromSql();
                await customerService.GetListOfCustomerFromSql();
                await CheckForNewExportInSql();

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

                List<Reading> GeneratedReadings = new();
                List<Customer> allCustomers = await dbContext.Database.Table<Customer>().ToListAsync();

                foreach (var customer in allCustomers)
                {
                    var existingReading = await dbContext.Database.Table<Reading>()
                        .Where(r => r.CUSTOMER_NUMBER == customer.CUSTNMBR)
                        .FirstOrDefaultAsync();

                    if (existingReading == null)
                    {
                        var readingFaker = new ReadingFaker();
                        var reading = readingFaker.Generate(1).FirstOrDefault();

                        reading.CUSTOMER_NUMBER = customer.CUSTNMBR;
                        reading.CUSTOMER_NAME = customer.CUSTNAME;
                        reading.ERF_NUMBER = customer.ZIP;
                        reading.PHONE1 = customer?.PHONE1;
                        reading.AREA = customer.STATE;
                        reading.CUSTOMER_ZONING = customer.CUSTCLAS;
                        reading.CURRENT_READING = 0;
                        reading.Comment = string.Empty;
                        reading.MonthID = currentMonthId;
                        //reading.Year = await _readingService.GetLatestExportItemYear() ?? reading.Year;
                        //reading.READING_DATE = DateTime.UtcNow.ToLongDateString();
                        reading.ReadingTaken = false;
                        reading.ReadingNotTaken = true;
                        reading.ReadingSync = false;
                        reading.WaterReadingExportID = currentExportId;
                        reading.METER_READER = string.Empty;

                        GeneratedReadings.Add(reading);
                    }
                }
                await dbContext.Database.InsertAllAsync(GeneratedReadings);

                await readingService.GetListOfPrevMonthReadingFromSql();
                await Shell.Current.DisplayAlert("Successful sync", "Readings were restored successfully", "OK");
                await GoBackAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        #endregion Get ReadingExport

        public async Task<int> PendingNotSyncedReadings()
        {
            try
            {
                var r = await dbContext.Database.Table<Reading>()
                        .Where(((r => r.ReadingSync == false
                        && r.ReadingTaken == true && r.CURRENT_READING >= 0
                        && r.WaterReadingExportDataID > 0 || (r.AreaUpdated == true && r.ReadingSync == false) || (r.CoordinatesUpdated == true && r.ReadingSync == false) )))
                        .OrderBy(r => r.ReadingDate).ToListAsync();

                if (r.Count > 0)
                {
                    return r.Count;
                }
                else if (r.Count == 0)
                {
                    return 0;
                }
                else { return 0; }
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error was encountered";
                return 0;
            }
        }

        #region GetLatestExportItemIntoSqlite

        public async Task GetLatestExportItemIntoSqlite()
        {
            var readingsCount = await dbContext.Database
                       .Table<ReadingExport>()
                       .Where(c => c.WaterReadingExportID >= 1)
                       .ToListAsync();

            if (readingsCount.Count > 0)
            {
                var existingIds = readingsCount
                    .Select(r => r.WaterReadingExportID)
                    .ToList();

                var response = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.ReadingExport);

                if (response.IsSuccessStatusCode)
                {
                    var newReadingExports = await response.Content.ReadFromJsonAsync<List<ReadingExport>>();
                    var newItemsToInsert = newReadingExports
                        .Where(r => !existingIds.Contains(r.WaterReadingExportID))
                        .ToList();

                    if (newItemsToInsert.Any())
                    {
                        latestExport.Clear();
                        latestExport.AddRange(newItemsToInsert);
                        //var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                        foreach (var item in latestExport)
                        {
                            var ExportItem = new ReadingExport
                            {
                                WaterReadingExportID = item.WaterReadingExportID,
                                MonthID = item.MonthID,
                                Year = item.Year,
                                SALSTERR = item.SALSTERR,
                            };

                            await dbContext.Database.InsertAsync(ExportItem);
                        }
                    }
                }
            }
        }

        #endregion GetLatestExportItemIntoSqlite

        #region DeleteOldReadings

        public async Task DeleteOldReadings()
        {
            #region Getting the latest export values(Id,Month & Year)

            var latestExportItem = await dbContext.Database.Table<ReadingExport>()
                       .OrderByDescending(r => r.WaterReadingExportID)
                       .FirstOrDefaultAsync();

            var latestExportMonthItem = await dbContext.Database.Table<ReadingExport>()
                   .OrderByDescending(r => r.MonthID)
                   .FirstOrDefaultAsync();

            var latestExportYearItem = await dbContext.Database.Table<ReadingExport>()
                   .OrderByDescending(r => r.Year)
                   .FirstOrDefaultAsync();

            int currentExportId = latestExportItem.WaterReadingExportID;
            int currentMonthId = latestExportMonthItem.MonthID;
            int currentYearId = latestExportYearItem.Year;
            if (currentMonthId == 0)
            {
                currentMonthId = 12;
                latestExportYearItem.Year -= 1;
            }

            #endregion Getting the latest export values(Id,Month & Year)

            List<Reading> oldRecords = await dbContext.Database.Table<Reading>().Where(r => r.WaterReadingExportDataID != currentExportId).ToListAsync();

            foreach (var item in oldRecords)
            {
                await dbContext.Database.DeleteAsync(item);
            }
        }

        #endregion DeleteOldReadings

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("../..");
        }

        public async Task<List<Reading>> ScanNewLocationsFromSql()
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Failed to scan for new locations!",
                    $"Please ensure connectivity and try again.", "OK");
                return null;
            }
            try
            {
                var responseSql = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetLocation);

                var locationsList = await dbContext.Database.Table<BillingLocation>().ToListAsync();

                var existingLocationNo = locationsList
                       .Select(r => r.Location)
                       .ToList();

                if (responseSql.IsSuccessStatusCode)
                {
                    var responseContent = await responseSql.Content.ReadAsStringAsync();

                    var newApiLocations = JsonConvert.DeserializeObject<List<BillingLocation>>(responseContent);

                    var newLocations = newApiLocations
                            .Where(r => !existingLocationNo.Contains(r.Location) /*&& r.WaterReadingExportID == currentExportId*/)
                            .ToList();
                    if (newLocations.Any())
                    {
                        foreach (var location in newLocations)
                        {
                            await dbContext.Database.InsertAsync(new BillingLocation
                            {
                                BillingLocationID = location.BillingLocationID,
                                Location = location.Location,
                                Township = location.Township
                            });

                            await dbContext.Database.InsertAsync(location);
                        }
                        string tstMsg1 = $"{newLocations.Count} found and successfully inserted";
                        await Toast.Make(tstMsg1, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
                    }
                    else
                    {
                        string tstMsg2 = "No new locations found";
                        await Toast.Make(tstMsg2, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
                    }

                    string tstMsg = "You Can Proceed Using The App! ";
                    await Toast.Make(tstMsg, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
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
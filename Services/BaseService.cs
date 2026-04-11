namespace SampleMauiMvvmApp.Services
{
    public class BaseService : ObservableObject
    {
        protected readonly DbContext dbContext;
        protected readonly AppShell _appShell;
        protected readonly MonthService _monthService;
        protected readonly ReadingService _readingService;
        protected readonly CustomerService _customerService;
        protected readonly ReadingExportService _readingExportService;
        protected readonly BillingLocationService _billingLocationService;

        public string StatusMessage;

        public BaseService(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public BaseService(DbContext dbContext, AppShell appShell, MonthService monthService,
            ReadingService readingService, CustomerService customerService,
            ReadingExportService readingExportService, BillingLocationService billingLocationService)
        {
            this.dbContext = dbContext;
            this._appShell = appShell;
            _monthService = monthService;
            _ = Init(this.dbContext);
            _readingService = readingService;
            _customerService = customerService;
            _readingExportService = readingExportService;
            _billingLocationService = billingLocationService;
        }

        public async Task Init(DbContext dbContext)
        {
            if (dbContext.Database is not null)
                return;

            dbContext.Database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);

            var migrationResult = await dbContext.Database.CreateTablesAsync(CreateFlags.None

                , typeof(SampleMauiMvvmApp.Models.Reading)
                , typeof(SampleMauiMvvmApp.Models.Month)
                , typeof(SampleMauiMvvmApp.Models.ReadingExport)
                , typeof(SampleMauiMvvmApp.Models.BillingLocation)
                , typeof(SampleMauiMvvmApp.Models.RM00303)
                , typeof(SampleMauiMvvmApp.Models.LoginHistory)
                , typeof(SampleMauiMvvmApp.Models.ReadingMedia)
                , typeof(SampleMauiMvvmApp.Models.Customer)
                , typeof(SampleMauiMvvmApp.Models.Notes));
            ;

            if (migrationResult.Results != null && migrationResult.Results.Count > 0)
            {
                bool isNewDatabase = migrationResult.Results.Any(x => x.Value.ToString().ToUpper() == "CREATED");//this line checks if its the first run after migrations
                if (isNewDatabase)
                {
                    await _customerService.GetListOfCustomerFromSql();
                    await SeedData(dbContext);
                    await _readingService.GetListOfPrevMonthReadingFromSql();
                }

                // Always sync billing locations — cheap call, rarely changes
                if (_billingLocationService != null)
                    _ = _billingLocationService.SyncFromApiAsync();
                //Check if all the existing readings are synced!

                //await _readingExportService.ScanForNewItems();
                //await _readingExportService.FlushAndSeed();
                //_appShell.CheckIfValidToken();
            }
        }

        public async Task SeedData(DbContext dbContext)
        {
            await _readingExportService.CheckForNewExportInSql();
            //await _readingService.GetListOfReadingExportFromSql();

            #region Getting the latest export values(Id,Month & Year)

            var latestExportItem = await dbContext.Database.Table<ReadingExport>()
                       .OrderByDescending(r => r.WaterReadingExportID)
                       .FirstOrDefaultAsync();

            var readingExport = new ReadingExport
            {
                WaterReadingExportID = latestExportItem.WaterReadingExportID,
                MonthID = latestExportItem.MonthID,
                Year = latestExportItem.Year,
            };

            if (readingExport.MonthID == 0)
            {
                readingExport.MonthID = 12;
                readingExport.Year -= 1;
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
                    //reading.AREA = customer.STATE;
                    reading.CUSTOMER_ZONING = customer.CUSTCLAS;
                    reading.CURRENT_READING = 0;
                    reading.Comment = string.Empty;
                    reading.MonthID = readingExport.MonthID;
                    reading.Year = readingExport.Year;
                    //reading.Year = await _readingService.GetLatestExportItemYear() ?? reading.Year;
                    //reading.READING_DATE = DateTime.UtcNow.ToLongDateString();
                    reading.WaterReadingExportID = readingExport.WaterReadingExportID;
                    reading.METER_READER = string.Empty;
                    reading.ReadingTaken = false;
                    reading.ReadingSync = false;
                    reading.ReadingNotTaken = true;

                    GeneratedReadings.Add(reading);
                }
            }
            await dbContext.Database.InsertAllAsync(GeneratedReadings);
        }
    }
}
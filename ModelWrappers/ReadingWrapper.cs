namespace SampleMauiMvvmApp.ModelWrappers
{
    public partial class ReadingWrapper : ObservableObject
    {
        public ReadingWrapper(Reading readingModel)
        {
            if (readingModel != null)
            {
                Id = readingModel.Id;
                WaterReadingExportDataID = (int)readingModel.WaterReadingExportDataID;
                WaterReadingExportId = (int)readingModel.WaterReadingExportID;
                Customer_number = readingModel.CUSTOMER_NUMBER;
                Customer_name = readingModel.CUSTOMER_NAME;
                Area = readingModel.AREA;
                Phone1 = (long)readingModel.PHONE1;
                Erf_number = readingModel.ERF_NUMBER;
                Meter_number = readingModel.METER_NUMBER;
                Current_reading = (long)readingModel.CURRENT_READING;
                Previous_reading = (decimal)readingModel.PREVIOUS_READING;
                MonthID = (int)readingModel.MonthID;
                CurrentMonth = readingModel.CurrentMonth;
                Year = (int)readingModel.Year;
                Customer_zoning = readingModel.CUSTOMER_ZONING;
                RouteNumber = readingModel.RouteNumber;
                MeterReader = readingModel.METER_READER;
                IsFlagged = (bool)readingModel.IsFlagged;
                //ReadingDate = (DateTime)readingModel.READING_DATE;
                Comment = readingModel.Comment;
                ReadingNotTaken = (bool)readingModel.ReadingNotTaken;
                Latitude = readingModel.Latitude ?? 0m;
                Longitude = readingModel.Longitude ?? 0m;
                //ReadingTaken = (bool)readingModel.ReadingTaken;
                //ReadingSync = (bool)readingModel.ReadingSync;
            }
        }

        public ReadingWrapper(List<Reading> month)
        {
            this.month = month;
        }

        public int Id;
        public int WaterReadingExportDataID { get; set; }

        [ObservableProperty]
        private int waterReadingExportId;

        [ObservableProperty]
        private string customer_number;

        [ObservableProperty]
        private string customer_name;

        [ObservableProperty]
        private string area;

        [ObservableProperty]
        private long phone1;

        [ObservableProperty]
        private string erf_number;

        [ObservableProperty]
        private string meter_number;

        [ObservableProperty]
        private string c_reading;

        [ObservableProperty]
        private long? current_reading;

        [ObservableProperty]
        private decimal previous_reading;

        [ObservableProperty]
        private int percentageChange;

        [ObservableProperty]
        private int monthID;

        [ObservableProperty]
        private string? currentMonth;

        [ObservableProperty]
        private int year;

        [ObservableProperty]
        private string customer_zoning;

        [ObservableProperty]
        private string readingDate;

        [ObservableProperty]
        private int customerId;

        [ObservableProperty]
        private bool isNew;

        [ObservableProperty]
        private int readingsID;

        [ObservableProperty]
        private string reading_number;

        [ObservableProperty]
        private string meterReader;

        [ObservableProperty]
        private string comment;

        [ObservableProperty]
        private int waterReadingTypeID;

        [ObservableProperty]
        private string routeNumber;

        [ObservableProperty]
        public bool readingTaken;

        [ObservableProperty]
        public bool isFlagged;

        [ObservableProperty]
        public bool readingNotTaken;

        [ObservableProperty]
        private bool readingSync;

        [ObservableProperty]
        private decimal latitude;

        [ObservableProperty]
        private decimal longitude;

        [ObservableProperty]
        private bool areaUpdated;

        public List<Reading> month;
    }
}
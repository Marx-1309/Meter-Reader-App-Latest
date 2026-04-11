namespace MeterReaderApp.API_URL_s
{
    public static class Constants
    {
        public const string HOST = ListOfUrl.WindhoekHome;

        //Month
        public const string GetMonth = HOST + "/api/Month";

        public const string PostMonth = HOST + "/api/Month";
        public const string GetMonthById = HOST + "/api/Month/{id}";
        public const string PutMonth = HOST + "/api/Month/{id}";
        public const string DeleteMonth = HOST + "/api/Month/{id}";

        //Reading Export
        public const string ReadingExport = HOST + "/api/ReadingExport";

        public const string ReadingExportById = HOST + "/api/ReadingExport/{id}";

        //Customer
        public const string GetCustomer = HOST + "/api/Customer";

        public const string PostCustomer = HOST + "/api/Customer";
        public const string GetCustomerById = HOST + "/api/Customer/{id}";
        public const string PutCustomer = HOST + "/api/Customer{id}";

        //Readings Data
        public const string GetReading = HOST + "/api/Reading";

        public const string PostReading = HOST + "/api/Reading";
        public const string SyncListOfReadingsToSql = HOST + "/api/Reading/list";
        public const string SyncReadingByCustomerId = HOST + "api/reading/{id}";
        public const string GetReadingById = HOST + "/api/Reading/{id}";
        public const string GetWaterReadingExportDataID = HOST + "/api/Reading/{WaterReadingExportDataID}";
        public const string PutReading = HOST + "/api/Reading/{id}";
        public const string DeleteReading = HOST + "/api/Reading/{id}";
        public const string SyncImages = HOST + "/api/Reading/Image/{id}";

        //Users
        public const string GetUser = HOST + "/api/Users";

        public const string GetUserById = HOST + "/api/Users/{id}";

        //Devices
        public const string GetDevice = HOST + "/api/Device";

        public const string GetDeviceById = HOST + "/api/Device/{id}";

        //RM00303
        public const string GetRM00303 = HOST + "/api/RM00303";

        public const string GetRM00303ById = HOST + "/api/RM00303/{id}";

        //Login
        public const string PostLogin = HOST + "/api/login";

        //Locations
        public const string GetLocation = HOST + "/api/BillingLocation";
    }

    public static class ListOfUrl
    {
        //Kinetic
        public const string KineticWifi = "http://192.168.178.50:81";

        //Local
        public const string LocalIIS = "http://127.0.0.1";

        //TN Card
        public const string TnWifi = "http://192.168.8.129:88";

        //Home Wi-Fi
        public const string OkahaoHomeWifi = "http://192.168.178.78:88";

        //My Phone
        public const string SamsungA51 = "http://192.168.57.27:82";

        public const string SamsungA35 = "http://192.168.3.138:81";

        public const string RTCOFRuacanaTcWifi = "http://192.168.178.5:81";

        public const string RTCOFAPIWifi = "http://192.168.118.251:84";

        public const string OkahaoTCStaff = "http://192.168.1.4:8088";
        public const string RuacanaTcLocalPcDb = "http://192.168.178.72:88";
        public const string OmaruruMun = "http://192.1.4.8:81";
        public const string OmaCentralHotel = "http://192.168.178.183:88";
        public const string DebugApi = "http://192.168.178.41:81";
        public const string OmusatiRC = "http://192.168.178.128:86";
        public const string Localhost = "https://localhost:7231";
        public const string WindhoekHome = "http://192.168.178.37:85";
        public const string OkahaoTcTest = "http://192.168.1.51:81";
        public const string OpuwoTC = "http://192.168.178.15:81";
    }
}
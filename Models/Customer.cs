namespace SampleMauiMvvmApp.Models
{
    public class Customer
    {
        [JsonPropertyName("CUSTNMBR")]
        public string? CUSTNMBR { get; set; }

        [JsonPropertyName("CUSTNAME")]
        public string? CUSTNAME { get; set; }

        [JsonPropertyName("CUSTCLAS")]
        public string? CUSTCLAS { get; set; }

        [JsonPropertyName("STATE")]
        public string? STATE { get; set; }

        [JsonPropertyName("ZIP")]
        public string? ZIP { get; set; }

        [JsonPropertyName("PHONE1")]
        public long? PHONE1 { get; set; } = 0;

        [JsonPropertyName("ERFNO")]
        public string? ERFNO { get; set; } = "";

        [JsonPropertyName("LATITUDE")]
        public string? LATITUDE { get; set; } = "";

        [JsonPropertyName("LONGITUDE")]
        public string? LONGITUDE { get; set; } = "";

        [OneToMany]
        public List<Reading>? Readings { get; set; }

        [Ignore]
        public string? CustomerInfo
        {
            get
            {
                return $"{CUSTNMBR}{CUSTNAME}";
            }
        }

        [Ignore]
        public string? ModelTitle => $"{CUSTNAME}";

        [Ignore]
        public string? AreaErf => $"{STATE}, ({ZIP})";

        [Ignore]
        public string? SearchHandlerProperties
        {
            get
            {
                return $"{CUSTNMBR}{CUSTNAME}{ZIP}";
            }
        }

        public static Customer GenerateNewFromWrapper(CustomerWrapper wrapper)
        {
            return new Customer()
            {
                CUSTNMBR = wrapper.Custnmbr,
                CUSTNAME = wrapper.Custname,
                CUSTCLAS = wrapper.Custclas,
                STATE = wrapper.State,
                ZIP = wrapper.Zip,
            };
        }
    }
}
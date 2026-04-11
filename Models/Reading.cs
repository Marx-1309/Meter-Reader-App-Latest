using System.ComponentModel.DataAnnotations.Schema;

namespace MeterReaderApp.Models
{
    public class Reading
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int WaterReadingExportDataID { get; set; }
        public int WaterReadingExportID { get; set; }
        public string CUSTOMER_NUMBER { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string AREA { get; set; }
        public long? PHONE1 { get; set; } = 0;
        public string ERF_NUMBER { get; set; }
        public string? METER_NUMBER { get; set; } = "";
        public decimal CURRENT_READING { get; set; }
        public decimal? PREVIOUS_READING { get; set; }
        public System.Int64 MonthID { get; set; }
        public string? CurrentMonth;
        public System.Int64 Year { get; set; }
        public string? CUSTOMER_ZONING { get; set; }
        public string? RouteNumber { get; set; }
        public string Comment { get; set; }
        public int? WaterReadingTypeId { get; set; }
        public string? METER_READER { get; set; }
        public string? ReadingDate { get; set; }  /*= DateTime.UtcNow.ToLocalTime();*/
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool ReadingTaken { get; set; }
        public bool ReadingNotTaken { get; set; }
        public bool? ReadingSync { get; set; }
        public bool? AreaUpdated { get; set; } = false;
        public bool? CoordinatesUpdated { get; set; } = false;


        [NotMapped]
        public bool? IsFlagged { get; set; } = false;

        [Ignore]
        public string? ReadingInfo
        {
            get
            {
                return $"{CUSTOMER_NAME}{ERF_NUMBER}{AREA}{METER_NUMBER}";
            }
        }

        [Ignore]
        [NotMapped]
        public string? SearchBarReadingInfo
        {
            get
            {
                return $"{CUSTOMER_NAME.Trim()}\n{ERF_NUMBER.Trim()} , Meter No : ({METER_NUMBER.Trim()})\nPrevious : {PREVIOUS_READING}";
            }
        }

        [Ignore]
        public int? PercentageChange { get; set; }

        public static Reading GenerateNewFromWrapper(ReadingWrapper wrapper)
        {
            return new Reading()
            {
                Id = wrapper.Id,
                WaterReadingExportDataID = wrapper.WaterReadingExportDataID,
                WaterReadingExportID = wrapper.WaterReadingExportId,
                CUSTOMER_NUMBER = wrapper.Customer_number,
                CUSTOMER_NAME = wrapper.Customer_name,
                AREA = wrapper.Area,
                PHONE1 = wrapper.Phone1,
                ERF_NUMBER = wrapper.Erf_number,
                METER_NUMBER = wrapper.Meter_number,
                CURRENT_READING = (decimal)wrapper.Current_reading,
                PREVIOUS_READING = wrapper.Previous_reading,
                PercentageChange = wrapper.PercentageChange,
                MonthID = wrapper.MonthID,
                CurrentMonth = wrapper.CurrentMonth,
                Year = wrapper.Year,
                CUSTOMER_ZONING = wrapper.Customer_zoning,
                RouteNumber = wrapper.RouteNumber,
                METER_READER = wrapper.MeterReader,
                //READING_DATE = wrapper.ReadingDate,
                ReadingTaken = wrapper.ReadingTaken,
                ReadingNotTaken = wrapper.ReadingNotTaken,
                IsFlagged = wrapper.IsFlagged,
                ReadingSync = wrapper.ReadingSync,
                AreaUpdated = wrapper.AreaUpdated,
                Comment = wrapper.Comment,
                Latitude = wrapper.Latitude,
                Longitude = wrapper.Longitude
            };
        }
    }
}
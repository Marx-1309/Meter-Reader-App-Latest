namespace SampleMauiMvvmApp.Models
{
    [Table("WaterReadingExport")]
    public class ReadingExport
    {
        [PrimaryKey]
        public int WaterReadingExportID { get; set; }

        [ForeignKey(typeof(Month), Name = "MonthID")]
        public int MonthID { get; set; }

        public int Year { get; set; }

        public string? SALSTERR { get; set; }

        [OneToMany]
        public List<Reading>? Readings { get; set; }
    }
}
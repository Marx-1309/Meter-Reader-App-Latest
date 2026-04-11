namespace SampleMauiMvvmApp.Models
{
    [Table("ReadingExportSnapshot")]
    public class ReadingExportSnapshot
    {
        [PrimaryKey, AutoIncrement]
        public int ExportId { get; set; }

        public int? ExportValue { get; set; }
        public string? ExportName { get; set; }
        public string? ExportDate { get; set; } = DateTime.Now.ToLongDateString();
    }
}
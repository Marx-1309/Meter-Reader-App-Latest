using System.Text.Json.Serialization;

namespace SampleMauiMvvmApp.Models
{
    [Table("Month")]
    public class Month
    {
        [JsonProperty]
        public int MonthID { get; set; }

        [JsonProperty]
        public string MonthName { get; set; }

        public bool IsActive { get; set; }
        public string TitleProp => $"{MonthName} ";
    }
}
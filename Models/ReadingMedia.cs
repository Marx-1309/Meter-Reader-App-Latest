global using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleMauiMvvmApp.Models
{
    public class ReadingMedia
    {
        [PrimaryKey]
        [AutoIncrement]
        [Unique]
        public int Id { get; set; }

        public string Title { get; set; }

        [Column("Data")]
        public string? MeterImage { get; set; }

        public int WaterReadingExportDataId { get; set; }
        public int WaterReadingExportId { get; set; }
        public bool IsSynced { get; set; } = false;
        public string DateTaken { get; set; } = DateTime.UtcNow.ToLongDateString();
    }
}
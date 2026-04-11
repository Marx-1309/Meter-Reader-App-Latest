using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleMauiMvvmApp.Models
{
    public class LocationReadings
    {
        public string? AREANAME { get; set; }
        public string? CUSTOMER_ZONING { get; set; }
        public int? NumberOfReadings { get; set; }
        public bool? IsAllCaptured { get; set; }
        public bool? IsAllNotCaptured { get; set; }
    }
}
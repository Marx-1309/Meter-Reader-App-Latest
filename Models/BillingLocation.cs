using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleMauiMvvmApp.Models
{
    public class BillingLocation
    {
        [PrimaryKey, AutoIncrement]
        public int BillingLocationID { get; set; }

        public string Location { get; set; }
        public string? Township { get; set; }
    }
}
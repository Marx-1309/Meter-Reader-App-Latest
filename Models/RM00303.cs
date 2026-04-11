namespace SampleMauiMvvmApp.Models
{
    public class RM00303
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string SLTERDSC { get; set; }

        public int INACTIVE { get; set; }

        public string SLPRSNID { get; set; }

        public string STMGRFNM { get; set; }

        public string STMGRMNM { get; set; }

        public string STMGRLNM { get; set; }

        public string COUNTRY { get; set; }

        public double COSTTODT { get; set; }

        public double TTLCOMTD { get; set; }

        public double TTLCOMLY { get; set; }

        public double NCOMSLYR { get; set; }

        public double COMSLLYR { get; set; }

        public double CSTLSTYR { get; set; }

        public double COMSLTDT { get; set; }

        public double NCOMSLTD { get; set; }

        public int KPCALHST { get; set; }

        public int KPERHIST { get; set; }

        public double NOTEINDX { get; set; }
        public DateTime MODIFDT { get; set; }
        public DateTime CREATDDT { get; set; }
        public DateTime DEX_ROW_TS { get; set; }
        public int DEX_ROW_ID { get; set; }

        [OneToMany]
        public List<ReadingExport>? ReadingExport { get; set; }
    }
}
using Microsoft.Maui.Graphics;

namespace MeterReaderApp.ViewModels
{
    public partial class AnalyticsViewModel : BaseViewModel
    {
        private readonly DbContext _db;

        public AnalyticsViewModel(DbContext db)
        {
            _db = db;
            Title = "Analytics";
        }

        [ObservableProperty] private int    totalReadings;
        [ObservableProperty] private int    capturedCount;
        [ObservableProperty] private int    uncapturedCount;
        [ObservableProperty] private int    abnormalCount;
        [ObservableProperty] private int    zeroCount;
        [ObservableProperty] private int    syncedCount;
        [ObservableProperty] private int    unsyncedCount;
        [ObservableProperty] private int    myReadingsCount;   // by logged-in user
        [ObservableProperty] private string myReadingsLabel;   // "by {username}"
        [ObservableProperty] private double captureRate;
        [ObservableProperty] private double syncRate;
        [ObservableProperty] private string captureRateText;
        [ObservableProperty] private string syncRateText;
        [ObservableProperty] private string greeting;
        [ObservableProperty] private string todayDate;

        public ObservableCollection<AreaStat>    AreaStats    { get; } = new();
        public ObservableCollection<TopConsumer> TopConsumers { get; } = new();

        [RelayCommand]
        public async Task LoadAnalytics()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                int hour   = DateTime.Now.Hour;
                Greeting   = hour < 12 ? "Good Morning ☀️" : hour < 17 ? "Good Afternoon 🌤" : "Good Evening 🌙";
                TodayDate  = DateTime.Now.ToString("dddd, dd MMMM yyyy");

                // Current user
                var email  = Preferences.Default.Get("username", "");
                var reader = email.Contains('@') ? email.Split('@')[0] : email;
                MyReadingsLabel = string.IsNullOrEmpty(reader) ? "by you" : $"by {reader}";

                var all = await _db.Database.Table<Reading>().ToListAsync();
                if (all.Count == 0) return;

                TotalReadings    = all.Count;
                CapturedCount    = all.Count(r => r.ReadingTaken);
                UncapturedCount  = all.Count(r => !r.ReadingTaken);
                AbnormalCount    = all.Count(r => r.CURRENT_READING > 0 && r.PREVIOUS_READING.HasValue
                                               && r.CURRENT_READING - r.PREVIOUS_READING.Value >= 20);
                ZeroCount        = all.Count(r => r.CURRENT_READING == r.PREVIOUS_READING);
                SyncedCount      = all.Count(r => r.ReadingSync == true);
                UnsyncedCount    = all.Count(r => r.ReadingSync != true && r.ReadingTaken);
                MyReadingsCount  = string.IsNullOrEmpty(reader)
                    ? 0
                    : all.Count(r => r.ReadingTaken &&
                                     !string.IsNullOrEmpty(r.METER_READER) &&
                                     r.METER_READER.StartsWith(reader, StringComparison.OrdinalIgnoreCase));

                CaptureRate     = TotalReadings > 0 ? Math.Round(CapturedCount * 100.0 / TotalReadings, 1) : 0;
                SyncRate        = CapturedCount > 0 ? Math.Round(SyncedCount   * 100.0 / CapturedCount, 1) : 0;
                CaptureRateText = $"{CaptureRate:F0}%";
                SyncRateText    = $"{SyncRate:F0}%";

                // Area stats
                AreaStats.Clear();
                all.Where(r => !string.IsNullOrWhiteSpace(r.AREA))
                   .GroupBy(r => r.AREA?.Trim())
                   .Select(g => new AreaStat
                   {
                       AreaName   = g.Key ?? "Unknown",
                       Total      = g.Count(),
                       Captured   = g.Count(r => r.ReadingTaken),
                       Uncaptured = g.Count(r => !r.ReadingTaken),
                   })
                   .OrderByDescending(a => a.Total)
                   .Take(8).ToList()
                   .ForEach(a => AreaStats.Add(a));

                // Top consumers
                TopConsumers.Clear();
                all.Where(r => r.CURRENT_READING > 0 && r.PREVIOUS_READING.HasValue && r.PREVIOUS_READING.Value >= 0)
                   .OrderByDescending(r => r.CURRENT_READING - r.PREVIOUS_READING)
                   .Take(5)
                   .Select(r => new TopConsumer
                   {
                       Name    = string.Join(" ", (r.CUSTOMER_NAME ?? "").Split().Take(2)),
                       Erf     = r.ERF_NUMBER ?? "",
                       Usage   = (int)(r.CURRENT_READING - (r.PREVIOUS_READING ?? 0)),
                       Current = (int)r.CURRENT_READING,
                       Area    = r.AREA?.Trim() ?? "",
                   })
                   .ToList()
                   .ForEach(t => TopConsumers.Add(t));
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
            finally { IsBusy = false; }
        }
    }

    public class AreaStat
    {
        public string AreaName   { get; set; }
        public int    Total      { get; set; }
        public int    Captured   { get; set; }
        public int    Uncaptured { get; set; }
        public double Pct        => Total > 0 ? Captured * 100.0 / Total : 0;
        public double PctRatio   => Pct / 100.0;
        public string PctText    => $"{Pct:F0}%";
        public Color  BarColor   =>
            Pct >= 80 ? Color.FromArgb("#1B6E2D")
          : Pct >= 50 ? Color.FromArgb("#0069B4")
                      : Color.FromArgb("#C62828");
    }

    public class TopConsumer
    {
        public string Name    { get; set; }
        public string Erf     { get; set; }
        public int    Usage   { get; set; }
        public int    Current { get; set; }
        public string Area    { get; set; }
        public string UsageText => $"+{Usage} kL";
    }
}

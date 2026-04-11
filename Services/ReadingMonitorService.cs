using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace MeterReaderApp.Services
{
    /// <summary>
    /// Fires a local push notification when unsynced readings reach 10% of captured.
    /// Throttled to once per day. Call CheckAndNotifyAsync() after saving a reading.
    /// </summary>
    public class ReadingMonitorService
    {
        private readonly DbContext _db;
        private const double ThresholdPct = 0.10;
        private const string PrefsKey     = "last_sync_notif_day";

        public ReadingMonitorService(DbContext db) => _db = db;

        public async Task CheckAndNotifyAsync()
        {
            try
            {
                if (_db.Database is null) return;

                var all      = await _db.Database.Table<Reading>().ToListAsync();
                int captured = all.Count(r => r.ReadingTaken);
                int unsynced = all.Count(r => r.ReadingSync != true && r.ReadingTaken);

                if (captured == 0) return;

                double ratio = (double)unsynced / captured;
                if (ratio < ThresholdPct) return;

                // Throttle: at most once per day
                string today = DateTime.Today.ToString("yyyy-MM-dd");
                if (Preferences.Default.Get(PrefsKey, "") == today) return;

                // Request permission on Android 13+
                if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
                    await LocalNotificationCenter.Current.RequestNotificationPermission();

                var request = new NotificationRequest
                {
                    NotificationId = 1001,
                    Title          = "Sync Reminder",
                    Description    = $"{unsynced} reading(s) ({ratio * 100:F0}% of captured) are unsynced. Connect to office WiFi to sync.",
                    BadgeNumber    = unsynced,
                    Android        = new AndroidOptions
                    {
                        ChannelId    = "sync_reminder",
                        // Note: AndroidNotificationPriority and AndroidColor constructor APIs
                        // vary by Plugin.LocalNotification version — omit to use safe defaults.
                        IsProgressBarIndeterminate = false,
                    },
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(2),
                    },
                };

                await LocalNotificationCenter.Current.Show(request);
                Preferences.Default.Set(PrefsKey, today);

                Debug.WriteLine($"[Monitor] Notified — {unsynced}/{captured} unsynced ({ratio * 100:F0}%)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Monitor] {ex.Message}");
            }
        }
    }
}

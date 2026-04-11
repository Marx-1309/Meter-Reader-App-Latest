namespace MeterReaderApp.Services
{
    /// <summary>
    /// Syncs BS_BillingLocation from the server API into the local SQLite table
    /// and provides the location list to the rest of the app.
    /// Called once at database initialisation; uses the cached SQLite table thereafter.
    /// </summary>
    public class BillingLocationService
    {
        private readonly DbContext          _db;
        private readonly HttpClient         _http = new();
        private readonly IConnectivity      _connectivity;

        public BillingLocationService(DbContext db, IConnectivity connectivity)
        {
            _db           = db;
            _connectivity = connectivity;
        }

        // ── Sync from API → SQLite ────────────────────────────────────────────

        /// <summary>
        /// Downloads the latest billing-location list from the server and upserts
        /// it into the local BillingLocation SQLite table.
        /// Safe to call multiple times — skips download when offline.
        /// </summary>
        public async Task SyncFromApiAsync()
        {
            try
            {
                if (_db.Database is null) return;
                if (_connectivity.NetworkAccess != NetworkAccess.Internet) return;

                var response = await _http.GetAsync(API_URL_s.Constants.GetLocation);
                if (!response.IsSuccessStatusCode) return;

                var serverList = await response.Content
                    .ReadFromJsonAsync<List<BillingLocation>>();

                if (serverList is null || serverList.Count == 0) return;

                // Delete old cached records and replace with fresh server data
                await _db.Database.DeleteAllAsync<BillingLocation>();
                await _db.Database.InsertAllAsync(serverList);

                Debug.WriteLine($"[BillingLocation] Synced {serverList.Count} location(s) from API");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BillingLocation] Sync error: {ex.Message}");
            }
        }

        // ── Read from SQLite ──────────────────────────────────────────────────

        /// <summary>
        /// Returns all location names from the local SQLite table.
        /// Falls back to a safe empty array on any error.
        /// </summary>
        public async Task<string[]> GetLocationNamesAsync()
        {
            try
            {
                if (_db.Database is null) return Array.Empty<string>();

                var rows = await _db.Database.Table<BillingLocation>()
                    .OrderBy(l => l.Location)
                    .ToListAsync();

                if (rows.Count == 0)
                    return Array.Empty<string>();

                return rows
                    .Where(l => !string.IsNullOrWhiteSpace(l.Location))
                    .Select(l => l.Location!.Trim())
                    .ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BillingLocation] GetLocationNames error: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Returns full BillingLocation objects (id + name) for display or selection.
        /// </summary>
        public async Task<List<BillingLocation>> GetLocationsAsync()
        {
            try
            {
                if (_db.Database is null) return new();

                return await _db.Database.Table<BillingLocation>()
                    .OrderBy(l => l.Location)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BillingLocation] GetLocations error: {ex.Message}");
                return new();
            }
        }
    }
}

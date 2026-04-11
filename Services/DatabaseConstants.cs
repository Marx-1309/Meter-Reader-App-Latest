namespace MeterReaderApp.Services
{
    public class DatabaseConstants
    {
        public const string DatabaseFileName = "OPUWODB.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create    |
            SQLite.SQLiteOpenFlags.SharedCache;

        /// <summary>
        /// Uses MAUI FileSystem.AppDataDirectory — guaranteed writable on all
        /// platforms. Environment.SpecialFolder.LocalApplicationData can resolve
        /// to an inaccessible path on some Android OEM devices.
        /// </summary>
        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
    }
}

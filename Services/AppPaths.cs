namespace InventurApp.Services
{
    public static class AppPaths
    {
        public static string DataDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AlexInventur");

        public static string ExportDirectory => Path.Combine(DataDirectory, "Exports");
        public static string BackupDirectory => Path.Combine(DataDirectory, "Backups");
        public static string LogDirectory => Path.Combine(DataDirectory, "Logs");
        public static string DocumentDirectory => Path.Combine(DataDirectory, "Documents");
        public static string ScanDirectory => Path.Combine(DocumentDirectory, "Scans");
        public static string AttachmentDirectory => Path.Combine(DocumentDirectory, "Attachments");
        public static string DatabaseFile => Path.Combine(DataDirectory, "inventur.db");
        public static string SettingsFile => Path.Combine(DataDirectory, "settings.json");

        public static void EnsureAll()
        {
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(ExportDirectory);
            Directory.CreateDirectory(BackupDirectory);
            Directory.CreateDirectory(LogDirectory);
            Directory.CreateDirectory(DocumentDirectory);
            Directory.CreateDirectory(ScanDirectory);
            Directory.CreateDirectory(AttachmentDirectory);
        }
    }
}

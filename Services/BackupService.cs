namespace InventurApp.Services
{
    public class BackupService
    {
        public string CreateBackup()
        {
            AppPaths.EnsureAll();
            if (!File.Exists(AppPaths.DatabaseFile))
                throw new FileNotFoundException("Die SQLite-Datenbank wurde noch nicht angelegt.");

            var backupFile = Path.Combine(AppPaths.BackupDirectory, $"inventur_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
            File.Copy(AppPaths.DatabaseFile, backupFile, overwrite: false);
            CleanupOldBackups(20);
            return backupFile;
        }

        public void AutoBackupIfNeeded()
        {
            AppPaths.EnsureAll();
            if (!File.Exists(AppPaths.DatabaseFile)) return;
            var latest = Directory.GetFiles(AppPaths.BackupDirectory, "inventur_backup_*.db")
                .OrderByDescending(File.GetCreationTime)
                .FirstOrDefault();
            if (latest == null || File.GetCreationTime(latest) < DateTime.Now.AddDays(-1))
                CreateBackup();
        }

        private static void CleanupOldBackups(int keep)
        {
            var files = Directory.GetFiles(AppPaths.BackupDirectory, "inventur_backup_*.db")
                .OrderByDescending(File.GetCreationTime)
                .Skip(keep);
            foreach (var file in files)
            {
                try { File.Delete(file); } catch { }
            }
        }
    }
}

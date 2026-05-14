using System.Text;

namespace InventurApp.Services
{
    public static class AppLogger
    {
        private static readonly object Sync = new();

        public static void Info(string message) => Write("INFO", message, null);
        public static void Warn(string message) => Write("WARN", message, null);
        public static void Error(Exception ex, string message = "Unerwarteter Fehler") => Write("ERROR", message, ex);
        public static void Audit(string message) => Write("AUDIT", message, null);

        private static void Write(string level, string message, Exception? ex)
        {
            try
            {
                AppPaths.EnsureAll();
                var file = Path.Combine(AppPaths.LogDirectory, $"alexinventur_{DateTime.Now:yyyyMMdd}.log");
                var sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.Append(" [").Append(level).Append("] ").AppendLine(message);
                if (ex != null) sb.AppendLine(ex.ToString());

                lock (Sync)
                    File.AppendAllText(file, sb.ToString());
            }
            catch
            {
                // Logging darf die App nie zum Absturz bringen.
            }
        }
    }
}

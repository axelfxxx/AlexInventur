using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace InventurApp.Services
{
    public static class AppInfoService
    {
        public static string ProductName => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Alex Inventur";

        public static string CompanyName => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "AlexInventur";

        public static string Description => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "Inventur-, Geräte- und Dokumentenverwaltung";

        public static Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);

        public static string CurrentVersionText
        {
            get
            {
                var version = CurrentVersion;
                return version.Revision > 0
                    ? version.ToString()
                    : $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        public static string ExecutablePath => Environment.ProcessPath ?? Application.ExecutablePath;

        public static string InstallDirectory => AppContext.BaseDirectory;

        public static string DataDirectory => AppPaths.DataDirectory;

        public static string BuildConfiguration
        {
            get
            {
#if DEBUG
                return "Debug";
#else
                return "Release";
#endif
            }
        }

        public static void OpenDataDirectory()
        {
            AppPaths.EnsureAll();
            Process.Start(new ProcessStartInfo(AppPaths.DataDirectory) { UseShellExecute = true });
        }
    }
}

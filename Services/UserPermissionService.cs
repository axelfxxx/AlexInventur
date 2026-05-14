using InventurApp.Models;

namespace InventurApp.Services
{
    public static class UserPermissionService
    {
        public const string RoleAdministrator = "Administrator";
        public const string RoleBenutzer = "Benutzer";
        public const string RoleInventur = "Inventur";
        public const string RoleNurLesen = "Nur Lesen";

        public static readonly string[] AvailableRoles =
        {
            RoleAdministrator,
            RoleBenutzer,
            RoleInventur,
            RoleNurLesen
        };

        public static bool IsAdmin(UserAccount? user) => HasRole(user, RoleAdministrator);

        public static bool CanCreateArticle(UserAccount? user) => IsAdmin(user) || HasRole(user, RoleBenutzer) || HasRole(user, RoleInventur);
        public static bool CanEditArticle(UserAccount? user) => IsAdmin(user) || HasRole(user, RoleBenutzer) || HasRole(user, RoleInventur);
        public static bool CanDeleteArticle(UserAccount? user) => IsAdmin(user);
        public static bool CanImport(UserAccount? user) => IsAdmin(user) || HasRole(user, RoleBenutzer);
        public static bool CanExport(UserAccount? user) => IsAdmin(user) || HasRole(user, RoleBenutzer) || HasRole(user, RoleInventur) || HasRole(user, RoleNurLesen);
        public static bool CanUseScanner(UserAccount? user) => IsAdmin(user) || HasRole(user, RoleBenutzer) || HasRole(user, RoleInventur);
        public static bool CanOpenDocuments(UserAccount? user) => IsAdmin(user) || HasRole(user, RoleBenutzer) || HasRole(user, RoleInventur) || HasRole(user, RoleNurLesen);
        public static bool CanManageDocuments(UserAccount? user) => IsAdmin(user) || HasRole(user, RoleBenutzer) || HasRole(user, RoleInventur);
        public static bool CanOpenSettings(UserAccount? user) => IsAdmin(user);
        public static bool CanManageUsers(UserAccount? user) => IsAdmin(user);
        public static bool CanCreateBackup(UserAccount? user) => IsAdmin(user);
        public static bool CanOpenStatistics(UserAccount? user) => true;

        public static string ExplainRole(string role) => role switch
        {
            RoleAdministrator => "Vollzugriff inkl. Benutzer, Einstellungen, Löschen und Backups.",
            RoleBenutzer => "Artikel bearbeiten, importieren, exportieren und scannen. Keine Benutzer-/Systemverwaltung.",
            RoleInventur => "Inventur-Arbeiten und Scanner-Nutzung. Kein Löschen, kein Import, keine Einstellungen.",
            RoleNurLesen => "Nur anzeigen, suchen, Statistik und Export/PDF. Keine Änderungen.",
            _ => "Eingeschränkter Standardzugriff."
        };

        private static bool HasRole(UserAccount? user, string role) =>
            string.Equals(user?.Role, role, StringComparison.OrdinalIgnoreCase);
    }
}

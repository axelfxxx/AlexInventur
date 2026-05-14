using InventurApp.Models;
using InventurApp.Persistence;
using InventurApp.Security;

namespace InventurApp.Services
{
    public class BenutzerService
    {
        private readonly SqliteRepository _repo = new();

        public UserAccount? CurrentUser { get; private set; }

        public bool Login(string username, string password)
        {
            var user = _repo.LadeBenutzer().FirstOrDefault(u =>
                string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user == null || !user.IsActive)
            {
                AppLogger.Audit($"Login abgelehnt: Benutzer '{username}' nicht vorhanden oder deaktiviert.");
                return false;
            }

            if (!PasswordHasher.Verify(password, user.PasswordHash))
            {
                user.FailedLoginCount++;
                _repo.UpdateBenutzer(user);
                AppLogger.Audit($"Fehlgeschlagener Login für '{user.Username}'. Fehlversuche: {user.FailedLoginCount}");
                return false;
            }

            user.LastLoginAt = DateTime.Now;
            user.FailedLoginCount = 0;

            if (PasswordHasher.NeedsRehash(user.PasswordHash))
            {
                user.PasswordHash = PasswordHasher.Hash(password);
                user.PasswordChangedAt = DateTime.Now;
            }

            _repo.UpdateBenutzer(user);
            CurrentUser = user;
            AppLogger.Audit($"Login erfolgreich: {user.Username} ({user.Role})");
            return true;
        }


        public bool AutoLogin(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var user = _repo.LadeBenutzer().FirstOrDefault(u =>
                string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user == null || !user.IsActive)
            {
                AppLogger.Audit($"Auto-Login abgelehnt: Benutzer '{username}' nicht vorhanden oder deaktiviert.");
                return false;
            }

            user.LastLoginAt = DateTime.Now;
            user.FailedLoginCount = 0;
            _repo.UpdateBenutzer(user);
            CurrentUser = user;
            AppLogger.Audit($"Auto-Login erfolgreich: {user.Username} ({user.Role})");
            return true;
        }

        public void Logout()
        {
            if (CurrentUser != null)
                AppLogger.Audit($"Logout: {CurrentUser.Username}");
            CurrentUser = null;
        }

        public List<UserAccount> GetUsers() => _repo.LadeBenutzer();

        public bool IsAdmin => UserPermissionService.IsAdmin(CurrentUser);

        public void CreateUser(string username, string password, string role = UserPermissionService.RoleBenutzer)
        {
            username = username.Trim();
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Benutzername darf nicht leer sein.");
            ValidatePassword(password);
            if (_repo.LadeBenutzer().Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Benutzername existiert bereits.");

            var newUser = new UserAccount
            {
                Username = username,
                PasswordHash = PasswordHasher.Hash(password),
                Role = UserPermissionService.AvailableRoles.Contains(role) ? role : UserPermissionService.RoleBenutzer,
                IsActive = true,
                PasswordChangedAt = DateTime.Now
            };

            _repo.SpeichereBenutzer(newUser);
            AppLogger.Audit($"Benutzer angelegt: {username} ({newUser.Role}) durch {CurrentUser?.Username ?? "System"}");
        }

        public void ChangePassword(Guid userId, string newPassword)
        {
            ValidatePassword(newPassword);
            var user = GetRequiredUser(userId);
            user.PasswordHash = PasswordHasher.Hash(newPassword);
            user.PasswordChangedAt = DateTime.Now;
            user.FailedLoginCount = 0;
            _repo.UpdateBenutzer(user);
            AppLogger.Audit($"Passwort geändert für {user.Username} durch {CurrentUser?.Username ?? "System"}");
        }

        public void SetUserActive(Guid userId, bool isActive)
        {
            var user = GetRequiredUser(userId);
            if (CurrentUser?.Id == user.Id && !isActive)
                throw new InvalidOperationException("Du kannst deinen eigenen Benutzer nicht deaktivieren.");

            user.IsActive = isActive;
            _repo.UpdateBenutzer(user);
            AppLogger.Audit($"Benutzer {(isActive ? "aktiviert" : "deaktiviert")}: {user.Username} durch {CurrentUser?.Username ?? "System"}");
        }

        public void ChangeRole(Guid userId, string role)
        {
            if (!UserPermissionService.AvailableRoles.Contains(role))
                throw new ArgumentException("Ungültige Rolle.");

            var user = GetRequiredUser(userId);
            if (CurrentUser?.Id == user.Id && !string.Equals(role, UserPermissionService.RoleAdministrator, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Du kannst dir selbst nicht die Administratorrolle entziehen.");

            user.Role = role;
            _repo.UpdateBenutzer(user);
            AppLogger.Audit($"Rolle geändert: {user.Username} => {role} durch {CurrentUser?.Username ?? "System"}");
        }

        private UserAccount GetRequiredUser(Guid userId) =>
            _repo.LadeBenutzer().FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("Benutzer wurde nicht gefunden.");

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException("Passwort muss mindestens 8 Zeichen lang sein.");
        }
    }
}

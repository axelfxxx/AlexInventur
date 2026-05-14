using InventurApp.Models;
using InventurApp.Security;
using InventurApp.Services;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace InventurApp.Persistence
{
    public class SqliteRepository
    {
        private readonly string _connectionString;

        public SqliteRepository()
        {
            AppPaths.EnsureAll();
            _connectionString = $"Data Source={AppPaths.DatabaseFile}";
            Initialize();
        }

        private void Initialize()
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS Artikel (
    Id TEXT PRIMARY KEY,
    Artikelnummer TEXT NOT NULL UNIQUE,
    Bezeichnung TEXT NOT NULL,
    Lagerort TEXT NOT NULL,
    SollMenge INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS Benutzer (
    Id TEXT PRIMARY KEY,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    PasswordChangedAt TEXT NULL,
    LastLoginAt TEXT NULL,
    FailedLoginCount INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS Dokumente (
    Id TEXT PRIMARY KEY,
    Titel TEXT NOT NULL,
    Kategorie TEXT NOT NULL,
    DateiPfad TEXT NOT NULL,
    DateiName TEXT NOT NULL,
    Artikelnummer TEXT NULL,
    Quelle TEXT NOT NULL,
    ErstelltAm TEXT NOT NULL,
    ErstelltVon TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS ArtikelFelder (
    ArtikelId TEXT NOT NULL,
    FeldName TEXT NOT NULL,
    FeldWert TEXT NOT NULL,
    PRIMARY KEY (ArtikelId, FeldName),
    FOREIGN KEY (ArtikelId) REFERENCES Artikel(Id) ON DELETE CASCADE
);";
            command.ExecuteNonQuery();
            EnsureBenutzerColumns(connection);

            using var countCommand = connection.CreateCommand();
            countCommand.CommandText = "SELECT COUNT(*) FROM Benutzer";
            var count = Convert.ToInt32(countCommand.ExecuteScalar());
            if (count == 0)
            {
                InsertUser(connection, new UserAccount
                {
                    Username = "admin",
                    PasswordHash = PasswordHasher.Hash("admin"),
                    Role = UserPermissionService.RoleAdministrator,
                    IsActive = true,
                    PasswordChangedAt = DateTime.Now
                });
            }

            MigrateJsonIfDatabaseEmpty(connection);
        }

        private static void EnsureBenutzerColumns(SqliteConnection connection)
        {
            AddColumnIfMissing(connection, "Benutzer", "IsActive", "INTEGER NOT NULL DEFAULT 1");
            AddColumnIfMissing(connection, "Benutzer", "PasswordChangedAt", "TEXT NULL");
            AddColumnIfMissing(connection, "Benutzer", "LastLoginAt", "TEXT NULL");
            AddColumnIfMissing(connection, "Benutzer", "FailedLoginCount", "INTEGER NOT NULL DEFAULT 0");
        }

        private static void AddColumnIfMissing(SqliteConnection connection, string table, string column, string definition)
        {
            using var check = connection.CreateCommand();
            check.CommandText = $"PRAGMA table_info({table})";
            using var reader = check.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
            alter.ExecuteNonQuery();
        }

        private static void MigrateJsonIfDatabaseEmpty(SqliteConnection connection)
        {
            using var countArtikel = connection.CreateCommand();
            countArtikel.CommandText = "SELECT COUNT(*) FROM Artikel";
            if (Convert.ToInt32(countArtikel.ExecuteScalar()) > 0)
                return;

            var oldJson = Path.Combine(AppPaths.DataDirectory, "artikel.json");
            if (!File.Exists(oldJson))
                return;

            try
            {
                var json = File.ReadAllText(oldJson);
                var artikel = JsonSerializer.Deserialize<List<Artikel>>(json) ?? new List<Artikel>();
                foreach (var item in artikel)
                {
                    using var insert = connection.CreateCommand();
                    insert.CommandText = @"INSERT OR IGNORE INTO Artikel (Id, Artikelnummer, Bezeichnung, Lagerort, SollMenge)
VALUES ($id, $nummer, $bezeichnung, $lagerort, $menge)";
                    insert.Parameters.AddWithValue("$id", item.Id.ToString());
                    insert.Parameters.AddWithValue("$nummer", item.Artikelnummer);
                    insert.Parameters.AddWithValue("$bezeichnung", item.Bezeichnung);
                    insert.Parameters.AddWithValue("$lagerort", item.Lagerort ?? string.Empty);
                    insert.Parameters.AddWithValue("$menge", item.SollMenge);
                    insert.ExecuteNonQuery();
                }

                File.Copy(oldJson, Path.Combine(AppPaths.DataDirectory, $"artikel_migriert_{DateTime.Now:yyyyMMdd_HHmmss}.json"), overwrite: false);
                AppLogger.Info("JSON-Daten wurden nach SQLite migriert.");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "JSON-Migration nach SQLite fehlgeschlagen.");
            }
        }

        private SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public List<Artikel> LadeArtikel()
        {
            var result = new List<Artikel>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Artikelnummer, Bezeichnung, Lagerort, SollMenge FROM Artikel ORDER BY Artikelnummer";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Artikel
                {
                    Id = Guid.TryParse(reader.GetString(0), out var id) ? id : Guid.NewGuid(),
                    Artikelnummer = reader.GetString(1),
                    Bezeichnung = reader.GetString(2),
                    Lagerort = reader.GetString(3),
                    SollMenge = reader.GetInt32(4),
                    CustomFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                });
            }
            reader.Close();

            using var fields = connection.CreateCommand();
            fields.CommandText = "SELECT ArtikelId, FeldName, FeldWert FROM ArtikelFelder";
            using var fieldReader = fields.ExecuteReader();
            while (fieldReader.Read())
            {
                var artikelIdText = fieldReader.GetString(0);
                var artikel = result.FirstOrDefault(a => string.Equals(a.Id.ToString(), artikelIdText, StringComparison.OrdinalIgnoreCase));
                if (artikel != null)
                    artikel.CustomFields[fieldReader.GetString(1)] = fieldReader.GetString(2);
            }

            return result;
        }

        public void SpeichereArtikel(IEnumerable<Artikel> artikel)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            using var delete = connection.CreateCommand();
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM ArtikelFelder";
            delete.ExecuteNonQuery();

            delete.CommandText = "DELETE FROM Artikel";
            delete.ExecuteNonQuery();

            foreach (var item in artikel)
            {
                using var insert = connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = @"INSERT INTO Artikel (Id, Artikelnummer, Bezeichnung, Lagerort, SollMenge)
VALUES ($id, $nummer, $bezeichnung, $lagerort, $menge)";
                insert.Parameters.AddWithValue("$id", item.Id.ToString());
                insert.Parameters.AddWithValue("$nummer", item.Artikelnummer);
                insert.Parameters.AddWithValue("$bezeichnung", item.Bezeichnung);
                insert.Parameters.AddWithValue("$lagerort", item.Lagerort ?? string.Empty);
                insert.Parameters.AddWithValue("$menge", item.SollMenge);
                insert.ExecuteNonQuery();

                if (item.CustomFields != null)
                {
                    foreach (var feld in item.CustomFields.Where(f => !string.IsNullOrWhiteSpace(f.Key)))
                    {
                        using var insertField = connection.CreateCommand();
                        insertField.Transaction = transaction;
                        insertField.CommandText = @"INSERT OR REPLACE INTO ArtikelFelder (ArtikelId, FeldName, FeldWert)
VALUES ($artikelId, $feldName, $feldWert)";
                        insertField.Parameters.AddWithValue("$artikelId", item.Id.ToString());
                        insertField.Parameters.AddWithValue("$feldName", feld.Key.Trim());
                        insertField.Parameters.AddWithValue("$feldWert", feld.Value ?? string.Empty);
                        insertField.ExecuteNonQuery();
                    }
                }
            }
            transaction.Commit();
        }

        public List<UserAccount> LadeBenutzer()
        {
            var result = new List<UserAccount>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Username, PasswordHash, Role, CreatedAt, IsActive, PasswordChangedAt, LastLoginAt, FailedLoginCount FROM Benutzer ORDER BY Username";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new UserAccount
                {
                    Id = Guid.TryParse(reader.GetString(0), out var id) ? id : Guid.NewGuid(),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3),
                    CreatedAt = DateTime.TryParse(reader.GetString(4), out var created) ? created : DateTime.Now,
                    IsActive = reader.GetInt32(5) == 1,
                    PasswordChangedAt = reader.IsDBNull(6) ? null : DateTime.TryParse(reader.GetString(6), out var pwdChanged) ? pwdChanged : null,
                    LastLoginAt = reader.IsDBNull(7) ? null : DateTime.TryParse(reader.GetString(7), out var lastLogin) ? lastLogin : null,
                    FailedLoginCount = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                });
            }
            return result;
        }

        public void SpeichereBenutzer(UserAccount user)
        {
            using var connection = OpenConnection();
            InsertUser(connection, user);
        }

        public void UpdateBenutzer(UserAccount user)
        {
            using var connection = OpenConnection();
            InsertUser(connection, user);
        }

        private static void InsertUser(SqliteConnection connection, UserAccount user)
        {
            using var insert = connection.CreateCommand();
            insert.CommandText = @"INSERT OR REPLACE INTO Benutzer (Id, Username, PasswordHash, Role, CreatedAt, IsActive, PasswordChangedAt, LastLoginAt, FailedLoginCount)
VALUES ($id, $username, $hash, $role, $createdAt, $isActive, $passwordChangedAt, $lastLoginAt, $failedLoginCount)";
            insert.Parameters.AddWithValue("$id", user.Id.ToString());
            insert.Parameters.AddWithValue("$username", user.Username);
            insert.Parameters.AddWithValue("$hash", user.PasswordHash);
            insert.Parameters.AddWithValue("$role", user.Role);
            insert.Parameters.AddWithValue("$createdAt", user.CreatedAt.ToString("O"));
            insert.Parameters.AddWithValue("$isActive", user.IsActive ? 1 : 0);
            insert.Parameters.AddWithValue("$passwordChangedAt", user.PasswordChangedAt?.ToString("O") ?? (object)DBNull.Value);
            insert.Parameters.AddWithValue("$lastLoginAt", user.LastLoginAt?.ToString("O") ?? (object)DBNull.Value);
            insert.Parameters.AddWithValue("$failedLoginCount", user.FailedLoginCount);
            insert.ExecuteNonQuery();
        }


        public List<DocumentRecord> LadeDokumente()
        {
            var result = new List<DocumentRecord>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Titel, Kategorie, DateiPfad, DateiName, Artikelnummer, Quelle, ErstelltAm, ErstelltVon FROM Dokumente ORDER BY ErstelltAm DESC";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new DocumentRecord
                {
                    Id = Guid.TryParse(reader.GetString(0), out var id) ? id : Guid.NewGuid(),
                    Titel = reader.GetString(1),
                    Kategorie = reader.GetString(2),
                    DateiPfad = reader.GetString(3),
                    DateiName = reader.GetString(4),
                    Artikelnummer = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Quelle = reader.GetString(6),
                    ErstelltAm = DateTime.TryParse(reader.GetString(7), out var erstellt) ? erstellt : DateTime.Now,
                    ErstelltVon = reader.GetString(8)
                });
            }
            return result;
        }

        public void SpeichereDokument(DocumentRecord document)
        {
            using var connection = OpenConnection();
            using var insert = connection.CreateCommand();
            insert.CommandText = @"INSERT OR REPLACE INTO Dokumente (Id, Titel, Kategorie, DateiPfad, DateiName, Artikelnummer, Quelle, ErstelltAm, ErstelltVon)
VALUES ($id, $titel, $kategorie, $dateiPfad, $dateiName, $artikelnummer, $quelle, $erstelltAm, $erstelltVon)";
            insert.Parameters.AddWithValue("$id", document.Id.ToString());
            insert.Parameters.AddWithValue("$titel", document.Titel);
            insert.Parameters.AddWithValue("$kategorie", document.Kategorie);
            insert.Parameters.AddWithValue("$dateiPfad", document.DateiPfad);
            insert.Parameters.AddWithValue("$dateiName", document.DateiName);
            insert.Parameters.AddWithValue("$artikelnummer", string.IsNullOrWhiteSpace(document.Artikelnummer) ? (object)DBNull.Value : document.Artikelnummer);
            insert.Parameters.AddWithValue("$quelle", document.Quelle);
            insert.Parameters.AddWithValue("$erstelltAm", document.ErstelltAm.ToString("O"));
            insert.Parameters.AddWithValue("$erstelltVon", document.ErstelltVon);
            insert.ExecuteNonQuery();
        }

        public void LoescheDokument(Guid id)
        {
            using var connection = OpenConnection();
            using var delete = connection.CreateCommand();
            delete.CommandText = "DELETE FROM Dokumente WHERE Id = $id";
            delete.Parameters.AddWithValue("$id", id.ToString());
            delete.ExecuteNonQuery();
        }

        public static string HashPassword(string password) => PasswordHasher.Hash(password);
    }
}

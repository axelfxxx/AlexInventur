Alex Inventur - Erweiterungspaket

Umgesetzte Erweiterungen:
- Benutzerverwaltung mit Login (Standardbenutzer: admin / admin)
- SQLite statt JSON für Artikel und Benutzer
- Barcode-Scanner-Unterstützung per Tastatur-Scanner / Eingabefeld
- PDF-Export der Artikelliste
- Statistikfenster mit Kennzahlen
- automatische Backups der SQLite-Datenbank
- manueller Backup-Button
- Dark Mode Umschaltung
- Mehrfenster-System für Dateimanager und Statistiken
- Setup-/Publish-Grundlage über PublishProfile

Wichtig:
- Bitte ändere nach dem ersten Start das Standardpasswort admin/admin.
- Für SQLite wird Microsoft.Data.Sqlite per NuGet referenziert.
- Build/Restore in Visual Studio ausführen: Rechtsklick auf Projekt -> NuGet-Pakete wiederherstellen, danach Build.

Update Einstellungen:
- Neues Einstellungsfenster über Sidebar oder Toolbar.
- Scanner-Eingabe kann aktiviert/deaktiviert werden.
- Dark Mode wird dauerhaft in settings.json gespeichert.
- Mehrfenster-Modus steuert, ob Statistik/Dateimanager parallel oder modal geöffnet werden.
- Einstellungen liegen unter %LocalAppData%\AlexInventur\settings.json.

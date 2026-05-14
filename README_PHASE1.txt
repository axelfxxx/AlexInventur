Alex Inventur – Phase 1
=======================

Dieses Paket erweitert die letzte Version um die Phase-1-Grundlagen für produktionsnähere Nutzung.

Neu/verbessert:
- Passwort-Hashing mit PBKDF2 statt einfachem SHA256
- Legacy-SHA256-Hashes werden beim erfolgreichen Login automatisch auf PBKDF2 migriert
- Benutzer können aktiviert/deaktiviert werden
- Rollen können in der Benutzerverwaltung geändert werden
- Administratoren können Passwörter zurücksetzen
- Login-Historie: letzter Login und Fehlversuche werden gespeichert
- globale Fehlerbehandlung in Program.cs
- App-Logging und Audit-Logging unter %LocalAppData%\AlexInventur\Logs
- SQLite-Migration ergänzt neue Benutzer-Spalten automatisch
- Benutzerverwaltung optisch und funktional erweitert

Hinweis:
- Bestehender Standardlogin admin/admin bleibt aus Kompatibilitätsgründen möglich.
- Neue und zurückgesetzte Passwörter müssen mindestens 8 Zeichen haben.
- Nach dem ersten Start sollte das Admin-Passwort geändert werden.

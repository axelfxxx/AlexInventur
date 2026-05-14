Alex Inventur – Installer & AutoUpdate-Vorbereitung
====================================================

Neu in dieser Version
---------------------
- App-Version im Projekt hinterlegt: 1.0.0
- neues Fenster: Version & Updates
- Update-Manifest per HTTPS-URL oder lokalem/Netzwerk-Dateipfad möglich
- automatische Update-Prüfung beim Start optional
- Datenordner bleibt sauber getrennt unter %LocalAppData%\AlexInventur
- Publish-Profil: Properties\PublishProfiles\WinX64SelfContained.pubxml
- Build-Script: Tools\build-release.ps1
- Inno-Setup-Vorlage: Installer\AlexInventur.iss
- Beispielmanifest: Releases\update.example.json

Release bauen
-------------
1. PowerShell im Projektordner öffnen.
2. Ausführen:
   Tools\build-release.ps1
3. Ergebnis liegt unter:
   publish\win-x64

Installer bauen
---------------
1. Inno Setup installieren.
2. Installer\AlexInventur.iss öffnen.
3. Kompilieren.
4. Setup-Datei landet unter:
   Releases\Installer

Update-Manifest
---------------
In den Einstellungen unter Version / Update kann eine Update-Quelle eingetragen werden.
Das kann sein:
- HTTPS-URL, z. B. https://server.example/update.json
- lokaler Pfad, z. B. C:\Releases\update.json
- Netzwerkpfad, z. B. \\Server\AlexInventur\update.json

Beispiel:
{
  "latestVersion": "1.0.1",
  "downloadUrl": "https://example.com/AlexInventur_Setup_1.0.1.exe",
  "publishedAt": "2026-05-13T18:00:00",
  "releaseNotes": "Korrekturen und Verbesserungen."
}

Hinweis
-------
Die Update-Funktion ist bewusst als sichere Vorbereitung gebaut: Die App prüft Versionen
und öffnet den Download. Sie ersetzt die laufende EXE nicht selbst. Das ist stabiler und
verhindert Update-Probleme durch gesperrte Programmdateien.

Zusatz
------
Für den konkreten Release-Ablauf siehe auch:
README_AUTOUPDATE_V1_READY.txt

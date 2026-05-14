Alex Inventur – AutoUpdater v1 Vorbereitung
===========================================

Status
------
Der AutoUpdater ist als sichere v1-Variante vorbereitet:

- Die App prüft beim Start optional ein Update-Manifest.
- Das Manifest kann eine HTTPS-URL, ein lokaler Pfad oder ein Netzwerkpfad sein.
- Wenn eine neuere Version verfügbar ist, fragt die App nach und öffnet den Download.
- Die laufende EXE wird bewusst nicht im Hintergrund ersetzt. Das ist für v1 stabiler.
- Der bestehende Inno-Setup-Installer bleibt der Update-Mechanismus.

Wichtige Dateien
----------------
- Services\UpdateService.cs
- Forms\UpdateInfoForm.cs
- Models\AppSettings.cs
- Program.cs
- Releases\update.example.json
- Releases\update.json
- Tools\build-release.ps1
- Tools\create-update-manifest.ps1
- Installer\AlexInventur.iss

So veröffentlichst du eine neue Version
---------------------------------------
1. Version in AlexInventur.csproj erhöhen, z. B. von 1.0.0 auf 1.0.1:

   <Version>1.0.1</Version>
   <AssemblyVersion>1.0.1.0</AssemblyVersion>
   <FileVersion>1.0.1.0</FileVersion>
   <InformationalVersion>1.0.1</InformationalVersion>

2. Installer\AlexInventur.iss anpassen:

   #define MyAppVersion "1.0.1"

3. Release bauen:

   Tools\build-release.ps1

4. Inno Setup öffnen und Installer\AlexInventur.iss kompilieren.

5. Die erzeugte Setup-Datei hochladen oder in einen Netzwerkordner legen.

6. Manifest erzeugen, Beispiel Netzwerkpfad:

   Tools\create-update-manifest.ps1 -LatestVersion "1.0.1" -DownloadUrl "\\Server\AlexInventur\AlexInventur_Setup_1.0.1.exe" -ReleaseNotes "Korrekturen und Verbesserungen."

   Beispiel HTTPS:

   Tools\create-update-manifest.ps1 -LatestVersion "1.0.1" -DownloadUrl "https://example.com/AlexInventur_Setup_1.0.1.exe" -ReleaseNotes "Korrekturen und Verbesserungen."

7. Die Datei Releases\update.json an die Update-Quelle legen.

8. In der App unter Einstellungen -> Version & Updates diese Manifest-Quelle eintragen.

Empfohlene Update-Quelle für den Anfang
---------------------------------------
Für Tests ist ein Netzwerkordner am einfachsten:

\\Server\AlexInventur\update.json
\\Server\AlexInventur\AlexInventur_Setup_1.0.1.exe

Für später ist HTTPS schöner:

https://deine-domain.de/alexinventur/update.json
https://deine-domain.de/alexinventur/AlexInventur_Setup_1.0.1.exe

Testablauf
----------
1. Installiere Version 1.0.0.
2. Erhöhe Projekt/Installer auf Version 1.0.1.
3. Baue das neue Setup.
4. Erstelle update.json mit latestVersion 1.0.1.
5. Starte die installierte 1.0.0.
6. Die App sollte melden: Update 1.0.1 ist verfügbar.

Hinweis
-------
Diese v1-Lösung ist absichtlich konservativ: Sie informiert und startet den Download/Installer.
Ein vollautomatischer Austausch der laufenden App kann später als v2-Updater ergänzt werden.

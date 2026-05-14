Alex Inventur - App-Icon & Branding
===================================

Neu in dieser Version:
- eigenes App-Icon unter Resources/AppIcon.ico
- PNG-Vorschau unter Resources/AppIcon.png
- Projektdatei nutzt ApplicationIcon
- Fenster übernehmen das Programm-Icon automatisch über ModernTheme.ApplyForm(...)
- neues Info-/Über-Fenster mit Produktname, Version, Build, Datenordner und Installationsordner
- zusätzlicher Info-Einstieg in der linken Navigation
- Einstellungen > Version / Update enthält jetzt auch den Über-Dialog
- Inno-Setup-Script nutzt SetupIconFile=..\Resources\AppIcon.ico

Wichtig beim Release:
1. In Visual Studio oder über Tools/build-release.ps1 publishen.
2. Danach Installer/AlexInventur.iss kompilieren.
3. Falls Inno Setup den Icon-Pfad nicht findet, prüfen, ob Resources/AppIcon.ico relativ zur .iss-Datei vorhanden ist.

Hinweis:
Das Icon ist ein generisches Alex-Inventur-Appsymbol. Es kann später jederzeit ersetzt werden, solange der Pfad Resources/AppIcon.ico gleich bleibt.

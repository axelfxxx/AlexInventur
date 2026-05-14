AlexInventur – Clean-Up-Paket

Änderungen:
- Dateimanager: Dokumente werden nur noch über DocumentService geladen, damit Dokumentdateien nicht doppelt erscheinen.
- DocumentService: UpdateDocument(...) ergänzt, damit Artikelzuordnung/Metadaten aktualisiert werden können, ohne Dokumente neu anzulegen.
- Dateimanager: Artikelzuordnung nutzt jetzt UpdateDocument statt RegisterScan + Delete.
- Artikelgrid: DataSource wird beim Filtern nicht mehr auf null gesetzt; das Grid nutzt eine stabile BindingSource/BindingList.
- Release-Paket bereinigt: keine .csproj.user-/pubxml.user-Dateien im ZIP.

Hinweis:
- In dieser Umgebung konnte kein dotnet build ausgeführt werden, weil das .NET SDK nicht installiert ist.

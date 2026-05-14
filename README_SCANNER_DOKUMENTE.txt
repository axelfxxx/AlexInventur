Alex Inventur – Scanner + Dokumentenverwaltung

Neu enthalten:
- Neues Fenster "Dokumentenverwaltung"
- Dokumente/Dateien importieren und zentral ablegen
- Dokumente optional einem Artikel zuordnen
- Kategorien wie Scan, Lieferschein, Rechnung, Inventurbeleg
- Dokumentensuche
- Dokumente direkt aus der App öffnen
- Dokumente aus Verwaltung entfernen, optional auch Datei löschen
- Scan-Button legt Scans automatisch im Dokumentenbereich ab
- Neue SQLite-Tabelle "Dokumente"
- Speicherorte:
  %LocalAppData%\AlexInventur\Documents\Scans
  %LocalAppData%\AlexInventur\Documents\Attachments

Wichtig:
Die Dokumentenverwaltung ist vollständig angebunden.
Die echte TWAIN-Gerätekommunikation ist weiterhin vorbereitet, aber noch nicht mit einer konkreten TWAIN/WIA-Bibliothek verbunden.
Der aktuelle Scan erzeugt deshalb einen Scan-Platzhalter in der Ablage und registriert ihn sauber in der Dokumentenverwaltung.

AlexInventur – Artikelbearbeitung, Auto-Login und flexible Feldnamen

Diese Version basiert auf AlexInventur_StatistikDashboardV2.

Neu:

1. Artikel editierbar
- Artikel können per Doppelklick geöffnet und bearbeitet werden.
- Zusätzlich gibt es einen Button "Bearbeiten" in der Toolbar.
- Direkte Tabellenbearbeitung ist aktiviert, inklusive automatischem Speichern nach der Zellbearbeitung.
- Soll-Menge wird direkt validiert und darf nicht negativ sein.
- Doppelte Artikelnummern werden beim Bearbeiten abgefangen.

2. Automatische Anmeldung
- In Einstellungen > Benutzer kann Auto-Login aktiviert werden.
- Ein aktiver Benutzer kann als Auto-Login-Benutzer ausgewählt werden.
- Es wird kein Passwort in den Einstellungen gespeichert.
- Hinweis: Auto-Login ist nur für vertrauenswürdige Einzelplatz-Rechner gedacht.

3. Flexible Feldnamen und Import-Mappings
- In Einstellungen > Felder / Import können die sichtbaren Feldnamen geändert werden.
- Die geänderten Feldnamen erscheinen im Artikel-Grid und im CSV-Export.
- Pro Feld können zusätzliche CSV-Aliase gepflegt werden.
- Der CSV-Import erkennt bekannte Aliase und sichtbare Feldnamen automatisch vor.

Interne Feldschlüssel bleiben stabil:
- Artikelnummer
- Bezeichnung
- Lagerort
- SollMenge

Dadurch bleibt die Datenbank kompatibel, auch wenn die sichtbaren Namen angepasst werden.

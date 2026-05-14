CSV-Gerätelisten-Import
=======================

Diese Version erweitert den CSV-Import für komplexe Hardware-/Gerätelisten.

Neu:
- CSV-Import ist nicht mehr auf Artikelnummer/Bezeichnung/Lagerort/Soll-Menge beschränkt.
- Unbekannte CSV-Spalten werden im Import-Mapping automatisch als Zusatzfelder vorgeschlagen.
- Standardfelder können weiterhin gemappt werden, z. B.:
  - Gerät -> Artikelnummer
  - Standort -> Lagerort
  - System Modell -> Bezeichnung
- Alle übrigen Spalten wie Seriennummer, BIOS, CPU, RAM, Disk, NICs usw. können als Zusatzfelder übernommen werden.
- Zusatzfelder werden pro Artikel in SQLite gespeichert.
- Neue Zusatzfelder werden optional in settings.json gemerkt und erscheinen beim nächsten Import/Export wieder.
- Das Hauptgrid zeigt gespeicherte Zusatzfelder dynamisch als zusätzliche Spalten an.
- CSV-Export enthält neben den Standardspalten auch die bekannten Zusatzfelder.

Hinweis:
Für echte Hardwarelisten ist "Gerät" als primäres Identifikationsfeld vorgesehen. Doppelte Gerätenamen aktualisieren bestehende Datensätze.

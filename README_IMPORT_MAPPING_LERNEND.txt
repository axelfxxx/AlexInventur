Alex Inventur – Lernender CSV-Import
====================================

Neu in dieser Version:

- Der CSV-Import zeigt weiterhin den Mapping-Dialog für die Spalten der eingelesenen Datei.
- Nicht erkannte CSV-Spalten können dort einem Zielfeld zugeordnet werden.
- Die Option "Diese Zuordnung für zukünftige Importe merken" speichert neue Spaltennamen automatisch als Import-Alias.
- Beim nächsten Import werden diese Spaltennamen automatisch erkannt.
- Ein Alias wird sauber auf genau ein Zielfeld umgehängt, falls der Benutzer ihn später anders zuordnet.

Beispiel:
CSV-Spalte "ItemCode" -> im Dialog als "Artikelnummer" wählen -> speichern.
Beim nächsten Import erkennt die App "ItemCode" automatisch als Artikelnummer.

Die Alias-Daten werden in den bestehenden Einstellungen gespeichert:
%LocalAppData%\AlexInventur\settings.json

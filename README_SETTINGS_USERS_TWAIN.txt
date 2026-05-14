Alex Inventur – Einstellungen: Benutzer + TWAIN

Neu in dieser Version:

1. Einstellungen erweitert
   - Registerkarten: Allgemein, Scanner / TWAIN, Benutzer
   - Dark Mode, Mehrfenster, Barcode-Scanner wie bisher
   - TWAIN aktivieren/deaktivieren
   - bevorzugte TWAIN-Quelle auswählen und speichern

2. Benutzerverwaltung
   - In Einstellungen > Benutzer über "Benutzerverwaltung öffnen"
   - Neue Benutzer mit Benutzername, Passwort und Rolle anlegen
   - Vorhandene Benutzer werden in einer Tabelle angezeigt
   - Passwörter werden weiterhin gehasht gespeichert

3. TWAIN-Auswahl
   - Quellen werden aus typischen Windows-TWAIN-Ordnern und Registry-Pfaden gesucht
   - Falls keine konkrete Quelle erkannt wird, bleibt "Standard-TWAIN-Quelle" auswählbar
   - Die Auswahl wird in settings.json gespeichert

Hinweis:
Diese Version speichert die TWAIN-Quelle bereits sauber in den Einstellungen.
Die eigentliche Bild-/Dokument-Scan-Funktion kann im nächsten Schritt angebunden werden,
z. B. über eine TWAIN-Library oder einen herstellerspezifischen Scanner-Treiber.

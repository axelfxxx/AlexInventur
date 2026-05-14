# Scanner finalisiert v1

- Der bisherige Platzhalter-Scan wurde durch einen echten Windows-WIA-Scan ersetzt.
- WIA-Scanner werden in den Einstellungen erkannt und auswählbar angezeigt.
- Wenn keine feste Quelle gewählt ist, öffnet die App den Standard-WIA-Scan-Dialog.
- Scans werden als JPG unter `%LocalAppData%\AlexInventur\Dokumente\Scans` gespeichert.
- Nach dem Scan wird automatisch ein Dokumentdatensatz erzeugt und in der Dokumentenverwaltung angezeigt.
- TWAIN-Quellen werden weiterhin erkannt, benötigen für echten TWAIN-Betrieb aber eine zusätzliche TWAIN-Bibliothek. WIA ist jetzt die direkte Windows-Scanneranbindung.

Hinweis: Einige Hersteller liefern Scanner nur über TWAIN und nicht sauber über WIA. In diesem Fall bitte im Treiber prüfen, ob WIA installiert/aktiviert ist.

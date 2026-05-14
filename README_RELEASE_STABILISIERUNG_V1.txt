Alex Inventur - Release-Stabilisierung v1

Enthaltene Stabilisierung:
- ModernTheme.SmallFont/BodyFont-Referenzen entfernt bzw. auf BaseFont stabilisiert.
- Automatische Update-Prüfung läuft erst nach Anzeige des Hauptfensters und blockiert den Start nicht mehr synchron.
- Update-Fenster speichert Einstellungen auch beim normalen Schließen des Fensters.
- Update-Buttons dürfen bei kleineren Fenstern umbrechen/scrollen.
- Release-Paket wird ohne .vs, bin, obj, *.user und temporäre Buildartefakte erzeugt.

Hinweis:
Der Build konnte in dieser Umgebung nicht ausgeführt werden, weil kein .NET SDK installiert ist. Bitte lokal mit Tools/build-release.ps1 oder dotnet publish testen.

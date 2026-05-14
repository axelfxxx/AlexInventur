AlexInventur - Grid Reentry Fix

Behoben:
- DataGridView InvalidOperationException:
  "Der Vorgang ist ungültig, da er einen Wiedereintrittsaufruf an die SetCurrentCellAddressCore-Funktion zur Folge hat."

Ursache:
- Die Artikeltabelle wurde während CellEndEdit/Validierung sofort neu gefiltert bzw. neu an DataSource gebunden.
- Zusätzlich löste das Aktualisieren des Lagerort-Filters SelectedIndexChanged aus und damit erneut ApplyArticleFilter().

Änderungen:
- DataSource-Refresh wird während Grid-Edit unterdrückt.
- Reload nach ungültiger Eingabe wird per BeginInvoke verzögert.
- Lagerort-Filter nutzt Suppress-Flag während Items/SelectedIndex geändert werden.
- ApplyArticleFilterSafe() verhindert Filterwechsel direkt im Edit-Modus.
- Grid erhält eine BindingList<Artikel> statt einer nackten List<Artikel>.

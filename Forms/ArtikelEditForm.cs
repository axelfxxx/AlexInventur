using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;

namespace InventurApp.Forms
{
    public partial class ArtikelEditForm : Form
    {
        private readonly ArtikelService _artikelService;

        public Artikel Artikel { get; private set; }

        public ArtikelEditForm(ArtikelService artikelService, Artikel? artikel = null)
        {
            InitializeComponent();

            ApplyModernDesign();

            _artikelService = artikelService ?? throw new ArgumentNullException(nameof(artikelService));
            Artikel = artikel ?? new Artikel();

            cmbBezeichnung.Items.AddRange(_artikelService.GetBezeichnungen().Cast<object>().ToArray());
            cmbBezeichnung.DropDownStyle = ComboBoxStyle.DropDown;

            if (string.IsNullOrWhiteSpace(Artikel.Artikelnummer))
            {
                txtArtikelnummer.Text = _artikelService.GetNaechsteArtikelnummer();
            }
            else
            {
                txtArtikelnummer.Text = Artikel.Artikelnummer;
            }

            txtArtikelnummer.ReadOnly = false;
            cmbBezeichnung.Text = Artikel.Bezeichnung;
            txtLagerort.Text = Artikel.Lagerort;
            numSollMenge.Value = Artikel.SollMenge;
        }

        private void ApplyModernDesign()
        {
            ModernTheme.ApplyForm(this);
            BackColor = ModernTheme.Background;
            Text = "Artikel bearbeiten";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            foreach (var label in new[] { lblArtikelnummer, lblBezeichnung, lblLagerort, lblSollMenge })
                ModernTheme.ApplyLabel(label, muted: true);

            foreach (Control input in new Control[] { txtArtikelnummer, cmbBezeichnung, txtLagerort, numSollMenge })
                ModernTheme.ApplyInput(input);

            btnOK.Text = "Speichern";
            btnAbbrechen.Text = "Abbrechen";
            ModernTheme.ApplyPrimaryButton(btnOK);
            ModernTheme.ApplySecondaryButton(btnAbbrechen);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Artikel.Artikelnummer = txtArtikelnummer.Text.Trim();
            Artikel.Bezeichnung = cmbBezeichnung.Text.Trim();
            Artikel.Lagerort = txtLagerort.Text.Trim();
            Artikel.SollMenge = (int)numSollMenge.Value;

            var fehler = _artikelService.Validate(Artikel, pruefeDuplikate: true);
            if (fehler.Count > 0)
            {
                MessageBox.Show(
                    string.Join(Environment.NewLine, fehler),
                    "Ungültige Eingabe",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _artikelService.EnsureBezeichnung(Artikel.Bezeichnung);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnAbbrechen_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

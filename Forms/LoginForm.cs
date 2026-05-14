using InventurApp.Services;
using InventurApp.UI;

namespace InventurApp.Forms
{
    public class LoginForm : Form
    {
        private readonly BenutzerService _benutzerService;
        private readonly TextBox _txtUser = new() { PlaceholderText = "Benutzername" };
        private readonly TextBox _txtPassword = new() { PlaceholderText = "Passwort", UseSystemPasswordChar = true };
        private readonly Button _btnLogin = new() { Text = "Anmelden" };
        private readonly Label _hint = new() { Text = "Standard: admin / admin", AutoSize = false, Height = 28, TextAlign = ContentAlignment.MiddleCenter };

        public LoginForm(BenutzerService benutzerService)
        {
            _benutzerService = benutzerService;
            Text = "Alex Inventur – Anmeldung";
            ClientSize = new Size(360, 250);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ModernTheme.ApplyForm(this);
            MinimumSize = new Size(360, 250);

            var title = new Label
            {
                Text = "Alex Inventur\nBitte anmelden",
                Dock = DockStyle.Top,
                Height = 78,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = ModernTheme.TitleFont,
                ForeColor = ModernTheme.Text
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(42, 8, 42, 20)
            };

            foreach (Control c in new Control[] { _txtUser, _txtPassword })
            {
                c.Width = 276;
                c.Height = 32;
                c.Margin = new Padding(0, 0, 0, 10);
                ModernTheme.ApplyInput(c);
            }

            _btnLogin.Width = 276;
            _btnLogin.Height = 38;
            _btnLogin.Margin = new Padding(0, 4, 0, 10);
            ModernTheme.ApplyPrimaryButton(_btnLogin);
            ModernTheme.ApplyLabel(_hint, muted: true);

            _btnLogin.Click += (_, _) => TryLogin();
            _txtPassword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) TryLogin(); };

            panel.Controls.Add(_txtUser);
            panel.Controls.Add(_txtPassword);
            panel.Controls.Add(_btnLogin);
            panel.Controls.Add(_hint);
            Controls.Add(panel);
            Controls.Add(title);
        }

        private void TryLogin()
        {
            if (_benutzerService.Login(_txtUser.Text.Trim(), _txtPassword.Text))
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }
            MessageBox.Show("Benutzername oder Passwort ist falsch.", "Anmeldung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

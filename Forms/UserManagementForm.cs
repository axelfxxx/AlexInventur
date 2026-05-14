using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;
using System.ComponentModel;

namespace InventurApp.Forms
{
    public class UserManagementForm : Form
    {
        private readonly BenutzerService _benutzerService;
        private readonly DataGridView _gridUsers = new();
        private readonly TextBox _txtUsername = new();
        private readonly TextBox _txtPassword = new();
        private readonly ComboBox _cmbRole = new();
        private readonly ComboBox _cmbSelectedRole = new();
        private readonly TextBox _txtNewPassword = new();
        private readonly Button _btnToggleActive = new();
        private readonly Button _btnSaveRole = new();
        private readonly Button _btnResetPassword = new();
        private readonly Label _lblStatus = new();
        private BindingList<UserDisplayRow> _users = new();

        public UserManagementForm(BenutzerService benutzerService)
        {
            _benutzerService = benutzerService;
            if (!UserPermissionService.CanManageUsers(_benutzerService.CurrentUser))
            {
                MessageBox.Show("Nur Administratoren dürfen Benutzer verwalten.", "Keine Berechtigung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BeginInvoke(new Action(Close));
                return;
            }
            InitializeLayout();
            LoadUsers();
        }

        private void InitializeLayout()
        {
            ModernTheme.ApplyForm(this);
            Text = "Benutzerverwaltung";
            ClientSize = new Size(980, 650);
            MinimumSize = new Size(860, 560);
            BackColor = ModernTheme.Background;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(24),
                BackColor = ModernTheme.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 138));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            Controls.Add(root);

            root.Controls.Add(ModernTheme.CreateTitleLabel("Benutzerverwaltung", "Benutzer anlegen, Rollen ändern, Konten sperren und Passwörter zurücksetzen"), 0, 0);

            var createCard = ModernTheme.CreateCardPanel();
            createCard.Dock = DockStyle.Fill;
            createCard.Padding = new Padding(18);
            root.Controls.Add(createCard, 0, 1);

            var createLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = ModernTheme.Surface
            };
            createLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
            createLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
            createLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            createLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            createLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            createLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            createCard.Controls.Add(createLayout);

            AddHeader(createLayout, "Benutzername", 0);
            AddHeader(createLayout, "Initialpasswort", 1);
            AddHeader(createLayout, "Rolle", 2);

            _txtUsername.Dock = DockStyle.Fill;
            _txtUsername.Margin = new Padding(0, 0, 10, 0);
            _txtUsername.PlaceholderText = "z. B. max.mustermann";
            ModernTheme.ApplyInput(_txtUsername);
            createLayout.Controls.Add(_txtUsername, 0, 1);

            _txtPassword.Dock = DockStyle.Fill;
            _txtPassword.Margin = new Padding(0, 0, 10, 0);
            _txtPassword.PlaceholderText = "mind. 8 Zeichen";
            _txtPassword.UseSystemPasswordChar = true;
            ModernTheme.ApplyInput(_txtPassword);
            createLayout.Controls.Add(_txtPassword, 1, 1);

            _cmbRole.Dock = DockStyle.Fill;
            _cmbRole.Margin = new Padding(0, 0, 10, 0);
            _cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbRole.Items.AddRange(UserPermissionService.AvailableRoles.Cast<object>().ToArray());
            _cmbRole.SelectedItem = UserPermissionService.RoleBenutzer;
            ModernTheme.ApplyInput(_cmbRole);
            createLayout.Controls.Add(_cmbRole, 2, 1);

            var btnAdd = new Button { Text = "＋ Anlegen", Dock = DockStyle.Fill, Height = 38 };
            btnAdd.Click += (_, _) => CreateUser();
            ModernTheme.ApplyPrimaryButton(btnAdd);
            createLayout.Controls.Add(btnAdd, 3, 1);

            var gridCard = ModernTheme.CreateCardPanel();
            gridCard.Dock = DockStyle.Fill;
            gridCard.Padding = new Padding(12);
            root.Controls.Add(gridCard, 0, 2);

            _gridUsers.Dock = DockStyle.Fill;
            _gridUsers.AutoGenerateColumns = false;
            _gridUsers.ReadOnly = true;
            _gridUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridUsers.MultiSelect = false;
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Benutzername", DataPropertyName = nameof(UserDisplayRow.Username), FillWeight = 22 });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rolle", DataPropertyName = nameof(UserDisplayRow.Role), FillWeight = 15 });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(UserDisplayRow.Status), FillWeight = 12 });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Letzter Login", DataPropertyName = nameof(UserDisplayRow.LastLoginAt), FillWeight = 18 });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fehlversuche", DataPropertyName = nameof(UserDisplayRow.FailedLogins), FillWeight = 12 });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rechte", DataPropertyName = nameof(UserDisplayRow.Permissions), FillWeight = 38 });
            _gridUsers.SelectionChanged += (_, _) => UpdateSelectedUserActions();
            ModernTheme.ApplyGrid(_gridUsers);
            gridCard.Controls.Add(_gridUsers);

            var actionsCard = ModernTheme.CreateCardPanel();
            actionsCard.Dock = DockStyle.Fill;
            actionsCard.Padding = new Padding(16);
            root.Controls.Add(actionsCard, 0, 3);

            var actions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 2,
                BackColor = ModernTheme.Surface
            };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            actionsCard.Controls.Add(actions);

            AddHeader(actions, "Rolle des markierten Benutzers", 0);
            AddHeader(actions, "", 1);
            AddHeader(actions, "Neues Passwort", 2);
            AddHeader(actions, "", 3);
            AddHeader(actions, "", 4);

            _cmbSelectedRole.Dock = DockStyle.Fill;
            _cmbSelectedRole.Margin = new Padding(0, 0, 10, 0);
            _cmbSelectedRole.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbSelectedRole.Items.AddRange(UserPermissionService.AvailableRoles.Cast<object>().ToArray());
            ModernTheme.ApplyInput(_cmbSelectedRole);
            actions.Controls.Add(_cmbSelectedRole, 0, 1);

            _btnSaveRole.Text = "Rolle speichern";
            _btnSaveRole.Dock = DockStyle.Fill;
            _btnSaveRole.Click += (_, _) => ChangeSelectedRole();
            ModernTheme.ApplySecondaryButton(_btnSaveRole);
            actions.Controls.Add(_btnSaveRole, 1, 1);

            _txtNewPassword.Dock = DockStyle.Fill;
            _txtNewPassword.Margin = new Padding(0, 0, 10, 0);
            _txtNewPassword.PlaceholderText = "mind. 8 Zeichen";
            _txtNewPassword.UseSystemPasswordChar = true;
            ModernTheme.ApplyInput(_txtNewPassword);
            actions.Controls.Add(_txtNewPassword, 2, 1);

            _btnResetPassword.Text = "Passwort setzen";
            _btnResetPassword.Dock = DockStyle.Fill;
            _btnResetPassword.Click += (_, _) => ResetSelectedPassword();
            ModernTheme.ApplySecondaryButton(_btnResetPassword);
            actions.Controls.Add(_btnResetPassword, 3, 1);

            _btnToggleActive.Text = "Sperren/Aktivieren";
            _btnToggleActive.Dock = DockStyle.Fill;
            _btnToggleActive.Click += (_, _) => ToggleSelectedUserActive();
            ModernTheme.ApplyDangerButton(_btnToggleActive);
            actions.Controls.Add(_btnToggleActive, 4, 1);

            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            ModernTheme.ApplyStatus(_lblStatus);
            root.Controls.Add(_lblStatus, 0, 4);
        }

        private static void AddHeader(TableLayoutPanel layout, string text, int column)
        {
            var label = new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            ModernTheme.ApplyLabel(label, muted: true);
            layout.Controls.Add(label, column, 0);
        }

        private void LoadUsers()
        {
            _users = new BindingList<UserDisplayRow>(_benutzerService.GetUsers()
                .Select(u => new UserDisplayRow
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    Status = u.IsActive ? "Aktiv" : "Gesperrt",
                    FailedLogins = u.FailedLoginCount,
                    LastLoginAt = u.LastLoginAt?.ToString("dd.MM.yyyy HH:mm") ?? "–",
                    Permissions = UserPermissionService.ExplainRole(u.Role)
                })
                .ToList());
            _gridUsers.DataSource = _users;
            _lblStatus.Text = $"{_users.Count} Benutzer vorhanden";
            UpdateSelectedUserActions();
        }

        private UserDisplayRow? SelectedUser => _gridUsers.CurrentRow?.DataBoundItem as UserDisplayRow;

        private void UpdateSelectedUserActions()
        {
            var user = SelectedUser;
            var hasUser = user != null;
            _btnSaveRole.Enabled = hasUser;
            _btnResetPassword.Enabled = hasUser;
            _btnToggleActive.Enabled = hasUser;
            _txtNewPassword.Enabled = hasUser;
            _cmbSelectedRole.Enabled = hasUser;

            if (user == null) return;
            _cmbSelectedRole.SelectedItem = user.Role;
            _btnToggleActive.Text = user.IsActive ? "Benutzer sperren" : "Benutzer aktivieren";
        }

        private void CreateUser()
        {
            try
            {
                _benutzerService.CreateUser(_txtUsername.Text, _txtPassword.Text, _cmbRole.SelectedItem?.ToString() ?? UserPermissionService.RoleBenutzer);
                _txtUsername.Clear();
                _txtPassword.Clear();
                LoadUsers();
                _lblStatus.Text = "Benutzer wurde angelegt ✔";
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Benutzer konnte nicht angelegt werden.");
                MessageBox.Show(ex.Message, "Benutzer anlegen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ChangeSelectedRole()
        {
            var user = SelectedUser;
            if (user == null) return;
            try
            {
                _benutzerService.ChangeRole(user.Id, _cmbSelectedRole.SelectedItem?.ToString() ?? UserPermissionService.RoleBenutzer);
                LoadUsers();
                _lblStatus.Text = "Rolle wurde gespeichert ✔";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Rolle ändern", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ResetSelectedPassword()
        {
            var user = SelectedUser;
            if (user == null) return;
            try
            {
                _benutzerService.ChangePassword(user.Id, _txtNewPassword.Text);
                _txtNewPassword.Clear();
                LoadUsers();
                _lblStatus.Text = "Passwort wurde geändert ✔";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Passwort ändern", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ToggleSelectedUserActive()
        {
            var user = SelectedUser;
            if (user == null) return;
            try
            {
                _benutzerService.SetUserActive(user.Id, !user.IsActive);
                LoadUsers();
                _lblStatus.Text = user.IsActive ? "Benutzer wurde gesperrt ✔" : "Benutzer wurde aktiviert ✔";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Benutzerstatus ändern", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private class UserDisplayRow
        {
            public Guid Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public string Status { get; set; } = string.Empty;
            public string LastLoginAt { get; set; } = string.Empty;
            public int FailedLogins { get; set; }
            public string Permissions { get; set; } = string.Empty;
        }
    }
}

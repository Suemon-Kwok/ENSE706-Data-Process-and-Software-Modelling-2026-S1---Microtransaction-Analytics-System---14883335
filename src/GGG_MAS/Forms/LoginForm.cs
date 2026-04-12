// =============================================================
// LoginForm.cs — Login window (FR13, FR14)
// First screen shown on launch. Authenticates the user and
// stores the session in AuthService for the lifetime of the app.
// Clean, professional GGG-themed dark UI.
// =============================================================

namespace GGG_MAS.Forms
{
    using GGG_MAS.Services;

    /// <summary>
    /// Modal login dialog. Only proceeds to the dashboard
    /// when valid credentials are supplied.
    /// </summary>
    public class LoginForm : Form
    {
        // ── UI Controls ───────────────────────────────────────
        private Panel     _pnlHeader    = null!;
        private Label     _lblTitle     = null!;
        private Label     _lblSubtitle  = null!;
        private Label     _lblUser      = null!;
        private TextBox   _txtUser      = null!;
        private Label     _lblPass      = null!;
        private TextBox   _txtPass      = null!;
        private Button    _btnLogin     = null!;
        private Label     _lblError     = null!;
        private Label     _lblHint      = null!;

        // Injected authentication service
        private readonly AuthService _auth;

        public LoginForm(AuthService auth)
        {
            _auth = auth;
            InitialiseComponents();
        }

        // Builds all controls programmatically — no designer file needed
        private void InitialiseComponents()
        {
            // ── Form settings ─────────────────────────────────
            Text            = "GGG MAS — Login";
            Size            = new Size(420, 460);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            BackColor       = Color.FromArgb(18, 24, 38);   // dark navy background

            // ── Header panel (GGG orange accent) ──────────────
            _pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 110,
                BackColor = Color.FromArgb(30, 58, 95)   // deep blue
            };

            _lblTitle = new Label
            {
                Text      = "⚙  GGG Analytics",
                Font      = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(251, 146, 60),   // GGG orange
                AutoSize  = true,
                Location  = new Point(20, 18)
            };

            _lblSubtitle = new Label
            {
                Text      = "Microtransaction Analytics System",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(148, 180, 212),
                AutoSize  = true,
                Location  = new Point(22, 60)
            };

            _pnlHeader.Controls.Add(_lblTitle);
            _pnlHeader.Controls.Add(_lblSubtitle);

            // ── Username row ──────────────────────────────────
            _lblUser = MakeLabel("Username", 140);
            _txtUser = MakeTextBox(165);

            // ── Password row ──────────────────────────────────
            _lblPass = MakeLabel("Password", 215);
            _txtPass = MakeTextBox(240);
            _txtPass.UseSystemPasswordChar = true;    // mask the password
            _txtPass.KeyDown += (s, e) =>             // allow Enter key to submit
            {
                if (e.KeyCode == Keys.Enter) AttemptLogin();
            };

            // ── Login button (orange GGG accent) ─────────────
            _btnLogin = new Button
            {
                Text      = "Sign In",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(234, 88, 12),   // burnt orange
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(356, 42),
                Location  = new Point(20, 290),
                Cursor    = Cursors.Hand
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += (s, e) => AttemptLogin();

            // ── Error message label (hidden by default) ───────
            _lblError = new Label
            {
                Text      = "Invalid username or password.",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(248, 113, 113),   // soft red
                AutoSize  = true,
                Location  = new Point(20, 345),
                Visible   = false
            };

            // ── Demo credentials hint ─────────────────────────
            _lblHint = new Label
            {
                Text = "Demo accounts:  analyst / analyst123  |  marketing / mktg123\n" +
                       "                developer / dev123    |  finance / finance123",
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize  = true,
                Location  = new Point(20, 380)
            };

            // ── Add all controls to form ───────────────────────
            Controls.Add(_pnlHeader);
            Controls.Add(_lblUser);
            Controls.Add(_txtUser);
            Controls.Add(_lblPass);
            Controls.Add(_txtPass);
            Controls.Add(_btnLogin);
            Controls.Add(_lblError);
            Controls.Add(_lblHint);

            // Focus the username field on load
            Load += (s, e) => _txtUser.Focus();
        }

        // Helper: creates a styled label at a fixed Y position
        private static Label MakeLabel(string text, int y) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(148, 180, 212),
            AutoSize  = true,
            Location  = new Point(20, y)
        };

        // Helper: creates a styled dark text box at a fixed Y position
        private static TextBox MakeTextBox(int y) => new TextBox
        {
            Font      = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Size      = new Size(356, 28),
            Location  = new Point(20, y)
        };

        // ── Login logic ───────────────────────────────────────
        private void AttemptLogin()
        {
            _lblError.Visible = false;   // reset previous error

            // Delegate to AuthService — does not reveal which field was wrong
            if (_auth.Login(_txtUser.Text.Trim(), _txtPass.Text))
            {
                DialogResult = DialogResult.OK;   // signal success to Program.cs
                Close();
            }
            else
            {
                _lblError.Visible = true;   // show generic error
                _txtPass.Clear();           // clear password on failure
                _txtPass.Focus();
            }
        }
    }
}

// Name : Suemon Kwok

// Student ID : 14883335

// LoginForm.cs — Login window (FR13, FR14)

// First screen shown on launch. Authenticates the user and

// stores the session in AuthService for the lifetime of the app.




namespace GGG_MAS.Forms
{
    using GGG_MAS.Services;                                                                                                         // brings in AuthService

    /// <summary>

    /// Modal login dialog. Only proceeds to the dashboard

    /// when valid credentials are supplied.

    /// </summary>

    public class LoginForm : Form                                                                                                   // inherits from Form — this is a Windows Forms dialog window
    {
        // UI Controls─
        private Panel     _pnlHeader    = null!;                                                                                    // top coloured banner containing the app title

        private Label     _lblTitle     = null!;                                                                                    // large "GGG Analytics" heading inside the banner

        private Label     _lblSubtitle  = null!;                                                                                    // smaller subtitle text inside the banner

        private Label     _lblUser      = null!;                                                                                    // "Username" field label

        private TextBox   _txtUser      = null!;                                                                                    // text input for the username

        private Label     _lblPass      = null!;                                                                                    // "Password" field label

        private TextBox   _txtPass      = null!;                                                                                    // text input for the password (masked)

        private Button    _btnLogin     = null!;                                                                                    // "Sign In" button that triggers authentication    

        private Label     _lblError     = null!;                                                                                    // red error message, hidden until a failed attempt

        private Label     _lblHint      = null!;                                                                                    // grey hint showing demo account credentials

        // Injected authentication service
        private readonly AuthService _auth;                                                                                         // reference to AuthService; used in AttemptLogin()

        public LoginForm(AuthService auth)
        {
            _auth = auth;                                                                                                           // stores the injected AuthService for use in AttemptLogin()

            InitialiseComponents();                                                                                                 // builds all controls programmatically
        }

        // Builds all controls programmatically — no designer file needed
        private void InitialiseComponents()
        {
            // Form settings
            Text            = "GGG MAS — Login";                                                                                    // sets the window title bar text

            Size            = new Size(420, 460);                                                                                   // fixed window size (width × height in pixels)

            StartPosition   = FormStartPosition.CenterScreen;                                                                       // opens the form in the centre of the screen

            FormBorderStyle = FormBorderStyle.FixedDialog;                                                                          // prevents the user from resizing the dialog    

            MaximizeBox     = false;                                                                                                // hides the maximise button

            BackColor       = Color.FromArgb(18, 24, 38);                                                                           // dark navy background

            // Header panel (GGG orange accent)
            _pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,                                                                                          // stretches to fill the full width at the top of the form

                Height    = 110,                                                                                                    // 110px tall header area

                BackColor = Color.FromArgb(30, 58, 95)                                                                              // deep blue colour matching the GGG theme
            };

            _lblTitle = new Label
            {
                Text      = "⚙  GGG Analytics",                                                                                   // gear icon + app name  

                Font      = new Font("Segoe UI", 20, FontStyle.Bold),                                                              // large bold font for the heading

                ForeColor = Color.FromArgb(251, 146, 60),                                                                          // GGG orange accent colour

                AutoSize  = true,                                                                                                  // label resizes to fit text

                Location  = new Point(20, 18)                                                                                      // positioned 20px from left, 18px from top of panel
            };

            _lblSubtitle = new Label
            {
                Text      = "Microtransaction Analytics System",                                                                    // descriptive subtitle

                Font      = new Font("Segoe UI", 9),                                                                                // smaller regular-weight font

                ForeColor = Color.FromArgb(148, 180, 212),                                                                          // muted blue-grey colour

                AutoSize  = true,
                
                Location  = new Point(22, 60)                                                                                       // positioned below the title
            };

            _pnlHeader.Controls.Add(_lblTitle);                                                                                     // adds the title label to the header panel

            _pnlHeader.Controls.Add(_lblSubtitle);                                                                                  // adds the subtitle label to the header panel

            // Username row
            _lblUser = MakeLabel("Username", 140);                                                                                  // creates the "Username" label at y=140

            _txtUser = MakeTextBox(165);                                                                                            // creates the username text box at y=165

            // Password row
            _lblPass = MakeLabel("Password", 215);                                                                                  // creates the "Password" label at y=215

            _txtPass = MakeTextBox(240);                                                                                            // creates the password text box at y=240

            _txtPass.UseSystemPasswordChar = true;                                                                                  // replaces typed characters with bullets (password masking)

            _txtPass.KeyDown += (s, e) =>                                                                                           // wires up a keyboard event handler on the password box
            {
                if (e.KeyCode == Keys.Enter) AttemptLogin();                                                                        // pressing Enter triggers login (accessibility improvement)
            };

            // Login button (orange GGG accent)
            _btnLogin = new Button
            {
                Text      = "Sign In",
                
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                
                BackColor = Color.FromArgb(234, 88, 12),                                                                            // burnt orange matching the GGG brand

                ForeColor = Color.White,
                
                FlatStyle = FlatStyle.Flat,                                                                                         // removes the default 3D button border

                Size      = new Size(356, 42),                                                                                      // wide button spanning almost the full form width

                Location  = new Point(20, 290),
                
                Cursor    = Cursors.Hand                                                                                            // shows a pointer cursor on hover for better UX
            };
            _btnLogin.FlatAppearance.BorderSize = 0;                                                                                // removes the flat-style border entirely

            _btnLogin.Click += (s, e) => AttemptLogin();                                                                            // wires up the click event to the login method

            // Error message label (hidden by default)
            _lblError = new Label
            {
                Text      = "Invalid username or password.",                                                                        // generic error message — never reveals which field is wrong

                Font      = new Font("Segoe UI", 9),
                
                ForeColor = Color.FromArgb(248, 113, 113),                                                                          // soft red to signal an error

                AutoSize  = true,
                
                Location  = new Point(20, 345),
                
                Visible   = false                                                                                                   // hidden until a failed login attempt occurs
            };

            // Demo credentials hint
            _lblHint = new Label
            {
                Text = "Demo accounts:  analyst / analyst123  |  marketing / mktg123\n" +
                       "                developer / dev123    |  finance / finance123",
                Font      = new Font("Segoe UI", 7.5f),                                                                             // small font so it doesn't dominate the layout

                ForeColor = Color.FromArgb(100, 116, 139),                                                                          // very muted grey — helper text only

                AutoSize  = true,
                
                Location  = new Point(20, 380)
            };

            // Add all controls to form
            Controls.Add(_pnlHeader);                                                                                               // adds the header panel (title + subtitle)

            Controls.Add(_lblUser);                                                                                                 // adds the username label

            Controls.Add(_txtUser);                                                                                                 // adds the username text box

            Controls.Add(_lblPass);                                                                                                 // adds the password label

            Controls.Add(_txtPass);                                                                                                 // adds the password text box

            Controls.Add(_btnLogin);                                                                                                // adds the sign-in button

            Controls.Add(_lblError);                                                                                                // adds the (hidden) error label

            Controls.Add(_lblHint);                                                                                                 // adds the demo credentials hint      

            // Focus the username field on load so the user can start typing immediately
            Load += (s, e) => _txtUser.Focus();                                                                                     // wires the Load event so focus is set after the form is shown
        }

        // Helper: creates a styled label at a fixed Y position
        private static Label MakeLabel(string text, int y) => new Label
        {
            Text      = text,
            
            Font      = new Font("Segoe UI", 9),
            
            ForeColor = Color.FromArgb(148, 180, 212),                                                                              // muted blue-grey for field labels

            AutoSize  = true,
            
            Location  = new Point(20, y)                                                                                            // fixed x=20, variable y position
        };

        // Helper: creates a styled dark text box at a fixed Y position
        private static TextBox MakeTextBox(int y) => new TextBox
        {
            Font      = new Font("Segoe UI", 10),
            
            BackColor = Color.FromArgb(30, 41, 59),                                                                                 // dark blue-grey input background

            ForeColor = Color.White,                                                                                                // white text for contrast on dark background

            BorderStyle = BorderStyle.FixedSingle,                                                                                  // single-pixel border

            Size      = new Size(356, 28),                                                                                          // wide input spanning most of the form

            Location  = new Point(20, y)                                                                                            // fixed x=20, variable y position
        };

        // Login logic — called by button click and Enter key press
        
        private void AttemptLogin()
        {
            _lblError.Visible = false;                                                                                              // hides any previous error message before attempting again

            // Delegate to AuthService — does not reveal which field was wrong
            if (_auth.Login(_txtUser.Text.Trim(), _txtPass.Text))                                                                   // Trim() removes accidental leading/trailing spaces
            {
                DialogResult = DialogResult.OK;                                                                                     // signals success to Program.cs (ShowDialog returns DialogResult.OK)

                Close();                                                                                                            // closes the login form and returns control to Program.cs
            }
            else
            {
                _lblError.Visible = true;                                                                                           // reveals the generic error message to the user
                
                _txtPass.Clear();                                                                                                   // clears the password field so the user can retype without selecting first

                _txtPass.Focus();                                                                                                   // moves focus back to the password box for quick retry
            }
        }
    }
}

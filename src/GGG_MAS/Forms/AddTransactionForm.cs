// Name : Suemon Kwok

// Student ID : 14883335

// AddTransactionForm.cs — "Record Purchase" dialog

// FR01–FR03, FR05: captures item, player, character class,

// bundle flag. Validates before submitting to TransactionService.

// What does this file do
// the "Record New Purchase" modal. Three dropdowns (item, player, character class) plus a bundle checkbox.
// When submitted, calls TransactionService.RecordPurchase(). Shows a live price preview when an item is selected

// OOP concept
// Encapsulation via the private ComboItemWrapper inner class —
// it hides the item ID from the dropdown display, only exposing the human-readable name to the user.

namespace GGG_MAS.Forms
{
    using GGG_MAS.Models;                                                                                           // brings in MTXItem, PlayerAccount, CharacterClass, Transaction

    using GGG_MAS.Repositories;                                                                                     // brings in IItemRepository

    using GGG_MAS.Services;                                                                                         // brings in TransactionService

    /// <summary>

    /// Modal dialog for recording a new MTX purchase event.

    /// Used by Analysts and Developers (FR14).

    /// </summary>

    public class AddTransactionForm : Form                                                                          // inherits from Form — this is a modal Windows Forms dialog
    {
        // Controls
        private Label    _lblItem      = null!;                                                                     // "MTX Item" field label

        private ComboBox _cmbItem      = null!;                                                                     // dropdown listing all catalogue items

        private Label    _lblPlayer    = null!;                                                                     // "Player Account" field label

        private ComboBox _cmbPlayer    = null!;                                                                     // dropdown listing all demo player accounts

        private Label    _lblClass     = null!;                                                                     // "Character Class" field label

        private ComboBox _cmbClass     = null!;                                                                     // dropdown listing all CharacterClass enum values

        private CheckBox _chkBundle    = null!;                                                                     // tick box for marking the purchase as a bundle (FR05)

        private Label    _lblPrice     = null!;                                                                     // live price preview label that updates on item selection

        private Button   _btnSubmit    = null!;                                                                     // "Record Purchase" button that calls Submit()

        private Button   _btnCancel    = null!;                                                                     // "Cancel" button that closes the form without saving

        private Label    _lblStatus    = null!;                                                                     // red error/status label shown if validation fails

        // Dependencies
        private readonly TransactionService    _txService;                                                          // handles the business logic of recording a purchase

        private readonly List<MTXItem>         _catalogue;                                                          // local copy of all items for price preview     

        private readonly List<PlayerAccount>   _players;                                                            // local copy of all players for the dropdown

        // The recorded transaction is stored here after success
        public Transaction? RecordedTransaction { get; private set; }                                               // null until a purchase is successfully recorded

        public AddTransactionForm(TransactionService txService,
                                  IItemRepository    itemRepo,
                                  List<PlayerAccount> players)
        {
            _txService = txService;                                                                                 // stores the transaction service reference

            _catalogue = itemRepo.GetAll().ToList();                                                                // materialises the full catalogue for price preview lookups

            _players   = players;                                                                                   // stores the player list for the dropdown

            InitialiseComponents();                                                                                 // builds all controls programmatically
        }

        private void InitialiseComponents()
        {
            // ── Form settings
            Text            = "Record New Purchase";                                                                // window title bar text

            Size            = new Size(440, 380);                                                                   // fixed dialog size

            StartPosition   = FormStartPosition.CenterParent;                                                       // centres over the parent dashboard window

            FormBorderStyle = FormBorderStyle.FixedDialog;                                                          // prevents resizing

            MaximizeBox     = false;                                                                                // hides the maximise button

            BackColor       = Color.FromArgb(18, 24, 38);                                                           // dark navy background

            int row = 20, rowH = 56;                                                                                // row = current vertical position; rowH = how much to advance between rows

            // ── Item selector
            _lblItem = MakeLabel("MTX Item", row);                                                                  // creates the "MTX Item" label at the current row

            _cmbItem = MakeCombo(row + 22, 380);                                                                    // creates the dropdown 22px below the label

            // Populate with all catalogue items
            foreach (var item in _catalogue)                                                                        // loops over every item in the catalogue
                _cmbItem.Items.Add(new ComboItemWrapper(item.ItemId, item.ToString()!));                            // adds each item as a wrapper with ID + display text

            _cmbItem.SelectedIndexChanged += (s, e) => UpdatePriceLabel();                                          // wires up event: updates price preview when selection changes
            
            row += rowH;                                                                                            // advances the vertical position down by one row height

            // ── Player selector
            _lblPlayer = MakeLabel("Player Account", row);                                                          // creates the "Player Account" label

            _cmbPlayer = MakeCombo(row + 22, 380);                                                                  // creates the player dropdown
            foreach (var p in _players)                                                                             // loops over every player account
                _cmbPlayer.Items.Add(new ComboItemWrapper(p.AccountId, p.ToString()!));                             // adds each player as a wrapper with ID + display text    

            row += rowH;                                                                                            // advances vertical position

            // Character class selector (FR03)
            _lblClass = MakeLabel("Character Class", row);                                                          // creates the "Character Class" label

            _cmbClass = MakeCombo(row + 22, 380);                                                                   // creates the class dropdown
            foreach (CharacterClass cls in Enum.GetValues<CharacterClass>())                                        // iterates all 7 enum values
                _cmbClass.Items.Add(cls);                                                                           // adds each CharacterClass enum value directly (ToString() is called automatically)

            row += rowH;

            // Bundle checkbox (FR05)
            _chkBundle = new CheckBox
            {
                Text      = "Bundle purchase",                                                                      // label shown beside the tick box

                Font      = new Font("Segoe UI", 9),
                
                ForeColor = Color.FromArgb(148, 180, 212),                                                          // muted blue-grey text

                AutoSize  = true,
                
                Location  = new Point(14, row)
            };
            row += 36;                                                                                              // checkbox row is shorter than the full rowH since it has no label above it

            // Computed price display
            _lblPrice = new Label
            {
                Text      = "Price: —",                                                                             // placeholder until an item is selected

                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                
                ForeColor = Color.FromArgb(251, 146, 60),                                                           // GGG orange — makes price stand out

                AutoSize  = true,
                
                Location  = new Point(14, row)
            };
            row += 40;

            // Status label
            _lblStatus = new Label
            {
                Text      = "",                                                                                    // empty by default; filled on validation failure

                Font      = new Font("Segoe UI", 8),                                                               // soft red for error messages

                ForeColor = Color.FromArgb(248, 113, 113),
                
                AutoSize  = true,
                
                Location  = new Point(14, row)
            };
            row += 28;

            // Action buttons
            _btnSubmit = MakeButton("Record Purchase", 14, row, Color.FromArgb(21, 128, 61));                       // green submit button on the left

            _btnCancel = MakeButton("Cancel",         210, row, Color.FromArgb(100, 116, 139));                     // grey cancel button to the right        

            _btnSubmit.Click += (s, e) => Submit();                                                                 // wires submit button to Submit()

            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };                         // cancel sets result and closes

            // Add controls to form
            Controls.AddRange(new Control[]
            {
                _lblItem, _cmbItem, _lblPlayer, _cmbPlayer,
                
                _lblClass, _cmbClass, _chkBundle, _lblPrice,
                
                _lblStatus, _btnSubmit, _btnCancel
            }); // AddRange adds all controls in one call instead of individual Controls.Add() calls
        }

        // Update the price preview when item changes
        private void UpdatePriceLabel()
        {
            if (_cmbItem.SelectedItem is ComboItemWrapper w)                                                        // pattern-matches and casts the selected item to a ComboItemWrapper
            {
                var item = _catalogue.FirstOrDefault(i => i.ItemId == w.Id);                                        // finds the matching MTXItem by ID
                
                _lblPrice.Text = item != null ? $"Price: ${item.GetPrice():F2} NZD" : "Price: —";
            } // F2 = two decimal places; shows "Price: —" if item not found (shouldn't happen)
        }

        // Submit the transaction through the service layer
        private void Submit()
        {
            // Validate all required fields are selected
            if (_cmbItem.SelectedItem == null || _cmbPlayer.SelectedItem == null
                || _cmbClass.SelectedItem == null)
            {
                _lblStatus.Text = "Please select an item, player, and character class.";                            // shows validation message
                
                return;                                                                                             // exits the method without recording — forces the user to complete all fields
            }

            try
            {
                var itemId  = ((ComboItemWrapper)_cmbItem.SelectedItem).Id;                                         // extracts the item ID from the wrapper

                var acctId  = ((ComboItemWrapper)_cmbPlayer.SelectedItem).Id;                                       // extracts the account ID from the wrapper

                var cls     = (CharacterClass)_cmbClass.SelectedItem;                                               // casts the selected enum value directly

                var player  = _players.First(p => p.AccountId == acctId);                                           // looks up the full PlayerAccount by ID

                // Delegate to service — handles validation, persistence, and player updates
                RecordedTransaction = _txService.RecordPurchase(itemId, player, cls, _chkBundle.Checked);
                // stores the returned Transaction so the dashboard can show a confirmation message


                DialogResult = DialogResult.OK;                                                                     // signals to ShowDialog() caller that the purchase was successfully recorded

                Close();                                                                                            // closes the dialog and returns to the dashboard
            }
            catch (Exception ex)
            {
                // Show user-friendly error without exposing internal details
                _lblStatus.Text = $"Error: {ex.Message}";                                                          // displays the exception message in the status label     
            }
        }

        // Control factory helpers

        // Creates a styled field label at position (14, y)

        private static Label MakeLabel(string text, int y) => new Label
        {
            Text = text, Font = new Font("Segoe UI", 8.5f),
            
            ForeColor = Color.FromArgb(148, 180, 212),                                                              // muted blue-grey for field labels

            AutoSize  = true, Location = new Point(14, y)                                                           // all labels start at x=14
        };


        // Creates a styled dark combo box at position (14, y) with specified width

        private static ComboBox MakeCombo(int y, int width) => new ComboBox
        {
            Font          = new Font("Segoe UI", 9),
            
            BackColor     = Color.FromArgb(30, 41, 59),                                                             // dark blue-grey input background

            ForeColor     = Color.White,
            
            DropDownStyle = ComboBoxStyle.DropDownList,                                                             // prevents free-text entry; selection only

            Size          = new Size(width, 26),
            
            Location      = new Point(14, y)
        };


        // Creates a styled flat button at position (x, y) with the given background colour

        private static Button MakeButton(string text, int x, int y, Color bg) => new Button
        {
            Text      = text,
            
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            
            BackColor = bg,
            
            ForeColor = Color.White,
            
            FlatStyle = FlatStyle.Flat,                                                                             // removes the default raised border

            Size      = new Size(180, 34),
            
            Location  = new Point(x, y),
            
            Cursor    = Cursors.Hand                                                                                // shows a pointer cursor on hover
        };

        // Helper class: wraps an ID + display text for ComboBox items

        // Needed because ComboBox displays the result of ToString() but we need the ID for lookups

        private class ComboItemWrapper
        {
            public string Id      { get; }                                                                          // the internal ID (e.g. "WPN_001" or "P003")

            public string Display { get; }                                                                          // the human-readable label shown in the dropdown

            public ComboItemWrapper(string id, string display) { Id = id; Display = display; }
            
            public override string ToString() => Display;                                                           // ComboBox calls this to get the visible text
        }
    }
}

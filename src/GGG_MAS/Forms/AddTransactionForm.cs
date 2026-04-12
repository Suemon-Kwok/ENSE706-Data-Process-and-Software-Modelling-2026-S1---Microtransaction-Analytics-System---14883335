// =============================================================
// AddTransactionForm.cs — "Record Purchase" dialog
// FR01–FR03, FR05: captures item, player, character class,
// bundle flag. Validates before submitting to TransactionService.
// =============================================================

namespace GGG_MAS.Forms
{
    using GGG_MAS.Models;
    using GGG_MAS.Repositories;
    using GGG_MAS.Services;

    /// <summary>
    /// Modal dialog for recording a new MTX purchase event.
    /// Used by Analysts and Developers (FR14).
    /// </summary>
    public class AddTransactionForm : Form
    {
        // ── Controls ──────────────────────────────────────────
        private Label    _lblItem      = null!;
        private ComboBox _cmbItem      = null!;
        private Label    _lblPlayer    = null!;
        private ComboBox _cmbPlayer    = null!;
        private Label    _lblClass     = null!;
        private ComboBox _cmbClass     = null!;
        private CheckBox _chkBundle    = null!;
        private Label    _lblPrice     = null!;
        private Button   _btnSubmit    = null!;
        private Button   _btnCancel    = null!;
        private Label    _lblStatus    = null!;

        // ── Dependencies ──────────────────────────────────────
        private readonly TransactionService    _txService;
        private readonly List<MTXItem>         _catalogue;
        private readonly List<PlayerAccount>   _players;

        // The recorded transaction is stored here after success
        public Transaction? RecordedTransaction { get; private set; }

        public AddTransactionForm(TransactionService txService,
                                  IItemRepository    itemRepo,
                                  List<PlayerAccount> players)
        {
            _txService = txService;
            _catalogue = itemRepo.GetAll().ToList();
            _players   = players;
            InitialiseComponents();
        }

        private void InitialiseComponents()
        {
            // ── Form settings ─────────────────────────────────
            Text            = "Record New Purchase";
            Size            = new Size(440, 380);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            BackColor       = Color.FromArgb(18, 24, 38);

            int row = 20, rowH = 56;

            // ── Item selector ─────────────────────────────────
            _lblItem = MakeLabel("MTX Item", row);
            _cmbItem = MakeCombo(row + 22, 380);
            // Populate with all catalogue items
            foreach (var item in _catalogue)
                _cmbItem.Items.Add(new ComboItemWrapper(item.ItemId, item.ToString()!));
            _cmbItem.SelectedIndexChanged += (s, e) => UpdatePriceLabel();
            row += rowH;

            // ── Player selector ───────────────────────────────
            _lblPlayer = MakeLabel("Player Account", row);
            _cmbPlayer = MakeCombo(row + 22, 380);
            foreach (var p in _players)
                _cmbPlayer.Items.Add(new ComboItemWrapper(p.AccountId, p.ToString()!));
            row += rowH;

            // ── Character class selector (FR03) ───────────────
            _lblClass = MakeLabel("Character Class", row);
            _cmbClass = MakeCombo(row + 22, 380);
            foreach (CharacterClass cls in Enum.GetValues<CharacterClass>())
                _cmbClass.Items.Add(cls);
            row += rowH;

            // ── Bundle checkbox (FR05) ─────────────────────────
            _chkBundle = new CheckBox
            {
                Text      = "Bundle purchase",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(148, 180, 212),
                AutoSize  = true,
                Location  = new Point(14, row)
            };
            row += 36;

            // ── Computed price display ─────────────────────────
            _lblPrice = new Label
            {
                Text      = "Price: —",
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(251, 146, 60),
                AutoSize  = true,
                Location  = new Point(14, row)
            };
            row += 40;

            // ── Status label ──────────────────────────────────
            _lblStatus = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(248, 113, 113),
                AutoSize  = true,
                Location  = new Point(14, row)
            };
            row += 28;

            // ── Action buttons ────────────────────────────────
            _btnSubmit = MakeButton("Record Purchase", 14, row, Color.FromArgb(21, 128, 61));
            _btnCancel = MakeButton("Cancel",         210, row, Color.FromArgb(100, 116, 139));
            _btnSubmit.Click += (s, e) => Submit();
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // ── Add controls ──────────────────────────────────
            Controls.AddRange(new Control[]
            {
                _lblItem, _cmbItem, _lblPlayer, _cmbPlayer,
                _lblClass, _cmbClass, _chkBundle, _lblPrice,
                _lblStatus, _btnSubmit, _btnCancel
            });
        }

        // Update the price preview when item changes
        private void UpdatePriceLabel()
        {
            if (_cmbItem.SelectedItem is ComboItemWrapper w)
            {
                var item = _catalogue.FirstOrDefault(i => i.ItemId == w.Id);
                _lblPrice.Text = item != null ? $"Price: ${item.GetPrice():F2} NZD" : "Price: —";
            }
        }

        // Submit the transaction through the service layer
        private void Submit()
        {
            // Validate all required fields are selected
            if (_cmbItem.SelectedItem == null || _cmbPlayer.SelectedItem == null
                || _cmbClass.SelectedItem == null)
            {
                _lblStatus.Text = "Please select an item, player, and character class.";
                return;
            }

            try
            {
                var itemId  = ((ComboItemWrapper)_cmbItem.SelectedItem).Id;
                var acctId  = ((ComboItemWrapper)_cmbPlayer.SelectedItem).Id;
                var cls     = (CharacterClass)_cmbClass.SelectedItem;
                var player  = _players.First(p => p.AccountId == acctId);

                // Delegate to service — handles validation, persistence, and player updates
                RecordedTransaction = _txService.RecordPurchase(itemId, player, cls, _chkBundle.Checked);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                // Show user-friendly error without exposing internal details
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        // ── Control factory helpers ───────────────────────────

        private static Label MakeLabel(string text, int y) => new Label
        {
            Text = text, Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(148, 180, 212),
            AutoSize  = true, Location = new Point(14, y)
        };

        private static ComboBox MakeCombo(int y, int width) => new ComboBox
        {
            Font          = new Font("Segoe UI", 9),
            BackColor     = Color.FromArgb(30, 41, 59),
            ForeColor     = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size          = new Size(width, 26),
            Location      = new Point(14, y)
        };

        private static Button MakeButton(string text, int x, int y, Color bg) => new Button
        {
            Text      = text,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = bg,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size      = new Size(180, 34),
            Location  = new Point(x, y),
            Cursor    = Cursors.Hand
        };

        // ── Helper class: wraps an ID + display text for ComboBox items ──
        private class ComboItemWrapper
        {
            public string Id      { get; }
            public string Display { get; }
            public ComboItemWrapper(string id, string display) { Id = id; Display = display; }
            public override string ToString() => Display;
        }
    }
}

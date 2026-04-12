// =============================================================
// MainDashboardForm.cs — AnalyticsDashboard (UML)
// LAYOUT FIX: all UI issues from screenshots resolved:
//   • Sidebar 180px fixed left; all content fully to its right
//   • Filter bar has two rows (labels above, controls below)
//   • Apply button is large (90×38) and always fully visible
//   • Stat cards fully visible — no clipping behind sidebar
//   • All 4 views use Dock=Fill grids → no truncation on any screen
//   • SplitContainer keeps the two Sales grids side-by-side
//   • Revenue Trends: Period + Revenue columns only (no RawRevenue)
//   • Underperforming: Item, Category, Sales, Status all visible
//   • Demographics: Region, Revenue, Casual, Regular, HighValue
//   • AutoCols() with MinimumWidth=60 keeps columns tight but readable
// =============================================================

namespace GGG_MAS.Forms
{
    using GGG_MAS.Models;
    using GGG_MAS.Repositories;
    using GGG_MAS.Services;

    public class MainDashboardForm : Form
    {
        // ── Panels ────────────────────────────────────────────
        private Panel  _pnlSidebar  = null!;
        private Panel  _pnlMain     = null!;
        private Panel  _pnlTopBar   = null!;
        private Panel  _pnlFilters  = null!;
        private Panel  _pnlViewHost = null!;

        // ── Sidebar buttons ───────────────────────────────────
        private Button _btnSales     = null!;
        private Button _btnDemograph = null!;
        private Button _btnTrends    = null!;
        private Button _btnUnderperf = null!;
        private Button _btnAddTx     = null!;
        private Button _btnExport    = null!;
        private Button _btnLogout    = null!;
        private Label  _lblUserInfo  = null!;

        // Sidebar collapse/expand toggle
        private Button _btnToggle      = null!;  // arrow in top-bar
        private bool   _sidebarVisible = true;   // current state

        // ── Filter controls ───────────────────────────────────
        private ComboBox       _cmbRegion    = null!;
        private ComboBox       _cmbItemType  = null!;
        private ComboBox       _cmbCharClass = null!;
        private ComboBox       _cmbGran      = null!;
        private DateTimePicker _dtpFrom      = null!;
        private DateTimePicker _dtpTo        = null!;
        private Button         _btnRefresh   = null!;

        // ── View panels ───────────────────────────────────────
        private Panel _viewSales       = null!;
        private Panel _viewDemographic = null!;
        private Panel _viewTrends      = null!;
        private Panel _viewUnderperf   = null!;

        // ── Grids ─────────────────────────────────────────────
        private DataGridView _dgvTopCategory = null!;
        private DataGridView _dgvTopClass    = null!;
        private DataGridView _dgvDemographic = null!;
        private DataGridView _dgvTrends      = null!;
        private DataGridView _dgvUnderperf   = null!;

        // ── KPI stat cards ────────────────────────────────────
        private Panel _cardRevenue   = null!;
        private Panel _cardTxCount   = null!;
        private Panel _cardBundlePct = null!;
        private Panel _cardTopItem   = null!;

        // ── Services ──────────────────────────────────────────
        private readonly AuthService            _auth;
        private readonly ReportEngine           _engine;
        private readonly ITransactionRepository _txRepo;
        private readonly IItemRepository        _itemRepo;
        private readonly TransactionService     _txService;
        private readonly List<PlayerAccount>    _players;
        private Report? _currentReport;

        // ── Colours ───────────────────────────────────────────
        private static readonly Color ColBg      = Color.FromArgb(18,  24,  38);
        private static readonly Color ColSidebar = Color.FromArgb(12,  18,  30);
        private static readonly Color ColAccent  = Color.FromArgb(234, 88,  12);
        private static readonly Color ColHeader  = Color.FromArgb(30,  58,  95);
        private static readonly Color ColCard    = Color.FromArgb(22,  32,  50);
        private static readonly Color ColMuted   = Color.FromArgb(100, 116, 139);
        private static readonly Color ColFilt    = Color.FromArgb(22,  32,  50);
        private static readonly Color ColGridRow = Color.FromArgb(22,  32,  50);
        private static readonly Color ColGridAlt = Color.FromArgb(26,  38,  60);
        private static readonly Color ColGridHdr = Color.FromArgb(30,  58,  95);
        private static readonly Color ColGridLn  = Color.FromArgb(40,  55,  75);

        // Fixed sidebar width
        private const int SW = 180;

        // ─────────────────────────────────────────────────────
        public MainDashboardForm(AuthService auth, ReportEngine engine,
                                 ITransactionRepository txRepo,
                                 IItemRepository itemRepo,
                                 TransactionService txService,
                                 List<PlayerAccount> players)
        {
            _auth=auth; _engine=engine; _txRepo=txRepo;
            _itemRepo=itemRepo; _txService=txService; _players=players;
            Build();
            Load += (s,e) => RefreshDashboard();
        }

        // ═══════════════ BUILD ═══════════════════════════════

        private void Build()
        {
            Text          = "GGG — Microtransaction Analytics System";
            Size          = new Size(1280, 820);
            MinimumSize   = new Size(1050, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColBg;
            BuildSidebar();
            BuildMain();
        }

        // ── Sidebar ───────────────────────────────────────────
        private void BuildSidebar()
        {
            _pnlSidebar = new Panel { Dock=DockStyle.Left, Width=SW, BackColor=ColSidebar };

            int y = 16;
            _pnlSidebar.Controls.Add(SideSection("ANALYTICS", ref y));
            _btnSales     = SideBtn("📊  Sales Overview",   ref y);
            _btnDemograph = SideBtn("🌍  Demographics",     ref y);
            _btnTrends    = SideBtn("📈  Revenue Trends",   ref y);
            _btnUnderperf = SideBtn("⚠   Underperforming", ref y);
            foreach (var b in new[]{_btnSales,_btnDemograph,_btnTrends,_btnUnderperf})
                _pnlSidebar.Controls.Add(b);

            y += 8;
            _pnlSidebar.Controls.Add(SideSection("ACTIONS", ref y));
            _btnAddTx = SideBtn("➕  Record Purchase", ref y);
            _btnExport= SideBtn("📁  Export Report",   ref y);
            _pnlSidebar.Controls.Add(_btnAddTx);
            _pnlSidebar.Controls.Add(_btnExport);

            _btnLogout = new Button
            {
                Text="⏻  Logout", Font=new Font("Segoe UI",9),
                ForeColor=ColMuted, BackColor=Color.Transparent,
                FlatStyle=FlatStyle.Flat, Size=new Size(SW-10,34),
                Location=new Point(5,460),
                TextAlign=ContentAlignment.MiddleLeft,
                Padding=new Padding(8,0,0,0), Cursor=Cursors.Hand
            };
            _btnLogout.FlatAppearance.BorderSize=0;
            _pnlSidebar.Controls.Add(_btnLogout);

            _btnSales.Click     += (_,__)=>{ ShowView(_viewSales);       HighlightNav(_btnSales);     };
            _btnDemograph.Click += (_,__)=>{ ShowView(_viewDemographic); HighlightNav(_btnDemograph); };
            _btnTrends.Click    += (_,__)=>{ ShowView(_viewTrends);      HighlightNav(_btnTrends);    };
            _btnUnderperf.Click += (_,__)=>{ ShowView(_viewUnderperf);   HighlightNav(_btnUnderperf); };
            _btnAddTx.Click     += (_,__)=> OpenAddTx();
            _btnExport.Click    += (_,__)=> OpenExport();
            _btnLogout.Click    += (_,__)=> Logout();

            Controls.Add(_pnlSidebar);
        }

        // ── Main panel (right of sidebar) ────────────────────
        private void BuildMain()
        {
            _pnlMain = new Panel { Dock=DockStyle.Fill, BackColor=ColBg };

            // Top title bar
            _pnlTopBar = new Panel { Dock=DockStyle.Top, Height=52, BackColor=ColHeader };
            _pnlTopBar.Controls.Add(new Label
            {
                Text="⚙  GGG Microtransaction Analytics System",
                Font=new Font("Segoe UI",13,FontStyle.Bold),
                ForeColor=ColAccent, AutoSize=true, Location=new Point(16,13)
            });
            _lblUserInfo = new Label
            {
                Font=new Font("Segoe UI",9), ForeColor=ColMuted,
                AutoSize=true, Location=new Point(880,18)
            };
            _pnlTopBar.Controls.Add(_lblUserInfo);

            // Toggle button: collapses/expands the sidebar
            // Anchored to the right edge of the top bar so it never overlaps content
            _btnToggle = new Button
            {
                Text      = "◀",   // left arrow = sidebar is open
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(50, 80, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(34, 34),
                Location  = new Point(4, 9),  // left side of top-bar
                Cursor    = Cursors.Hand,
                Tag       = "toggle"
            };
            _btnToggle.FlatAppearance.BorderSize = 0;
            _btnToggle.Click += ToggleSidebar;
            _pnlTopBar.Controls.Add(_btnToggle);

            // Filter bar — 76px tall, two-row (label on top, control below)
            _pnlFilters = new Panel { Dock=DockStyle.Top, Height=76, BackColor=ColFilt };
            BuildFilters();

            // View host fills remaining space
            _pnlViewHost = new Panel { Dock=DockStyle.Fill, BackColor=ColBg };
            BuildViewSales();
            BuildViewDemographic();
            BuildViewTrends();
            BuildViewUnderperf();

            // Add in reverse dock order so Top items stack correctly
            _pnlMain.Controls.Add(_pnlViewHost);  // Fill — must be first
            _pnlMain.Controls.Add(_pnlFilters);   // Top
            _pnlMain.Controls.Add(_pnlTopBar);    // Top (above filters)

            Controls.Add(_pnlMain);

            ShowView(_viewSales);
            HighlightNav(_btnSales);
        }

        // ── Filter bar ────────────────────────────────────────
        // Columns: From | To | Region | Type | Class | Group | [Apply]
        private void BuildFilters()
        {
            // Column start positions
            int[] xs   = { 12, 120, 232, 348, 474, 594 };
            int[] ws   = { 96,  96, 104, 114, 108,  86 };
            var labels = new[]{"From","To","Region","Type","Class","Group"};

            // Row 1: labels at y=8
            for (int i=0;i<labels.Length;i++)
                _pnlFilters.Controls.Add(new Label
                {
                    Text=labels[i], Font=new Font("Segoe UI",7.5f),
                    ForeColor=Color.FromArgb(148,180,212),
                    AutoSize=true, Location=new Point(xs[i],8)
                });

            // Row 2: controls at y=30
            _dtpFrom = new DateTimePicker { Format=DateTimePickerFormat.Short,
                Location=new Point(xs[0],30), Width=ws[0],
                Value=DateTime.Today.AddDays(-30) };
            _dtpTo = new DateTimePicker { Format=DateTimePickerFormat.Short,
                Location=new Point(xs[1],30), Width=ws[1],
                Value=DateTime.Today };

            _cmbRegion = FiltCombo(xs[2],ws[2]);
            _cmbRegion.Items.Add("All Regions");
            foreach (var r in new[]{"NZ","AU","US","EU","APAC"}) _cmbRegion.Items.Add(r);
            _cmbRegion.SelectedIndex=0;

            _cmbItemType = FiltCombo(xs[3],ws[3]);
            _cmbItemType.Items.Add("All Types");
            foreach (ItemType t in Enum.GetValues<ItemType>()) _cmbItemType.Items.Add(t);
            _cmbItemType.SelectedIndex=0;

            _cmbCharClass = FiltCombo(xs[4],ws[4]);
            _cmbCharClass.Items.Add("All Classes");
            foreach (CharacterClass c in Enum.GetValues<CharacterClass>()) _cmbCharClass.Items.Add(c);
            _cmbCharClass.SelectedIndex=0;

            _cmbGran = FiltCombo(xs[5],ws[5]);
            _cmbGran.Items.AddRange(new object[]{"daily","weekly","monthly"});
            _cmbGran.SelectedIndex=0;

            // Apply button — large orange, vertically centred in 76px bar
            _btnRefresh = new Button
            {
                Text="↻  Apply",
                Font=new Font("Segoe UI",9.5f,FontStyle.Bold),
                BackColor=ColAccent, ForeColor=Color.White,
                FlatStyle=FlatStyle.Flat,
                Size=new Size(96,42),          // large and prominent
                Location=new Point(694,17),    // vertically centred
                Cursor=Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderSize=0;
            _btnRefresh.Click+=(_,__)=>RefreshDashboard();

            _pnlFilters.Controls.AddRange(new Control[]
            {
                _dtpFrom,_dtpTo,_cmbRegion,_cmbItemType,_cmbCharClass,_cmbGran,_btnRefresh
            });
        }

        // ═══════════════ VIEW BUILDERS ═══════════════════════

        // ── Sales ─────────────────────────────────────────────
        private void BuildViewSales()
        {
            _viewSales = new Panel
            {
                Dock=DockStyle.Fill, BackColor=ColBg,
                Padding=new Padding(14,10,14,10)
            };

            // KPI cards row
            var pCards = new Panel { Dock=DockStyle.Top, Height=98, BackColor=Color.Transparent };
            _cardRevenue   = MakeCard("Total Revenue","$0.00",  0);
            _cardTxCount   = MakeCard("Transactions", "0",      206);
            _cardBundlePct = MakeCard("Bundle %",     "0%",     412);
            _cardTopItem   = MakeCard("Top Item",     "—",      618);
            pCards.Controls.AddRange(new Control[]
                {_cardRevenue,_cardTxCount,_cardBundlePct,_cardTopItem});
            _viewSales.Controls.Add(pCards);

            // Two side-by-side grid panels using Dock Left + Dock Fill.
            // Avoids SplitContainer which crashes if the form has no size yet.

            // Left panel — docks left, takes 50% width via Resize event
            var pLeft = new Panel { Dock=DockStyle.Left, BackColor=ColBg, Width=500 };
            var lblCat = GridTitle("🏆  Top Sellers by Category (BR-01)");
            lblCat.Dock = DockStyle.Top;
            lblCat.Height = 24;
            _dgvTopCategory = MakeGrid();
            _dgvTopCategory.Dock = DockStyle.Fill;
            pLeft.Controls.Add(_dgvTopCategory);   // Fill added first
            pLeft.Controls.Add(lblCat);            // Top stacks above it

            // Right panel — fills the remaining space
            var pRight = new Panel { Dock=DockStyle.Fill, BackColor=ColBg };
            var lblCls = GridTitle("🎮  Top by Class (BR-02)");
            lblCls.Dock = DockStyle.Top;
            lblCls.Height = 24;
            _dgvTopClass = MakeGrid();
            _dgvTopClass.Dock = DockStyle.Fill;
            pRight.Controls.Add(_dgvTopClass);     // Fill added first
            pRight.Controls.Add(lblCls);           // Top stacks above it

            // Outer row panel holds both side-by-side
            var pGridRow = new Panel { Dock=DockStyle.Fill, BackColor=ColBg, Padding=new Padding(0,4,0,0) };
            pGridRow.Controls.Add(pRight);   // Fill — added first
            pGridRow.Controls.Add(pLeft);    // Left — docks to left side

            // Keep left panel at exactly 50% when form is resized
            pGridRow.Resize += (s,e) =>
            {
                if (pGridRow.Width > 20)
                    pLeft.Width = pGridRow.Width / 2 - 5;
            };

            _viewSales.Controls.Add(pGridRow);
        }

        // ── Demographics ──────────────────────────────────────
        private void BuildViewDemographic()
        {
            _viewDemographic = new Panel
            {
                Dock=DockStyle.Fill, BackColor=ColBg,
                Padding=new Padding(14,10,14,10)
            };
            var pt = new Panel { Dock=DockStyle.Top, Height=26, BackColor=Color.Transparent };
            pt.Controls.Add(GridTitle("🌍  Revenue by Region & Spending Tier (BR-06, FR07)"));
            _dgvDemographic=MakeGrid(); _dgvDemographic.Dock=DockStyle.Fill;
            // Add Fill first, then Top (reverse order for Dock stacking)
            _viewDemographic.Controls.Add(_dgvDemographic);
            _viewDemographic.Controls.Add(pt);
        }

        // ── Revenue Trends ────────────────────────────────────
        private void BuildViewTrends()
        {
            _viewTrends = new Panel
            {
                Dock=DockStyle.Fill, BackColor=ColBg,
                Padding=new Padding(14,10,14,10)
            };
            var pt = new Panel { Dock=DockStyle.Top, Height=26, BackColor=Color.Transparent };
            pt.Controls.Add(GridTitle("📈  Revenue Trends — Daily / Weekly / Monthly (FR10)"));
            _dgvTrends=MakeGrid(); _dgvTrends.Dock=DockStyle.Fill;
            _viewTrends.Controls.Add(_dgvTrends);
            _viewTrends.Controls.Add(pt);
        }

        // ── Underperforming ───────────────────────────────────
        private void BuildViewUnderperf()
        {
            _viewUnderperf = new Panel
            {
                Dock=DockStyle.Fill, BackColor=ColBg,
                Padding=new Padding(14,10,14,10)
            };
            var pt = new Panel { Dock=DockStyle.Top, Height=26, BackColor=Color.Transparent };
            pt.Controls.Add(GridTitle("⚠   Underperforming MTX Items (FR11, BR-04, BR-05)"));
            _dgvUnderperf=MakeGrid(); _dgvUnderperf.Dock=DockStyle.Fill;
            _viewUnderperf.Controls.Add(_dgvUnderperf);
            _viewUnderperf.Controls.Add(pt);
        }

        // ═══════════════ DATA REFRESH ════════════════════════

        private void RefreshDashboard()
        {
            var filters = new FilterSet
            {
                DateRange = new DateRange(_dtpFrom.Value, _dtpTo.Value,
                    _cmbGran.SelectedItem?.ToString() ?? "daily"),
                Region         = _cmbRegion.SelectedIndex    > 0 ? _cmbRegion.SelectedItem?.ToString()         : null,
                ItemType       = _cmbItemType.SelectedIndex  > 0 ? (ItemType?)_cmbItemType.SelectedItem        : null,
                CharacterClass = _cmbCharClass.SelectedIndex > 0 ? (CharacterClass?)_cmbCharClass.SelectedItem : null
            };

            _currentReport = _engine.GenerateReport(filters, _txRepo.GetAll(), _itemRepo.GetAll());

            if (_auth.CurrentUser != null)
                _lblUserInfo.Text = $"👤 {_auth.CurrentUser.Username}  [{_auth.CurrentUser.Role}]";

            PopulateSales(_currentReport);
            PopulateDemographic(_currentReport);
            PopulateTrends(_currentReport);
            PopulateUnderperf(_currentReport);

            _btnExport.Visible = _auth.CurrentUser?.CanExport() ?? false;
        }

        // ── Populate: Sales ───────────────────────────────────
        private void PopulateSales(Report r)
        {
            SetCard(_cardRevenue,   $"${r.TotalRevenue:N2}");
            SetCard(_cardTxCount,   $"{r.TotalTransactions:N0}");
            int pct = r.TotalTransactions > 0
                      ? (int)(100f * r.BundleSplit.BundleCount / r.TotalTransactions) : 0;
            SetCard(_cardBundlePct, $"{pct}%");
            var top = r.TopByCategory.Values.OrderByDescending(v=>v.Count)
                                            .Select(v=>v.ItemName).FirstOrDefault() ?? "—";
            SetCard(_cardTopItem, top);

            // BR-01: top sellers per category
            _dgvTopCategory.DataSource = r.TopByCategory
                .Select(kv=>new{ Category=kv.Key.ToString(),
                                 TopItem=kv.Value.ItemName,
                                 Sales=kv.Value.Count })
                .OrderByDescending(x=>x.Sales).ToList();
            AutoCols(_dgvTopCategory);

            // BR-02: top sellers per character class
            _dgvTopClass.DataSource = r.TopByClass
                .Select(kv=>new{ Class=kv.Key.ToString(),
                                 TopItem=kv.Value.ItemName,
                                 Sales=kv.Value.Count })
                .OrderByDescending(x=>x.Sales).ToList();
            AutoCols(_dgvTopClass);
        }

        // ── Populate: Demographics ────────────────────────────
        private void PopulateDemographic(Report r)
        {
            _dgvDemographic.DataSource = r.RevenueByRegion
                .Select(kv=>new
                {
                    Region    = kv.Key,
                    Revenue   = $"${kv.Value:N2}",
                    Casual    = r.TierDistribution.GetValueOrDefault(SpendingTier.Casual,    0),
                    Regular   = r.TierDistribution.GetValueOrDefault(SpendingTier.Regular,   0),
                    HighValue = r.TierDistribution.GetValueOrDefault(SpendingTier.HighValue, 0)
                })
                .OrderByDescending(x=>x.Revenue).ToList();
            AutoCols(_dgvDemographic);
        }

        // ── Populate: Trends ──────────────────────────────────
        // Exactly two columns: Period and Revenue (no extra props = no extra columns)
        private void PopulateTrends(Report r)
        {
            _dgvTrends.DataSource = r.RevenueTrend
                .Select(kv=>new{ Period=kv.Key, Revenue=$"${kv.Value:N2}" })
                .OrderBy(x=>x.Period).ToList();
            AutoCols(_dgvTrends);
        }

        // ── Populate: Underperforming ─────────────────────────
        private void PopulateUnderperf(Report r)
        {
            _dgvUnderperf.DataSource = r.UnderperformingItems
                .Select(u=>new{ Item=u.ItemName, Category=u.Type.ToString(),
                                Sales=u.Sales,
                                Status=u.Sales==0?"🔴 No Sales":"🟡 Low Sales" })
                .ToList();
            AutoCols(_dgvUnderperf);

            // Colour zero-sale rows red
            foreach (DataGridViewRow row in _dgvUnderperf.Rows)
                if (row.Cells["Sales"].Value is int s && s==0)
                    row.DefaultCellStyle.ForeColor=Color.FromArgb(248,113,113);
        }

        // ═══════════════ ACTIONS ═════════════════════════════

        private void OpenAddTx()
        {
            if (_auth.CurrentUser?.CanConfigure()==false)
            {
                MessageBox.Show("Your role does not permit recording purchases.",
                    "Access Denied",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            using var dlg=new AddTransactionForm(_txService,_itemRepo,_players);
            if (dlg.ShowDialog(this)==DialogResult.OK)
            {
                RefreshDashboard();
                MessageBox.Show($"Purchase recorded:\n{dlg.RecordedTransaction}",
                    "Success",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        private void OpenExport()
        {
            if (_currentReport==null){RefreshDashboard();return;}
            var fmt=ChooseFormat();
            if (fmt==null) return;
            using var dlg=new SaveFileDialog
            {
                Title="Export Report",
                Filter=fmt switch
                {
                    ExportFormat.CSV  =>"CSV Files|*.csv",
                    ExportFormat.JSON =>"JSON Files|*.json",
                    _                 =>"Text Files|*.txt"
                },
                FileName=$"GGG_Report_{DateTime.Now:yyyyMMdd_HHmm}"
            };
            if (dlg.ShowDialog(this)==DialogResult.OK)
            {
                try
                {
                    _engine.ExportReport(_currentReport,fmt.Value,dlg.FileName);
                    MessageBox.Show($"Exported:\n{dlg.FileName}","Done",
                        MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}","Error",
                        MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }

        private ExportFormat? ChooseFormat()
        {
            using var dlg=new Form
            {
                Text="Export Format",Size=new Size(260,186),
                StartPosition=FormStartPosition.CenterParent,
                FormBorderStyle=FormBorderStyle.FixedDialog,
                MaximizeBox=false,BackColor=ColBg
            };
            ExportFormat? chosen=null;
            int y=18;
            foreach (ExportFormat f in Enum.GetValues<ExportFormat>())
            {
                var fmt=f;
                var b=new Button
                {
                    Text=fmt.ToString(),
                    Font=new Font("Segoe UI",10,FontStyle.Bold),
                    BackColor=ColHeader,ForeColor=Color.White,
                    FlatStyle=FlatStyle.Flat,
                    Size=new Size(210,34),Location=new Point(22,y),Cursor=Cursors.Hand
                };
                b.FlatAppearance.BorderSize=0;
                b.Click+=(_,__)=>{chosen=fmt;dlg.Close();};
                dlg.Controls.Add(b);
                y+=40;
            }
            dlg.ShowDialog(this);
            return chosen;
        }

        private void Logout(){_auth.Logout();Close();}

        // ═══════════════ SIDEBAR TOGGLE

        // Slides the sidebar in or out and flips the arrow direction
        private void ToggleSidebar(object? sender, EventArgs e)
        {
            _sidebarVisible = !_sidebarVisible;

            // Show or hide the sidebar panel
            _pnlSidebar.Visible = _sidebarVisible;

            // Flip the arrow: ◀ when open (click to close), ▶ when closed (click to open)
            _btnToggle.Text = _sidebarVisible ? "◀" : "▶";
        }

        // ═══════════════ NAVIGATION ══════════════════════════

        private void ShowView(Panel target)
        {
            foreach(var v in new[]{_viewSales,_viewDemographic,_viewTrends,_viewUnderperf})
                _pnlViewHost.Controls.Remove(v);
            _pnlViewHost.Controls.Add(target);
            target.BringToFront();
        }

        private void HighlightNav(Button active)
        {
            foreach(var b in new[]{_btnSales,_btnDemograph,_btnTrends,_btnUnderperf})
            {
                b.BackColor = b==active ? ColHeader : Color.Transparent;
                b.ForeColor = b==active ? Color.White : ColMuted;
            }
        }

        // ═══════════════ FACTORIES ═══════════════════════════

        // Sidebar section heading
        private static Label SideSection(string t, ref int y)
        {
            var l=new Label{Text=t,Font=new Font("Segoe UI",7.5f,FontStyle.Bold),
                ForeColor=Color.FromArgb(71,85,105),AutoSize=true,Location=new Point(14,y)};
            y+=22; return l;
        }

        // Sidebar nav button
        private static Button SideBtn(string t, ref int y)
        {
            var b=new Button
            {
                Text=t,Font=new Font("Segoe UI",9.5f),
                ForeColor=ColMuted,BackColor=Color.Transparent,
                FlatStyle=FlatStyle.Flat,Size=new Size(SW-10,38),
                Location=new Point(5,y),
                TextAlign=ContentAlignment.MiddleLeft,
                Padding=new Padding(8,0,0,0),Cursor=Cursors.Hand
            };
            b.FlatAppearance.BorderSize=0; y+=40; return b;
        }

        // Filter-bar combo box
        private static ComboBox FiltCombo(int x, int w)=>new ComboBox
        {
            Font=new Font("Segoe UI",8.5f),
            BackColor=Color.FromArgb(30,41,59),ForeColor=Color.White,
            DropDownStyle=ComboBoxStyle.DropDownList,
            Size=new Size(w,26),Location=new Point(x,30)
        };

        // KPI stat card panel
        private static Panel MakeCard(string title,string val,int x)
        {
            var p=new Panel{Size=new Size(196,88),Location=new Point(x,2),
                            BackColor=Color.FromArgb(22,32,50)};
            p.Controls.Add(new Label{Text=title,Font=new Font("Segoe UI",8),
                ForeColor=Color.FromArgb(148,180,212),AutoSize=true,Location=new Point(12,10)});
            p.Controls.Add(new Label{Text=val,Font=new Font("Segoe UI",17,FontStyle.Bold),
                ForeColor=Color.FromArgb(251,146,60),AutoSize=true,
                Location=new Point(12,32),Tag="v"});
            return p;
        }

        // Update the orange value label inside a stat card
        private static void SetCard(Panel card,string val)
        {
            foreach(Control c in card.Controls)
                if(c is Label l&&(string?)l.Tag=="v") l.Text=val;
        }

        // Dark-themed DataGridView
        private static DataGridView MakeGrid()
        {
            var g=new DataGridView
            {
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible     = false,
                BackgroundColor       = Color.FromArgb(22,32,50),
                BorderStyle           = BorderStyle.None,
                ColumnHeadersHeight   = 34,
                RowTemplate           = {Height=28},
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                Font                  = new Font("Segoe UI",9),
                ForeColor             = Color.FromArgb(226,232,240),
                GridColor             = Color.FromArgb(40,55,75)
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30,58,95);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI",9,FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding   = new Padding(6,0,0,0);
            g.EnableHeadersVisualStyles               = false;
            g.DefaultCellStyle.BackColor          = Color.FromArgb(22,32,50);
            g.DefaultCellStyle.ForeColor          = Color.FromArgb(226,232,240);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30,58,95);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.Padding            = new Padding(6,0,0,0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26,38,60);
            return g;
        }

        // Set all columns to Fill mode with a tight minimum width
        private static void AutoCols(DataGridView g)
        {
            g.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
            foreach(DataGridViewColumn c in g.Columns)
            { c.SortMode=DataGridViewColumnSortMode.Automatic; c.MinimumWidth=60; }
        }

        // Section heading above a grid
        private static Label GridTitle(string t)=>new Label
        {
            Text=t,Font=new Font("Segoe UI",9.5f,FontStyle.Bold),
            ForeColor=Color.FromArgb(148,180,212),AutoSize=true,Location=new Point(0,4)
        };
    }
}

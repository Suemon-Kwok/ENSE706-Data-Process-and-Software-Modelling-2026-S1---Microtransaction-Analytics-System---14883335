// =============================================================
// MainDashboardForm.cs — AnalyticsDashboard (UML)
// The primary application window. Contains all dashboard views:
//   • Sales Overview    (BR-01, BR-02, BR-03)
//   • Demographic View  (FR06, FR07, FR08, BR-06)
//   • Revenue Trends    (FR10)
//   • Underperforming   (FR11, BR-04, BR-05)
//   • Add Transaction   (FR01–FR05)
//   • Export Report     (FR12)
// Role-based controls enforced throughout (FR14).
// =============================================================

namespace GGG_MAS.Forms
{
    using GGG_MAS.Models;
    using GGG_MAS.Repositories;
    using GGG_MAS.Services;

    /// <summary>
    /// Main application dashboard. All analytics are displayed here.
    /// Navigation uses a left sidebar with tab-panel switching.
    /// </summary>
    public class MainDashboardForm : Form
    {
        // ── Layout panels ─────────────────────────────────────
        private Panel  _pnlSidebar   = null!;   // left navigation
        private Panel  _pnlContent   = null!;   // right content area
        private Panel  _pnlTopBar    = null!;   // top title bar

        // ── Sidebar nav buttons ───────────────────────────────
        private Button _btnSales     = null!;
        private Button _btnDemograph = null!;
        private Button _btnTrends    = null!;
        private Button _btnUnderperf = null!;
        private Button _btnAddTx     = null!;
        private Button _btnExport    = null!;
        private Button _btnLogout    = null!;
        private Label  _lblUserInfo  = null!;

        // ── Filter controls (FR09) ────────────────────────────
        private ComboBox  _cmbRegion    = null!;
        private ComboBox  _cmbItemType  = null!;
        private ComboBox  _cmbCharClass = null!;
        private ComboBox  _cmbGran      = null!;
        private DateTimePicker _dtpFrom = null!;
        private DateTimePicker _dtpTo   = null!;
        private Button    _btnRefresh   = null!;

        // ── Content panels (one per view) ─────────────────────
        private Panel _viewSales       = null!;
        private Panel _viewDemographic = null!;
        private Panel _viewTrends      = null!;
        private Panel _viewUnderperf   = null!;

        // ── Data grids / list controls ────────────────────────
        private DataGridView _dgvTopCategory   = null!;
        private DataGridView _dgvTopClass      = null!;
        private DataGridView _dgvDemographic   = null!;
        private DataGridView _dgvTrends        = null!;
        private DataGridView _dgvUnderperf     = null!;

        // ── Summary stat labels ───────────────────────────────
        private Label _lblRevenue    = null!;
        private Label _lblTxCount    = null!;
        private Label _lblBundlePct  = null!;
        private Label _lblTopItem    = null!;

        // ── Services & data ───────────────────────────────────
        private readonly AuthService           _auth;
        private readonly ReportEngine          _engine;
        private readonly ITransactionRepository _txRepo;
        private readonly IItemRepository        _itemRepo;
        private readonly TransactionService    _txService;
        private readonly List<PlayerAccount>   _players;

        // Currently loaded report data
        private Report? _currentReport;

        // GGG brand colours
        private static readonly Color ColBg       = Color.FromArgb(18, 24, 38);
        private static readonly Color ColSidebar  = Color.FromArgb(12, 18, 30);
        private static readonly Color ColAccent   = Color.FromArgb(234, 88, 12);
        private static readonly Color ColHeader   = Color.FromArgb(30, 58, 95);
        private static readonly Color ColText     = Color.FromArgb(226, 232, 240);
        private static readonly Color ColMuted    = Color.FromArgb(100, 116, 139);
        private static readonly Color ColGrid     = Color.FromArgb(30, 41, 59);
        private static readonly Color ColSuccess  = Color.FromArgb(21, 128, 61);

        public MainDashboardForm(AuthService auth, ReportEngine engine,
                                 ITransactionRepository txRepo,
                                 IItemRepository itemRepo,
                                 TransactionService txService,
                                 List<PlayerAccount> players)
        {
            _auth      = auth;
            _engine    = engine;
            _txRepo    = txRepo;
            _itemRepo  = itemRepo;
            _txService = txService;
            _players   = players;

            InitialiseComponents();
            Load += (s, e) => RefreshDashboard();   // load data on open
        }

        // ═══════════════════ LAYOUT INIT ═════════════════════

        private void InitialiseComponents()
        {
            Text            = "GGG — Microtransaction Analytics System";
            Size            = new Size(1280, 800);
            MinimumSize     = new Size(1100, 680);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = ColBg;

            BuildTopBar();
            BuildSidebar();
            BuildContentArea();
            BuildViewSales();
            BuildViewDemographic();
            BuildViewTrends();
            BuildViewUnderperf();

            // Sales is the default starting view
            ShowView(_viewSales);
            HighlightNav(_btnSales);
        }

        // ── Top title bar ─────────────────────────────────────
        private void BuildTopBar()
        {
            _pnlTopBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 50,
                BackColor = ColHeader
            };

            var title = new Label
            {
                Text      = "⚙  GGG Microtransaction Analytics System",
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = ColAccent,
                AutoSize  = true,
                Location  = new Point(14, 12)
            };

            _lblUserInfo = new Label
            {
                Font      = new Font("Segoe UI", 9),
                ForeColor = ColMuted,
                AutoSize  = true,
                Location  = new Point(1050, 17)
            };

            _pnlTopBar.Controls.Add(title);
            _pnlTopBar.Controls.Add(_lblUserInfo);
            Controls.Add(_pnlTopBar);
        }

        // ── Left navigation sidebar ───────────────────────────
        private void BuildSidebar()
        {
            _pnlSidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 200,
                BackColor = ColSidebar,
                Padding   = new Padding(0, 10, 0, 0)
            };

            int y = 10;

            // Section label: Analytics
            _pnlSidebar.Controls.Add(MakeSectionLabel("ANALYTICS", ref y));
            _btnSales     = MakeNavButton("📊  Sales Overview",   ref y); _pnlSidebar.Controls.Add(_btnSales);
            _btnDemograph = MakeNavButton("🌍  Demographics",     ref y); _pnlSidebar.Controls.Add(_btnDemograph);
            _btnTrends    = MakeNavButton("📈  Revenue Trends",   ref y); _pnlSidebar.Controls.Add(_btnTrends);
            _btnUnderperf = MakeNavButton("⚠   Underperforming", ref y); _pnlSidebar.Controls.Add(_btnUnderperf);
            y += 10;

            // Section label: Actions
            _pnlSidebar.Controls.Add(MakeSectionLabel("ACTIONS", ref y));
            _btnAddTx  = MakeNavButton("➕  Record Purchase", ref y); _pnlSidebar.Controls.Add(_btnAddTx);
            _btnExport = MakeNavButton("📁  Export Report",   ref y); _pnlSidebar.Controls.Add(_btnExport);
            y += 20;

            // Logout at bottom
            _btnLogout = new Button
            {
                Text      = "⏻  Logout",
                Font      = new Font("Segoe UI", 9),
                ForeColor = ColMuted,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(190, 34),
                Location  = new Point(5, y),
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0)
            };
            _btnLogout.FlatAppearance.BorderSize = 0;
            _pnlSidebar.Controls.Add(_btnLogout);

            // ── Wire navigation events ─────────────────────────
            _btnSales.Click     += (s, e) => { ShowView(_viewSales);       HighlightNav(_btnSales);     };
            _btnDemograph.Click += (s, e) => { ShowView(_viewDemographic); HighlightNav(_btnDemograph); };
            _btnTrends.Click    += (s, e) => { ShowView(_viewTrends);      HighlightNav(_btnTrends);    };
            _btnUnderperf.Click += (s, e) => { ShowView(_viewUnderperf);   HighlightNav(_btnUnderperf); };
            _btnAddTx.Click     += (s, e) => OpenAddTransactionDialog();
            _btnExport.Click    += (s, e) => OpenExportDialog();
            _btnLogout.Click    += (s, e) => Logout();

            Controls.Add(_pnlSidebar);
        }

        // ── Right content area (holds filter bar + view panels) ──
        private void BuildContentArea()
        {
            _pnlContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColBg,
                Padding   = new Padding(16)
            };
            Controls.Add(_pnlContent);

            BuildFilterBar();
        }

        // ── Filter bar (FR09) ─────────────────────────────────
        private void BuildFilterBar()
        {
            var pnlFilters = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 60,
                BackColor = Color.FromArgb(22, 32, 50),
                Padding   = new Padding(8, 6, 8, 6)
            };

            int x = 8;

            // Date from
            pnlFilters.Controls.Add(MakeFilterLabel("From:", x, 10));
            _dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short,
                Location = new Point(x + 38, 8), Width = 100,
                Value = DateTime.Today.AddDays(-30) };
            _dtpFrom.BackColor = ColGrid;
            pnlFilters.Controls.Add(_dtpFrom); x += 148;

            // Date to
            pnlFilters.Controls.Add(MakeFilterLabel("To:", x, 10));
            _dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Short,
                Location = new Point(x + 24, 8), Width = 100,
                Value = DateTime.Today };
            pnlFilters.Controls.Add(_dtpTo); x += 134;

            // Region
            pnlFilters.Controls.Add(MakeFilterLabel("Region:", x, 10));
            _cmbRegion = MakeFilterCombo(x + 52, 110);
            _cmbRegion.Items.Add("All Regions");
            foreach (var r in new[] { "NZ", "AU", "US", "EU", "APAC" })
                _cmbRegion.Items.Add(r);
            _cmbRegion.SelectedIndex = 0;
            pnlFilters.Controls.Add(_cmbRegion); x += 172;

            // Item type
            pnlFilters.Controls.Add(MakeFilterLabel("Type:", x, 10));
            _cmbItemType = MakeFilterCombo(x + 42, 120);
            _cmbItemType.Items.Add("All Types");
            foreach (ItemType t in Enum.GetValues<ItemType>())
                _cmbItemType.Items.Add(t);
            _cmbItemType.SelectedIndex = 0;
            pnlFilters.Controls.Add(_cmbItemType); x += 172;

            // Character class
            pnlFilters.Controls.Add(MakeFilterLabel("Class:", x, 10));
            _cmbCharClass = MakeFilterCombo(x + 44, 110);
            _cmbCharClass.Items.Add("All Classes");
            foreach (CharacterClass c in Enum.GetValues<CharacterClass>())
                _cmbCharClass.Items.Add(c);
            _cmbCharClass.SelectedIndex = 0;
            pnlFilters.Controls.Add(_cmbCharClass); x += 164;

            // Granularity
            pnlFilters.Controls.Add(MakeFilterLabel("Group:", x, 10));
            _cmbGran = MakeFilterCombo(x + 46, 90);
            _cmbGran.Items.AddRange(new object[] { "daily", "weekly", "monthly" });
            _cmbGran.SelectedIndex = 0;
            pnlFilters.Controls.Add(_cmbGran); x += 146;

            // Refresh button
            _btnRefresh = new Button
            {
                Text      = "↻  Apply",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = ColAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(80, 30),
                Location  = new Point(x, 6),
                Cursor    = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += (s, e) => RefreshDashboard();
            pnlFilters.Controls.Add(_btnRefresh);

            _pnlContent.Controls.Add(pnlFilters);
        }

        // ═══════════════════ VIEW BUILDERS ═══════════════════

        // ── Sales Overview view ───────────────────────────────
        private void BuildViewSales()
        {
            _viewSales = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            // KPI stat cards row
            var pnlStats = new Panel { Dock = DockStyle.Top, Height = 100 };
            pnlStats.BackColor = Color.Transparent;

            _lblRevenue   = MakeStatCard("Total Revenue",    "$0.00",     0);
            _lblTxCount   = MakeStatCard("Transactions",     "0",         210);
            _lblBundlePct = MakeStatCard("Bundle %",         "0%",        420);
            _lblTopItem   = MakeStatCard("Top Item",         "—",         630);

            pnlStats.Controls.AddRange(new Control[]
                { _lblRevenue, _lblTxCount, _lblBundlePct, _lblTopItem });
            _viewSales.Controls.Add(pnlStats);

            // Two grids side by side: Top by Category | Top by Class
            var pnlGrids = new Panel { Dock = DockStyle.Fill };
            pnlGrids.BackColor = Color.Transparent;

            var lblCat   = MakeGridTitle("🏆  Top Sellers by Category (BR-01)");
            lblCat.Location  = new Point(0, 8);
            _dgvTopCategory  = MakeDataGrid();
            _dgvTopCategory.Location = new Point(0, 34);
            _dgvTopCategory.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

            var lblCls   = MakeGridTitle("🎮  Top Sellers by Character Class (BR-02)");
            lblCls.Location  = new Point(480, 8);
            _dgvTopClass     = MakeDataGrid();
            _dgvTopClass.Location = new Point(480, 34);
            _dgvTopClass.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

            pnlGrids.Controls.AddRange(new Control[]
                { lblCat, _dgvTopCategory, lblCls, _dgvTopClass });
            _viewSales.Controls.Add(pnlGrids);
        }

        // ── Demographics view ─────────────────────────────────
        private void BuildViewDemographic()
        {
            _viewDemographic = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            var lbl = MakeGridTitle("🌍  Revenue by Region & Spending Tier (BR-06, FR07)");
            lbl.Location = new Point(0, 10);
            _dgvDemographic = MakeDataGrid();
            _dgvDemographic.Location = new Point(0, 40);
            _dgvDemographic.Anchor   = AnchorStyles.Top | AnchorStyles.Left |
                                       AnchorStyles.Bottom | AnchorStyles.Right;
            _viewDemographic.Controls.Add(lbl);
            _viewDemographic.Controls.Add(_dgvDemographic);
        }

        // ── Revenue Trends view ───────────────────────────────
        private void BuildViewTrends()
        {
            _viewTrends = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            var lbl = MakeGridTitle("📈  Revenue Trends — Daily / Weekly / Monthly (FR10)");
            lbl.Location = new Point(0, 10);
            _dgvTrends   = MakeDataGrid();
            _dgvTrends.Location = new Point(0, 40);
            _dgvTrends.Anchor   = AnchorStyles.Top | AnchorStyles.Left |
                                  AnchorStyles.Bottom | AnchorStyles.Right;
            _viewTrends.Controls.Add(lbl);
            _viewTrends.Controls.Add(_dgvTrends);
        }

        // ── Underperforming view ──────────────────────────────
        private void BuildViewUnderperf()
        {
            _viewUnderperf = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            var lbl = MakeGridTitle("⚠   Underperforming MTX Items (FR11, BR-04, BR-05)");
            lbl.Location = new Point(0, 10);
            _dgvUnderperf = MakeDataGrid();
            _dgvUnderperf.Location = new Point(0, 40);
            _dgvUnderperf.Anchor   = AnchorStyles.Top | AnchorStyles.Left |
                                     AnchorStyles.Bottom | AnchorStyles.Right;
            _viewUnderperf.Controls.Add(lbl);
            _viewUnderperf.Controls.Add(_dgvUnderperf);
        }

        // ═══════════════════ DATA REFRESH ════════════════════

        /// <summary>
        /// Reads current filter values, runs the report engine,
        /// and populates all view grids with fresh data.
        /// </summary>
        private void RefreshDashboard()
        {
            // Build filter set from UI controls (FR09)
            var filters = new FilterSet
            {
                DateRange = new DateRange(_dtpFrom.Value, _dtpTo.Value,
                                          _cmbGran.SelectedItem?.ToString() ?? "daily"),
                Region    = _cmbRegion.SelectedIndex  > 0 ? _cmbRegion.SelectedItem?.ToString()  : null,
                ItemType  = _cmbItemType.SelectedIndex > 0 ? (ItemType?)_cmbItemType.SelectedItem : null,
                CharacterClass = _cmbCharClass.SelectedIndex > 0
                                 ? (CharacterClass?)_cmbCharClass.SelectedItem : null
            };

            // Run the report engine
            _currentReport = _engine.GenerateReport(filters,
                                                     _txRepo.GetAll(),
                                                     _itemRepo.GetAll());

            // Update top bar user info
            if (_auth.CurrentUser != null)
                _lblUserInfo.Text = $"👤 {_auth.CurrentUser.Username}  [{_auth.CurrentUser.Role}]";

            // Populate each view with the fresh report data
            PopulateSalesView(_currentReport);
            PopulateDemographicView(_currentReport);
            PopulateTrendsView(_currentReport);
            PopulateUnderperfView(_currentReport);

            // Show/hide export button based on user role (FR14)
            _btnExport.Visible = _auth.CurrentUser?.CanExport() ?? false;
        }

        // ── Populate: Sales Overview ──────────────────────────
        private void PopulateSalesView(Report r)
        {
            // Update KPI stat cards
            UpdateStatCard(_lblRevenue,   $"${r.TotalRevenue:N2}");
            UpdateStatCard(_lblTxCount,   $"{r.TotalTransactions:N0}");
            int bundlePct = r.TotalTransactions > 0
                            ? (int)(100f * r.BundleSplit.BundleCount / r.TotalTransactions) : 0;
            UpdateStatCard(_lblBundlePct, $"{bundlePct}%");

            // Find single highest-revenue item for BR-03
            var topOverall = r.TopByCategory.Values
                              .OrderByDescending(v => v.Count)
                              .Select(v => v.ItemName)
                              .FirstOrDefault() ?? "—";
            UpdateStatCard(_lblTopItem, topOverall);

            // Top by Category grid (BR-01)
            _dgvTopCategory.DataSource = r.TopByCategory
                .Select(kv => new { Category = kv.Key.ToString(),
                                    TopItem = kv.Value.ItemName,
                                    UnitsSold = kv.Value.Count })
                .OrderByDescending(x => x.UnitsSold)
                .ToList();
            StyleGrid(_dgvTopCategory);
            ResizeGrid(_dgvTopCategory, 460);

            // Top by Class grid (BR-02)
            _dgvTopClass.DataSource = r.TopByClass
                .Select(kv => new { Class = kv.Key.ToString(),
                                    TopItem = kv.Value.ItemName,
                                    UnitsSold = kv.Value.Count })
                .OrderByDescending(x => x.UnitsSold)
                .ToList();
            StyleGrid(_dgvTopClass);
            ResizeGrid(_dgvTopClass, 450);
        }

        // ── Populate: Demographics ────────────────────────────
        private void PopulateDemographicView(Report r)
        {
            // Combine region revenue and tier distribution into one display
            var rows = r.RevenueByRegion
                .Select(kv => new
                {
                    Region    = kv.Key,
                    Revenue   = $"${kv.Value:N2}",
                    Casual    = r.TierDistribution.GetValueOrDefault(SpendingTier.Casual, 0),
                    Regular   = r.TierDistribution.GetValueOrDefault(SpendingTier.Regular, 0),
                    HighValue = r.TierDistribution.GetValueOrDefault(SpendingTier.HighValue, 0)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            _dgvDemographic.DataSource = rows;
            StyleGrid(_dgvDemographic);
        }

        // ── Populate: Revenue Trends ──────────────────────────
        private void PopulateTrendsView(Report r)
        {
            _dgvTrends.DataSource = r.RevenueTrend
                .Select(kv => new { Period = kv.Key, Revenue = $"${kv.Value:N2}",
                                    RawRevenue = kv.Value })
                .OrderBy(x => x.Period)
                .ToList();
            StyleGrid(_dgvTrends);
        }

        // ── Populate: Underperforming ─────────────────────────
        private void PopulateUnderperfView(Report r)
        {
            _dgvUnderperf.DataSource = r.UnderperformingItems
                .Select(u => new { Item = u.ItemName, Category = u.Type.ToString(),
                                   Sales = u.Sales, Status = u.Sales == 0 ? "🔴 No Sales" : "🟡 Low Sales" })
                .ToList();
            StyleGrid(_dgvUnderperf);

            // Colour rows red for zero-sales items
            foreach (DataGridViewRow row in _dgvUnderperf.Rows)
                if (row.Cells["Sales"].Value is int s && s == 0)
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(248, 113, 113);
        }

        // ═══════════════════ ACTIONS ═════════════════════════

        // ── Record Purchase dialog ─────────────────────────────
        private void OpenAddTransactionDialog()
        {
            // Only Analysts and Developers can add transactions (FR14)
            if (_auth.CurrentUser?.CanConfigure() == false)
            {
                MessageBox.Show("Your role does not have permission to record purchases.",
                                "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new AddTransactionForm(_txService, _itemRepo, _players);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                RefreshDashboard();   // reload all views after a new purchase
                MessageBox.Show($"Purchase recorded: {dlg.RecordedTransaction}",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ── Export dialog (FR12, BR-09) ────────────────────────
        private void OpenExportDialog()
        {
            if (_currentReport == null) { RefreshDashboard(); return; }

            // Choose export format
            var fmt = ChooseExportFormat();
            if (fmt == null) return;

            // Choose save file location
            using var dlg = new SaveFileDialog
            {
                Title    = "Export Report",
                Filter   = fmt switch
                {
                    ExportFormat.CSV  => "CSV Files|*.csv",
                    ExportFormat.JSON => "JSON Files|*.json",
                    _                 => "Text Files|*.txt"
                },
                FileName = $"GGG_MAS_Report_{DateTime.Now:yyyyMMdd_HHmm}"
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _engine.ExportReport(_currentReport, fmt.Value, dlg.FileName);
                    MessageBox.Show($"Report exported to:\n{dlg.FileName}",
                                    "Export Complete",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}",
                                    "Export Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Ask the user which format to export in
        private ExportFormat? ChooseExportFormat()
        {
            using var dlg = new Form
            {
                Text            = "Select Export Format",
                Size            = new Size(280, 170),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                BackColor       = ColBg
            };

            ExportFormat? chosen = null;

            int y = 20;
            foreach (ExportFormat f in Enum.GetValues<ExportFormat>())
            {
                var fmt = f;   // capture for lambda
                var btn = new Button
                {
                    Text      = fmt.ToString(),
                    Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                    BackColor = ColHeader,
                    ForeColor = ColText,
                    FlatStyle = FlatStyle.Flat,
                    Size      = new Size(230, 32),
                    Location  = new Point(18, y),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => { chosen = fmt; dlg.Close(); };
                dlg.Controls.Add(btn);
                y += 38;
            }

            dlg.ShowDialog(this);
            return chosen;
        }

        // ── Logout ────────────────────────────────────────────
        private void Logout()
        {
            _auth.Logout();
            Close();
        }

        // ═══════════════════ UI HELPERS ══════════════════════

        // Shows one content panel, hides all others
        private void ShowView(Panel target)
        {
            // Remove any previous view from content panel
            _pnlContent.Controls.Remove(_viewSales);
            _pnlContent.Controls.Remove(_viewDemographic);
            _pnlContent.Controls.Remove(_viewTrends);
            _pnlContent.Controls.Remove(_viewUnderperf);

            // Add and show the requested view
            _pnlContent.Controls.Add(target);
            target.BringToFront();
        }

        // Highlight active sidebar button; reset others
        private void HighlightNav(Button active)
        {
            var navBtns = new[] { _btnSales, _btnDemograph, _btnTrends, _btnUnderperf };
            foreach (var b in navBtns)
            {
                b.BackColor = b == active
                              ? Color.FromArgb(30, 58, 95)
                              : Color.Transparent;
                b.ForeColor = b == active ? Color.White : ColMuted;
            }
        }

        // Creates a KPI stat card panel with two stacked labels
        private static Label MakeStatCard(string title, string value, int x)
        {
            var pnl = new Panel
            {
                Size      = new Size(200, 90),
                Location  = new Point(x, 4),
                BackColor = Color.FromArgb(22, 32, 50)
            };

            var lTitle = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(148, 180, 212),
                AutoSize  = true,
                Location  = new Point(12, 10)
            };
            var lValue = new Label
            {
                Text      = value,
                Font      = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(251, 146, 60),
                AutoSize  = true,
                Location  = new Point(12, 32),
                Tag       = "value"   // used by UpdateStatCard to find this label
            };

            pnl.Controls.Add(lTitle);
            pnl.Controls.Add(lValue);
            return pnl;
        }

        // Finds the value label inside a stat card and updates its text
        private static void UpdateStatCard(Label card, string newValue)
        {
            // The card is actually a Panel — iterate children to find the value label
            if (card is Panel pnl)
            {
                foreach (Control c in pnl.Controls)
                    if (c is Label lbl && (string?)lbl.Tag == "value")
                        lbl.Text = newValue;
            }
        }

        // Styled DataGridView factory
        private static DataGridView MakeDataGrid()
        {
            var dgv = new DataGridView
            {
                AutoSizeColumnsMode  = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor      = Color.FromArgb(22, 32, 50),
                BorderStyle          = BorderStyle.None,
                ColumnHeadersHeight  = 32,
                RowTemplate          = { Height = 28 },
                ReadOnly             = true,
                AllowUserToAddRows   = false,
                AllowUserToDeleteRows= false,
                SelectionMode        = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect          = false,
                Font                 = new Font("Segoe UI", 9),
                ForeColor            = Color.FromArgb(226, 232, 240),
                GridColor            = Color.FromArgb(40, 55, 75),
                Size                 = new Size(460, 400)
            };

            // Header style
            dgv.ColumnHeadersDefaultCellStyle.BackColor   = Color.FromArgb(30, 58, 95);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor   = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font        = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding     = new Padding(6, 0, 0, 0);
            dgv.EnableHeadersVisualStyles = false;

            // Row styles (alternating)
            dgv.DefaultCellStyle.BackColor          = Color.FromArgb(22, 32, 50);
            dgv.DefaultCellStyle.ForeColor          = Color.FromArgb(226, 232, 240);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 58, 95);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Padding            = new Padding(6, 0, 0, 0);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26, 38, 60);

            return dgv;
        }

        // Apply column styling after DataSource is set
        private static void StyleGrid(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
                col.MinimumWidth = 80;
            }
        }

        // Resize a grid to a fixed width, keeping height flexible
        private static void ResizeGrid(DataGridView dgv, int width)
        {
            dgv.Width = width;
            dgv.Height = 400;
        }

        // Section label in the sidebar
        private static Label MakeSectionLabel(string text, ref int y)
        {
            var lbl = new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize  = true,
                Location  = new Point(14, y)
            };
            y += 22;
            return lbl;
        }

        // Navigation button in the sidebar
        private static Button MakeNavButton(string text, ref int y)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(190, 38),
                Location  = new Point(5, y),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            y += 40;
            return btn;
        }

        // Small label above a filter combo
        private static Label MakeFilterLabel(string text, int x, int y) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 7.5f),
            ForeColor = Color.FromArgb(148, 180, 212),
            AutoSize  = true,
            Location  = new Point(x, y)
        };

        // Compact combo for the filter bar
        private static ComboBox MakeFilterCombo(int x, int width) => new ComboBox
        {
            Font          = new Font("Segoe UI", 8.5f),
            BackColor     = Color.FromArgb(30, 41, 59),
            ForeColor     = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size          = new Size(width, 24),
            Location      = new Point(x, 28)
        };

        // Section/column heading label on a view panel
        private static Label MakeGridTitle(string text) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(148, 180, 212),
            AutoSize  = true
        };
    }
}

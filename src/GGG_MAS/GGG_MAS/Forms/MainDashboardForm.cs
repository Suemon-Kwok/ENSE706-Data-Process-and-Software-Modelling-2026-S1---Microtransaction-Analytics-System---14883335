
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


namespace GGG_MAS.Forms
{
    using GGG_MAS.Models;                                                                                                       // brings in Report, FilterSet, DateRange, and all model types

    using GGG_MAS.Repositories;                                                                                                 // brings in ITransactionRepository and IItemRepository

    using GGG_MAS.Services;                                                                                                     // brings in AuthService, ReportEngine, TransactionService

    public class MainDashboardForm : Form
    {
        // Layout panels
        private Panel  _pnlSidebar  = null!;                                                                                    // left navigation sidebar (180px wide)

        private Panel  _pnlMain     = null!;                                                                                    // right-hand content area (everything to the right of the sidebar)

        private Panel  _pnlTopBar   = null!;                                                                                    // title bar at the top of _pnlMain

        private Panel  _pnlFilters  = null!;                                                                                    // filter bar below the title bar (From/To/Region/Type/Class/Group + Apply)

        private Panel  _pnlViewHost = null!;                                                                                    // fills the remaining space; holds whichever view panel is currently active


        // Sidebar navigation buttons
        private Button _btnSales     = null!;                                                                                   // switches to the Sales Overview view

        private Button _btnDemograph = null!;                                                                                   // switches to the Demographics view

        private Button _btnTrends    = null!;                                                                                   // switches to the Revenue Trends view

        private Button _btnUnderperf = null!;                                                                                   // switches to the Underperforming Items view

        private Button _btnAddTx     = null!;                                                                                   // opens the Add Transaction dialog        

        private Button _btnExport    = null!;                                                                                   // opens the Export format chooser

        private Button _btnLogout    = null!;                                                                                   // logs out and closes the dashboard

        private Label  _lblUserInfo  = null!;                                                                                   // shows the logged-in username and role in the title bar


        // Sidebar collapse/expand toggle
        private Button _btnToggle      = null!;                                                                                 // button that collapses the sidebar

        private bool   _sidebarVisible = true;                                                                                  // tracks whether the sidebar is currently shown


        // Filter bar controls
        private ComboBox       _cmbRegion    = null!;                                                                           // "All Regions" or a specific region code

        private ComboBox       _cmbItemType  = null!;                                                                           // "All Types" or a specific ItemType

        private ComboBox       _cmbCharClass = null!;                                                                           // "All Classes" or a specific CharacterClass

        private ComboBox       _cmbGran      = null!;                                                                           // granularity: daily / weekly / monthly

        private DateTimePicker _dtpFrom      = null!;                                                                           // report start date

        private DateTimePicker _dtpTo        = null!;                                                                           // report end date

        private Button         _btnRefresh   = null!;                                                                           // "↻ Apply" button — regenerates the report


        // View panels (only one is visible at a time)
        private Panel _viewSales       = null!;                                                                                 // Sales Overview panel

        private Panel _viewDemographic = null!;                                                                                 // Demographics panel

        private Panel _viewTrends      = null!;                                                                                 // Revenue Trends panel

        private Panel _viewUnderperf   = null!;                                                                                 // Underperforming Items panel


        // Data grids
        private DataGridView _dgvTopCategory = null!;                                                                           // top sellers by category (BR-01)

        private DataGridView _dgvTopClass    = null!;                                                                           // top sellers by character class (BR-02)

        private DataGridView _dgvDemographic = null!;                                                                           // revenue by region and spending tier (BR-06)

        private DataGridView _dgvTrends      = null!;                                                                           // revenue trend over time (FR10)

        private DataGridView _dgvUnderperf   = null!;                                                                           // underperforming items (FR11)    


        // KPI stat cards
        private Panel _cardRevenue   = null!;                                                                                   // shows total revenue in the current filter window

        private Panel _cardTxCount   = null!;                                                                                   // shows total transaction count

        private Panel _cardBundlePct = null!;                                                                                   // shows the percentage of bundle purchases

        private Panel _cardTopItem   = null!;                                                                                   // shows the name of the single best-selling item


        // Injected services and data
        private readonly AuthService            _auth;                                                                          // used for session info and role-based access checks

        private readonly ReportEngine           _engine;                                                                        // runs all analytics calculations

        private readonly ITransactionRepository _txRepo;                                                                        // provides all stored transactions to the engine

        private readonly IItemRepository        _itemRepo;                                                                      // provides the full item catalogue to the engine

        private readonly TransactionService     _txService;                                                                     // used by AddTransactionForm to record new purchases

        private readonly List<PlayerAccount>    _players;                                                                       // passed to AddTransactionForm for the player dropdown

        private Report? _currentReport;                                                                                         // the most recently generated Report; used by export        

        // Theme colour constants
        private static readonly Color ColBg      = Color.FromArgb(18,  24,  38);                                                // main dark navy background

        private static readonly Color ColSidebar = Color.FromArgb(12,  18,  30);                                                // slightly darker sidebar background

        private static readonly Color ColAccent  = Color.FromArgb(234, 88,  12);                                                // GGG burnt orange — primary accent

        private static readonly Color ColHeader  = Color.FromArgb(30,  58,  95);                                                // deep blue for panels and selected nav

        private static readonly Color ColCard    = Color.FromArgb(22,  32,  50);                                                // card background colour

        private static readonly Color ColMuted   = Color.FromArgb(100, 116, 139);                                               // grey for inactive nav buttons    

        private static readonly Color ColFilt    = Color.FromArgb(22,  32,  50);                                                // filter bar background

        private static readonly Color ColGridRow = Color.FromArgb(22,  32,  50);                                                // default grid row background

        private static readonly Color ColGridAlt = Color.FromArgb(26,  38,  60);                                                // alternating grid row background    

        private static readonly Color ColGridHdr = Color.FromArgb(30,  58,  95);                                                // grid column header background

        private static readonly Color ColGridLn  = Color.FromArgb(40,  55,  75);                                                // grid line colour


        // Fixed sidebar width in pixels
        private const int SW = 180;                                                                                             // all layout calculations use SW to position content to the right of the sidebar


        public MainDashboardForm(AuthService auth, ReportEngine engine,
                                 ITransactionRepository txRepo,
                                 IItemRepository itemRepo,
                                 TransactionService txService,
                                 List<PlayerAccount> players)
        {
            _auth=auth; _engine=engine; _txRepo=txRepo;                                                                         // stores all injected dependencies
            
            _itemRepo =itemRepo; _txService=txService; _players=players;
            
            Build();                                                                                                            // constructs all UI panels and controls
            
            Load += (s,e) => RefreshDashboard();                                                                                // generates the first report when the form opens    
        }

        // BUILD

        // NFR06: Interface uses labelled controls, colour coding, and tooltips —
        
        // usable without training for staff familiar with dashboards
        private void Build()
        {
            Text          = "GGG — Microtransaction Analytics System";                                                          // window title bar
            
            Size          = new Size(1280, 820);                                                                                // default window size
            
            MinimumSize   = new Size(1050, 680);                                                                                // prevents the window being shrunk too small

            StartPosition = FormStartPosition.CenterScreen;                                                                     // opens centred on the monitor

            BackColor     = ColBg;                                                                                              // dark navy window background    

            BuildSidebar();                                                                                                     // builds and adds the left navigation panel

            BuildMain();                                                                                                        // builds and adds the right content area
        }

        // Sidebar
        private void BuildSidebar()
        {
            _pnlSidebar = new Panel { Dock=DockStyle.Left, Width=SW, BackColor=ColSidebar };
            // Dock=Left makes the sidebar stick to the left edge and resize vertically with the form


            int y = 16;                                                                                                         // tracks the current vertical position within the sidebar
            
            _pnlSidebar.Controls.Add(SideSection("ANALYTICS", ref y));                                                          // adds the "ANALYTICS" section heading label

            // Create the four analytics navigation buttons and add them to the sidebar

            _btnSales = SideBtn("📊  Sales Overview",   ref y);
            
            _btnDemograph = SideBtn("🌍  Demographics",     ref y);
            
            _btnTrends    = SideBtn("📈  Revenue Trends",   ref y);
            
            _btnUnderperf = SideBtn("⚠   Underperforming", ref y);
            
            foreach (var b in new[]{_btnSales,_btnDemograph,_btnTrends,_btnUnderperf})
                _pnlSidebar.Controls.Add(b);                                                                                    // adds all four buttons in one loop

            y += 8;                                                                                                             // adds a small gap before the ACTIONS section
            
            _pnlSidebar.Controls.Add(SideSection("ACTIONS", ref y));                                                            // adds the "ACTIONS" section heading label


            // Create the two action buttons and add them to the sidebar

            _btnAddTx = SideBtn("➕  Record Purchase", ref y);
            
            _btnExport= SideBtn("📁  Export Report",   ref y);
            
            _pnlSidebar.Controls.Add(_btnAddTx);
            
            _pnlSidebar.Controls.Add(_btnExport);

            // Logout button — styled differently (muted, no section heading)

            _btnLogout = new Button
            {
                Text="⏻  Logout", Font=new Font("Segoe UI",9),
                
                ForeColor=ColMuted, BackColor=Color.Transparent,
                
                FlatStyle=FlatStyle.Flat, Size=new Size(SW-10,34),
                
                Location=new Point(5,460),
                
                TextAlign=ContentAlignment.MiddleLeft,                                                                          // left-aligns text like other nav buttons

                Padding =new Padding(8,0,0,0), Cursor=Cursors.Hand
            };
            _btnLogout.FlatAppearance.BorderSize=0;                                                                             // removes the flat-style border

            _pnlSidebar.Controls.Add(_btnLogout);

            // Wire up click events for each sidebar button

            _btnSales.Click     += (_,__)=>{ ShowView(_viewSales);       HighlightNav(_btnSales);     };                        // shows Sales and highlights its button        

            _btnDemograph.Click += (_,__)=>{ ShowView(_viewDemographic); HighlightNav(_btnDemograph); };
            
            _btnTrends.Click    += (_,__)=>{ ShowView(_viewTrends);      HighlightNav(_btnTrends);    };
            
            _btnUnderperf.Click += (_,__)=>{ ShowView(_viewUnderperf);   HighlightNav(_btnUnderperf); };
            
            _btnAddTx.Click     += (_,__)=> OpenAddTx();                                                                        // opens the Add Transaction modal dialog

            _btnExport.Click    += (_,__)=> OpenExport();                                                                       // opens the Export format chooser

            _btnLogout.Click    += (_,__)=> Logout();                                                                           // logs out and closes the dashboard

            // Toggle arrow — sits at the very bottom of the sidebar.

            // Shows ◄ (collapse) when sidebar is open.
            _btnToggle = new Button
            {
                Text      = "◄  Hide Menu",
                
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                
                BackColor = Color.FromArgb(50, 80, 120),                                                                        // slightly lighter than the sidebar for contrast

                ForeColor = Color.White,
                
                FlatStyle = FlatStyle.Flat,
                
                Size      = new Size(SW - 10, 36),
                
                Location  = new Point(5, 510),
                
                Cursor    = Cursors.Hand
            };
            _btnToggle.FlatAppearance.BorderSize = 0;
            
            _btnToggle.Click += ToggleSidebar;                                                                                  // wires the toggle button to the collapse/expand handler

            _pnlSidebar.Controls.Add(_btnToggle);

            Controls.Add(_pnlSidebar);                                                                                          // adds the completed sidebar to the form
        }

        // MAIN PANEL (right of sidebar)
        private void BuildMain()
        {
            // Give _pnlMain a fixed left margin = sidebar width.
            
            // This guarantees content is ALWAYS to the right of the sidebar,
            
            // no matter what draw order Windows uses.
            _pnlMain = new Panel
            {
                BackColor = ColBg,
                
                Location  = new Point(SW, 0),                                                                                   // starts immediately to the right of the sidebar

                Size      = new Size(ClientSize.Width - SW, ClientSize.Height)                                                  // fills the remaining width and full height
            };


            // Keep _pnlMain filling the right area when the window is resized

            Resize += (_,__) =>
            {
                int left = _pnlSidebar.Visible ? SW : 0;                                                                        // if sidebar is hidden, main panel starts at x=0  

                _pnlMain.Location = new Point(left, 0);                                                                         // repositions the main panel       

                _pnlMain.Size     = new Size(ClientSize.Width - left, ClientSize.Height);                                       // resizes it to fill the rest   
            };
            
            // Also update layout immediately when the sidebar is toggled
            
            _pnlSidebar.VisibleChanged += (_,__) =>
            {
                int left = _pnlSidebar.Visible ? SW : 0;
                
                _pnlMain.Location = new Point(left, 0);
                
                _pnlMain.Size     = new Size(ClientSize.Width - left, ClientSize.Height);
            };

            // Title bar at the top of the main content area
            
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
                
                AutoSize=true, Location=new Point(1020,18)                                                                  // positioned near the right edge of the title bar
            };
            _pnlTopBar.Controls.Add(_lblUserInfo);                                                                          // adds the user info label to the title bar

            // Filter bar — 76px tall, two-row (label on top, control below)

            _pnlFilters = new Panel { Dock=DockStyle.Top, Height=76, BackColor=ColFilt };
            
            BuildFilters();                                                                                                 // populates the filter bar with dropdowns and date pickers

            // View host fills all remaining space after the title bar and filter bar

            _pnlViewHost = new Panel { Dock=DockStyle.Fill, BackColor=ColBg };
            
            BuildViewSales();                                                                                               // creates the Sales view panel

            BuildViewDemographic();                                                                                         // creates the Demographics view panel

            BuildViewTrends();                                                                                              // creates the Revenue Trends view panel

            BuildViewUnderperf();                                                                                           // creates the Underperforming view panel

            // Controls added in reverse dock order so DockStyle.Top panels stack correctly

            _pnlMain.Controls.Add(_pnlViewHost);                                                                            // Fill — must be added first so Top panels stack above it

            _pnlMain.Controls.Add(_pnlFilters);                                                                             // Top — stacks above the fill area

            _pnlMain.Controls.Add(_pnlTopBar);                                                                              // Top — stacks above the filter bar

            Controls.Add(_pnlMain);                                                                                         // adds the completed main panel to the form

            ShowView(_viewSales);                                                                                           // shows the Sales view by default on launch

            HighlightNav(_btnSales);                                                                                        // highlights the Sales button in the sidebar
        }

        // Filter bar
        
        // Columns: From | To | Region | Type | Class | Group | [Apply]
        private void BuildFilters()
        {
            // Column start positions (x) and widths for each filter control
            int[] xs   = { 16, 124, 236, 352, 478, 598 };                                                                   // x positions for each column

            int[] ws   = { 96,  96, 104, 114, 108,  86 };                                                                   // widths for each column

            var labels = new[]{"From","To","Region","Type","Class","Group"};                                                // label text for each column

            // Row 1: labels at y=8
            for (int i=0;i<labels.Length;i++)
                _pnlFilters.Controls.Add(new Label
                {
                    Text=labels[i], Font=new Font("Segoe UI",7.5f),
                    
                    ForeColor=Color.FromArgb(148,180,212),                                                                  // muted blue-grey for filter labels                                                                      
                    
                    AutoSize=true, Location=new Point(xs[i],8)
                });

            // Row 2: controls at y=30

            _dtpFrom = new DateTimePicker { Format=DateTimePickerFormat.Short,
                Location=new Point(xs[0],30), Width=ws[0],
                
                Value=DateTime.Today.AddDays(-30) };                                                                        // defaults to 30 days ago

            _dtpTo = new DateTimePicker { Format=DateTimePickerFormat.Short,
                Location=new Point(xs[1],30), Width=ws[1],
                
                Value=DateTime.Today };                                                                                     // defaults to today

            _cmbRegion = FiltCombo(xs[2],ws[2]);
            
            _cmbRegion.Items.Add("All Regions");                                                                            // first option = no region filter
            
            foreach (var r in new[]{"NZ","AU","US","EU","APAC"}) _cmbRegion.Items.Add(r);                                   // adds each region code

            _cmbRegion.SelectedIndex=0;                                                                                     // defaults to "All Regions"

            _cmbItemType = FiltCombo(xs[3],ws[3]);
            
            _cmbItemType.Items.Add("All Types");                                                                            // first option = no type filter

            foreach (ItemType t in Enum.GetValues<ItemType>()) _cmbItemType.Items.Add(t);                                   // adds each ItemType enum value

            _cmbItemType.SelectedIndex=0;                                                                                   // first option = no class filter

            _cmbCharClass = FiltCombo(xs[4],ws[4]);
            
            _cmbCharClass.Items.Add("All Classes");
            
            foreach (CharacterClass c in Enum.GetValues<CharacterClass>()) _cmbCharClass.Items.Add(c);
            
            _cmbCharClass.SelectedIndex=0;

            _cmbGran = FiltCombo(xs[5],ws[5]);
            
            _cmbGran.Items.AddRange(new object[]{"daily","weekly","monthly"});                                              // three granularity options

            _cmbGran.SelectedIndex=0;                                                                                       // defaults to daily grouping

            // Apply button — large orange, vertically centred in the 76px filter bar
            
            _btnRefresh = new Button
            {
                Text="↻  Apply",
                
                Font=new Font("Segoe UI",9.5f,FontStyle.Bold),
                
                BackColor=ColAccent, ForeColor=Color.White,
                
                FlatStyle=FlatStyle.Flat,
                
                Size=new Size(96,42),                                                                                       // tall enough to be easy to click

                Location =new Point(708,17),                                                                                // x=708 places it after all six filter columns

                Cursor =Cursors.Hand
            };
            
            _btnRefresh.FlatAppearance.BorderSize=0;
            
            _btnRefresh.Click+=(_,__)=>RefreshDashboard();                                                                  // regenerates the report with the current filter state

            _pnlFilters.Controls.AddRange(new Control[]
            {
                _dtpFrom,_dtpTo,_cmbRegion,_cmbItemType,_cmbCharClass,_cmbGran,_btnRefresh
            }); // adds all filter controls to the filter bar panel in one call
        }

        // VIEW BUILDERS 

        // Sales Overview — KPI cards + two side-by-side data grids
        
        private void BuildViewSales()
        {
            _viewSales = new Panel
            {
                Dock=DockStyle.Fill, BackColor=ColBg,
                
                Padding=new Padding(14,10,14,10)                                                                            // inner padding so content doesn't touch the panel edges
            };

            // KPI cards row — four side-by-side stat cards at the top
            
            var pCards = new Panel { Dock=DockStyle.Top, Height=98, BackColor=Color.Transparent };
            
            _cardRevenue   = MakeCard("Total Revenue","$0.00",  0);                                                         // revenue card at x=0

            _cardTxCount   = MakeCard("Transactions", "0",      206);                                                       // transaction count card at x=206

            _cardBundlePct = MakeCard("Bundle %",     "0%",     412);                                                       // bundle percentage card at x=412

            _cardTopItem   = MakeCard("Top Item",     "—",      618);                                                       // best-selling item card at x=618

            pCards.Controls.AddRange(new Control[]
                {_cardRevenue,_cardTxCount,_cardBundlePct,_cardTopItem});
            _viewSales.Controls.Add(pCards);                                                                                // adds the card row to the top of the sales view

            // Spacer between stat cards and the grids below (prevents overlap)
            
            _viewSales.Controls.Add(new Panel
                { Dock=DockStyle.Top, Height=16, BackColor=Color.Transparent });

            // Two side-by-side grid panels using Dock Left + Dock Fill.
            
            // Avoids SplitContainer which crashes if the form has no size yet.

            // Left panel — docks left, takes 50% width via Resize event
            
            var pLeft = new Panel { Dock=DockStyle.Left, BackColor=ColBg, Width=500 };
            
            var lblCat = GridTitle("🏆  Top Sellers by Category");                                                          // section heading for the left grid

            lblCat.Dock = DockStyle.Top;                                                                                    // heading docks to the top of the left panel

            lblCat.Height = 24;
            
            _dgvTopCategory = MakeGrid();                                                                                   // creates the top-sellers-by-category grid

            _dgvTopCategory.Dock = DockStyle.Fill;                                                                          // grid fills the rest of the left panel

            pLeft.Controls.Add(_dgvTopCategory);                                                                            // Fill must be added first

            pLeft.Controls.Add(lblCat);                                                                                     // Top stacks above it

            // Right panel — fills the remaining space after the left panel
            
            var pRight = new Panel { Dock=DockStyle.Fill, BackColor=ColBg };
            
            var lblCls = GridTitle("🎮  Top by Character Class");                                                          // section heading for the right grid      

            lblCls.Dock = DockStyle.Top;
            
            lblCls.Height = 24;
            
            _dgvTopClass = MakeGrid();                                                                                     // creates the top-sellers-by-class grid

            _dgvTopClass.Dock = DockStyle.Fill;
            
            pRight.Controls.Add(_dgvTopClass);                                                                             // Fill first

            pRight.Controls.Add(lblCls);                                                                                   // Top stacks above it

            // Outer row panel holds both side by side grids
            
            var pGridRow = new Panel { Dock=DockStyle.Fill, BackColor=ColBg, Padding=new Padding(0,4,0,0) };
            
            pGridRow.Controls.Add(pRight);                                                                                 // Fill : added first so it occupies the right remainder

            pGridRow.Controls.Add(pLeft);                                                                                  // Left : docks to the left edge

            // Keeps the left panel at exactly 50% width when the form is resized
            pGridRow.Resize += (s,e) =>
            {
                if (pGridRow.Width > 20)
                    pLeft.Width = pGridRow.Width / 2 - 5;                                                                  // halves the available width with a small gap
            };

            _viewSales.Controls.Add(pGridRow);                                                                             // adds the two-grid row to the sales view
        }

        // Demographics view — single grid showing revenue by region and spending tier
        
        private void BuildViewDemographic()
        {
            _viewDemographic = new Panel
            {
                Dock=DockStyle.Fill, BackColor=ColBg,
                
                Padding=new Padding(14,10,14,10)
            };
            var pt = new Panel { Dock=DockStyle.Top, Height=26, BackColor=Color.Transparent };
            
            pt.Controls.Add(GridTitle("🌍  Revenue by Region & Spending Tier"));                                         // section heading

            _dgvDemographic =MakeGrid(); _dgvDemographic.Dock=DockStyle.Fill;

            // Fill added first, then Top, so the heading sits above the grid
            
            _viewDemographic.Controls.Add(_dgvDemographic);
            
            _viewDemographic.Controls.Add(pt);
        }

        // Revenue Trends view — single grid showing revenue over time
        
        private void BuildViewTrends()
        {
            _viewTrends = new Panel
            {
                Dock=DockStyle.Fill, BackColor=ColBg,
                
                Padding=new Padding(14,10,14,10)
            };
            
            var pt = new Panel { Dock=DockStyle.Top, Height=26, BackColor=Color.Transparent };
            
            pt.Controls.Add(GridTitle("📈  Revenue Trends — Daily / Weekly / Monthly"));                                 // section heading

            _dgvTrends =MakeGrid(); _dgvTrends.Dock=DockStyle.Fill;
            
            _viewTrends.Controls.Add(_dgvTrends);
            
            _viewTrends.Controls.Add(pt);
        }

        // Underperforming Items view — single grid flagging low-selling items
        
        private void BuildViewUnderperf()
        {
            _viewUnderperf = new Panel
            {
                Dock=DockStyle.Fill, BackColor=ColBg,
                
                Padding=new Padding(14,10,14,10)
            };
            
            var pt = new Panel { Dock=DockStyle.Top, Height=26, BackColor=Color.Transparent };
            
            pt.Controls.Add(GridTitle("⚠   Underperforming MTX Items"));                                                // section heading

            _dgvUnderperf =MakeGrid(); _dgvUnderperf.Dock=DockStyle.Fill;
            
            _viewUnderperf.Controls.Add(_dgvUnderperf);
            
            _viewUnderperf.Controls.Add(pt);
        }

        // DATA REFRESH

        // Reads the filter controls, calls ReportEngine, then populates all views

        private void RefreshDashboard()
        {
            var filters = new FilterSet                                                                                // Build a FilterSet from the current state of the filter bar controls 
            {
                DateRange = new DateRange(_dtpFrom.Value, _dtpTo.Value,
                    _cmbGran.SelectedItem?.ToString() ?? "daily"),
                
                Region         = _cmbRegion.SelectedIndex    > 0 ? _cmbRegion.SelectedItem?.ToString()         : null,
                // index 0 = "All Regions" = no filter (null); any other index = a specific region string


                ItemType = _cmbItemType.SelectedIndex  > 0 ? (ItemType?)_cmbItemType.SelectedItem        : null,
                // index 0 = "All Types" = no filter; cast to nullable enum for the rest

                CharacterClass = _cmbCharClass.SelectedIndex > 0 ? (CharacterClass?)_cmbCharClass.SelectedItem : null
            };

            // Generate the report from the full transaction set using the new filters
            _currentReport = _engine.GenerateReport(filters, _txRepo.GetAll(), _itemRepo.GetAll());

            // Update the username/role label in the title bar
            if (_auth.CurrentUser != null)
                _lblUserInfo.Text = $"👤 {_auth.CurrentUser.Username}  [{_auth.CurrentUser.Role}]";

            // Populate all four views with the new report data
            
            PopulateSales(_currentReport);
            
            PopulateDemographic(_currentReport);
            
            PopulateTrends(_currentReport);
            
            PopulateUnderperf(_currentReport);

            _btnExport.Visible = _auth.CurrentUser?.CanExport() ?? false;
        }

        // Populate: Sales
        private void PopulateSales(Report r)
        {
            SetCard(_cardRevenue,   $"${r.TotalRevenue:N2}");                                                           // updates total revenue card (N2 = thousand-separated, 2 dp)    

            SetCard(_cardTxCount,   $"{r.TotalTransactions:N0}");                                                       // updates transaction count card


            // Calculates bundle percentage: (bundle count / total) * 100, clamped to 0 if no transactions
            
            int pct = r.TotalTransactions > 0
                      ? (int)(100f * r.BundleSplit.BundleCount / r.TotalTransactions) : 0;
            SetCard(_cardBundlePct, $"{pct}%");

            // Finds the single best-selling item across all categories

            var top = r.TopByCategory.Values.OrderByDescending(v=>v.Count)
                                            .Select(v=>v.ItemName).FirstOrDefault() ?? "—";
            SetCard(_cardTopItem, top);

            // BR-01: populate the top sellers by category grid

            _dgvTopCategory.DataSource = r.TopByCategory
                .Select(kv=>new{ Category=kv.Key.ToString(),
                                 TopItem=kv.Value.ItemName,
                                 Sales=kv.Value.Count })
                .OrderByDescending(x=>x.Sales).ToList();                                                                // sorted so highest sellers appear first
            AutoCols(_dgvTopCategory);                                                                                  // applies column width and sort settings

            // BR-02: populate the top sellers by character class grid
            _dgvTopClass.DataSource = r.TopByClass
                .Select(kv=>new{ Class=kv.Key.ToString(),
                                 TopItem=kv.Value.ItemName,
                                 Sales=kv.Value.Count })
                .OrderByDescending(x=>x.Sales).ToList();
            AutoCols(_dgvTopClass);
        }

        // Populate: Demographics
        
        private void PopulateDemographic(Report r)
        {
            _dgvDemographic.DataSource = r.RevenueByRegion
                .Select(kv=>new
                {
                    Region    = kv.Key,                                                                                 // region code (e.g. "NZ")

                    Revenue   = $"${kv.Value:N2}",                                                                      // formatted revenue string

                    Casual    = r.TierDistribution.GetValueOrDefault(SpendingTier.Casual,    0),                        // casual tier transaction count

                    Regular   = r.TierDistribution.GetValueOrDefault(SpendingTier.Regular,   0),                        // regular tier count

                    HighValue = r.TierDistribution.GetValueOrDefault(SpendingTier.HighValue, 0)                         // high-value tier count
                })
                .OrderByDescending(x=>x.Revenue).ToList();                                                              // sorted by revenue descending
            AutoCols(_dgvDemographic);
        }

        // Populate: Trends
        
        // Exactly two columns: Period and Revenue (no extra props = no extra columns)
        private void PopulateTrends(Report r)
        {
            _dgvTrends.DataSource = r.RevenueTrend
                .Select(kv=>new{ Period=kv.Key, Revenue=$"${kv.Value:N2}" })                                            // projects to Period + Revenue only
                .OrderBy(x=>x.Period).ToList();                                                                         // sorted chronologically by the date key string
            AutoCols(_dgvTrends);
        }

        // Populate: Underperforming
        private void PopulateUnderperf(Report r)
        {
            _dgvUnderperf.DataSource = r.UnderperformingItems
                .Select(u=>new{ Item=u.ItemName, Category=u.Type.ToString(),
                                Sales=u.Sales,
                                Status=u.Sales==0?"🔴 No Sales":"🟡 Low Sales" })                                      // colour-coded status based on zero vs low   
                .ToList();
            AutoCols(_dgvUnderperf);

            // Colour zero sale rows red to make them immediately visible
            
            foreach (DataGridViewRow row in _dgvUnderperf.Rows)
                if (row.Cells["Sales"].Value is int s && s==0)
                    row.DefaultCellStyle.ForeColor=Color.FromArgb(248,113,113);
        }

        // ACTIONS 

        // Opens the Add Transaction modal dialog (FR14 role check)

        private void OpenAddTx()
        {
            if (_auth.CurrentUser?.CanConfigure()==false)                                                               // checks role before opening
            {
                MessageBox.Show("Your role does not permit recording purchases.",
                    "Access Denied",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;                                                                                                 // exits without opening the dialog
            }
            using var dlg=new AddTransactionForm(_txService,_itemRepo,_players);
            if (dlg.ShowDialog(this)==DialogResult.OK)                                                                  // opens the dialog modally; waits for it to close
            {
                RefreshDashboard();                                                                                     // regenerates the report to include the new transaction
                MessageBox.Show($"Purchase recorded:\n{dlg.RecordedTransaction}",
                    "Success",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        // Opens the export format chooser then the save file dialog

        private void OpenExport()
        {
            if (_currentReport==null){RefreshDashboard();return; }                                                       // ensures a report exists before exporting

            var fmt=ChooseFormat();                                                                                      // shows the format picker dialog; returns null if user cancels   

            if (fmt==null) return;                                                                                       // user cancelled — do nothing

            using var dlg=new SaveFileDialog
            {
                Title="Export Report",
                
                Filter=fmt switch                                                                                        // sets the file filter based on the chosen format
                {
                    ExportFormat.CSV  =>"CSV Files|*.csv",
                    ExportFormat.JSON =>"JSON Files|*.json",
                    _                 => "Text Files|*.txt"                                                              // PDF exports as a .txt file
                },
                FileName=$"GGG_Report_{DateTime.Now:yyyyMMdd_HHmm}"                                                      // default filename with timestamp
            };
            if (dlg.ShowDialog(this)==DialogResult.OK)                                                                   // shows the Save dialog and waits   
            {
                try
                {
                    _engine.ExportReport(_currentReport,fmt.Value,dlg.FileName);                                         // writes the file via ReportEngine → ExportService
                    MessageBox.Show($"Exported:\n{dlg.FileName}","Done",
                        MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}","Error",
                        MessageBoxButtons.OK,MessageBoxIcon.Error);                                                     // shows the error message if writing fails
                }
            }
        }

        // Shows a small dialog with buttons for each ExportFormat and returns the chosen one

        private ExportFormat? ChooseFormat()
        {
            using var dlg=new Form
            {
                Text="Export Format",Size=new Size(260,186),
                
                StartPosition=FormStartPosition.CenterParent,
                
                FormBorderStyle=FormBorderStyle.FixedDialog,
                
                MaximizeBox=false,BackColor=ColBg
            };
            ExportFormat? chosen=null;                                                                                  // will be set when the user clicks a format button
            int y=18;
            foreach (ExportFormat f in Enum.GetValues<ExportFormat>())                                                  // iterates CSV, PDF, JSON
            {
                var fmt=f;                                                                                              // captures the loop variable for use in the lambda (closure)
                var b=new Button
                {
                    Text=fmt.ToString(),                                                                                // button label is the enum name (e.g. "CSV")

                    Font =new Font("Segoe UI",10,FontStyle.Bold),
                    
                    BackColor=ColHeader,ForeColor=Color.White,
                    
                    FlatStyle=FlatStyle.Flat,
                    
                    Size=new Size(210,34),Location=new Point(22,y),Cursor=Cursors.Hand
                };
                
                b.FlatAppearance.BorderSize=0;
                
                b.Click+=(_,__)=>{chosen=fmt;dlg.Close();};                                                              // stores the chosen format and closes the dialog

                dlg.Controls.Add(b);
                
                y+=40;                                                                                                  // advances vertical position for the next button
            }
            
            dlg.ShowDialog(this);                                                                                       // shows the dialog modally; blocks until closed

            return chosen;                                                                                              // null if user closed without clicking (cancellation)            
        }

        // Logs out the current user and closes the dashboard (returns to the login loop in Program.cs)

        private void Logout(){_auth.Logout();Close();}

        // SIDEBAR TOGGLE

        // Expand button shown on the form when sidebar is collapsed.
        
        // Stays pinned to left edge so user can always re-open the menu.
        private Button? _btnExpand;

        // Collapses or expands the sidebar.
        private void ToggleSidebar(object? sender, EventArgs e)
        {
            _sidebarVisible = !_sidebarVisible;                                                                         // flips the visibility flag
            
            _pnlSidebar.Visible = _sidebarVisible;                                                                      // shows or hides the sidebar panel
            
            // Force the main panel to reposition immediately after toggle
            
            OnResize(EventArgs.Empty);                                                                                  // forces the main panel to reposition immediately

            if (!_sidebarVisible)
            {
                // Sidebar is now hidden — create a small expand button on the form edge
                
                _btnExpand = new Button
                {
                    Text      = "►",                                                                                    // right-pointing arrow = click to re-open the sidebar

                    Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                    
                    BackColor = Color.FromArgb(30, 58, 95),
                    
                    ForeColor = Color.White,
                    
                    FlatStyle = FlatStyle.Flat,
                    
                    Size      = new Size(28, 60),
                    
                    Location  = new Point(0, 120),                                                                     // pinned to x=0 (left edge of the form)

                    Cursor    = Cursors.Hand
                };
                _btnExpand.FlatAppearance.BorderSize = 0;
                
                _btnExpand.Click += ToggleSidebar;                                                                     // same handler re-opens the sidebar when clicked

                Controls.Add(_btnExpand);
                
                _btnExpand.BringToFront();                                                                             // ensures the expand button is drawn above other controls 
            }
            else
            {
                // Sidebar is back — remove the floating expand button so it doesn't overlap content
                
                if (_btnExpand != null)
                {
                    Controls.Remove(_btnExpand);                                                                      // removes from the form's control collection  

                    _btnExpand.Dispose();                                                                             // releases the control's resources 

                    _btnExpand = null;                                                                                // clears the reference so it can be garbage collected  
                }
            }
        }

        // NAVIGATION

        // Swaps the active view in the view host panel

        private void ShowView(Panel target)
        {
            foreach(var v in new[]{_viewSales,_viewDemographic,_viewTrends,_viewUnderperf})

                _pnlViewHost.Controls.Remove(v);                                                                      // removes all view panels from the host (only one can be visible)                                                               
            
            _pnlViewHost.Controls.Add(target);                                                                        // adds only the requested view panel   

            target.BringToFront();                                                                                    // ensures it is drawn on top  
        }

        // Highlights the active sidebar button and dims the others

        private void HighlightNav(Button active)
        {
            foreach(var b in new[]{_btnSales,_btnDemograph,_btnTrends,_btnUnderperf})
            {
                b.BackColor = b==active ? ColHeader : Color.Transparent;                                              // active = blue background; inactive = transparent  

                b.ForeColor = b==active ? Color.White : ColMuted;                                                     // active = white text; inactive = muted grey          
            }
        }

        // FACTORIES

        // Creates a small grey section heading label inside the sidebar
        
        private static Label SideSection(string t, ref int y)
        {
            var l=new Label{Text=t,Font=new Font("Segoe UI",7.5f,FontStyle.Bold),
            
                ForeColor=Color.FromArgb(71,85,105),AutoSize=true,Location=new Point(14,y)};                        // very muted grey for section labels
            
            y+=22; return l;                                                                                        // advances y by 22px and returns the label
        }

        // Creates a sidebar navigation button and advances the y position
        
        private static Button SideBtn(string t, ref int y)
        {
            var b=new Button
            {
                Text=t,Font=new Font("Segoe UI",9.5f),
                
                ForeColor=ColMuted,BackColor=Color.Transparent,
                
                FlatStyle=FlatStyle.Flat,Size=new Size(SW-10,38),
                
                Location=new Point(5,y),
                
                TextAlign=ContentAlignment.MiddleLeft,                                                              // left-aligns the icon + text within the button

                Padding =new Padding(8,0,0,0),Cursor=Cursors.Hand
            };
            
            b.FlatAppearance.BorderSize=0; y+=40; return b;                                                         // advances y by 40px and returns the button
        }

        // Creates a styled combo box for the filter bar

        // Filter-bar combo box

        private static ComboBox FiltCombo(int x, int w)=>new ComboBox
        {
            Font=new Font("Segoe UI",8.5f),
            
            BackColor=Color.FromArgb(30,41,59),ForeColor=Color.White,
            
            DropDownStyle=ComboBoxStyle.DropDownList,                                                              // prevents free-text entry     

            Size =new Size(w,26),Location=new Point(x,30)                                                          // y=30 aligns with all other row-2 controls  
        };

        // Creates a KPI stat card panel at position (x, 2) in the cards row

        // KPI stat card panel
        private static Panel MakeCard(string title,string val,int x)
        {
            var p=new Panel{Size=new Size(196,88),Location=new Point(x,2),
                            BackColor=Color.FromArgb(22,32,50)};                                                    // dark card background

            p.Controls.Add(new Label{Text=title,Font=new Font("Segoe UI",8),
                ForeColor=Color.FromArgb(148,180,212),AutoSize=true,Location=new Point(12,10)});                    // muted blue-grey for the card title

            p.Controls.Add(new Label{Text=val,Font=new Font("Segoe UI",17,FontStyle.Bold),
                ForeColor=Color.FromArgb(251,146,60),AutoSize=true,                                                 // GGG orange for the large value    
                Location =new Point(12,32),Tag="v"});                                                               // Tag="v" lets SetCard() identify this label
            return p;
        }

        // Updates the orange value label inside a stat card
        
        private static void SetCard(Panel card,string val)
        {
            foreach(Control c in card.Controls)
                if(c is Label l&&(string?)l.Tag=="v") l.Text=val;                                                   // finds the label tagged "v" and updates its text
        }

        // Creates a dark-themed, read-only DataGridView with GGG styling
        
        private static DataGridView MakeGrid()
        {
            var g=new DataGridView
            {
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,                                       // columns share available width evenly

                RowHeadersVisible     = false,                                                                      // hides the row number column on the left

                BackgroundColor       = Color.FromArgb(22,32,50),                                                   // removes the default outer border     

                BorderStyle           = BorderStyle.None,                                                           // slightly taller headers for readability

                ColumnHeadersHeight   = 34,                                                                         // standard row height

                RowTemplate           = {Height=28},                                                                // prevents users from editing data

                ReadOnly              = true,                                                                       // hides the empty "new row" at the bottom

                AllowUserToAddRows    = false,
                
                AllowUserToDeleteRows = false,
                
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,                                    // clicking anywhere selects the whole row

                MultiSelect           = false,                                                                      // only one row can be selected at a time

                Font                  = new Font("Segoe UI",9),
                
                ForeColor             = Color.FromArgb(226,232,240),                                                // light text for dark background

                GridColor             = Color.FromArgb(40,55,75)                                                    // subtle grid lines
            };

            // Column header styling

            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30,58,95);                                   // deep blue header background

            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            
            g.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI",9,FontStyle.Bold);
            
            g.ColumnHeadersDefaultCellStyle.Padding   = new Padding(6,0,0,0);                                       // left-padding inside each header cell

            g.EnableHeadersVisualStyles               = false;                                                      // required to apply custom header colours

            // Default cell styling

            g.DefaultCellStyle.BackColor          = Color.FromArgb(22,32,50);                                       // dark cell background    

            g.DefaultCellStyle.ForeColor          = Color.FromArgb(226,232,240);
            
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30,58,95);                                       // blue highlight for selected row

            g.DefaultCellStyle.SelectionForeColor = Color.White;
            
            g.DefaultCellStyle.Padding            = new Padding(6,0,0,0);

            // Alternating row colour for readability

            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26,38,60);
            
            return g;
        }

        // Sets all grid columns to Fill mode with a minimum readable width
        
        private static void AutoCols(DataGridView g)
        {
            g.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;                                            // columns fill available width proportionally
            
            foreach(DataGridViewColumn c in g.Columns)
            
            { c.SortMode=DataGridViewColumnSortMode.Automatic; c.MinimumWidth=60; }
        } // Automatic sort mode enables click-to-sort on all columns; MinimumWidth=60 prevents columns becoming unreadable

        // Section heading above a grid

        // Creates a bold section heading label displayed above a data grid

        private static Label GridTitle(string t)=>new Label
        {
            Text=t,Font=new Font("Segoe UI",9.5f,FontStyle.Bold),
            
            ForeColor=Color.FromArgb(148,180,212),AutoSize=true,Location=new Point(0,4)                             // muted blue-grey to keep headings subtle
        };
    }
}

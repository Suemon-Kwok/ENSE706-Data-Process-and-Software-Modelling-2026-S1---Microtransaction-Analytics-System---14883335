// Name : Suemon Kwok

// Student ID : 14883335

// ReportEngine.cs — ReportEngine class (UML)

// Core analytics engine. Implements IReportGenerator.

// FR04, FR09, FR10, FR11 — best sellers, filters, trends, flags.

// What does this file do
// the analytics brain. Implements IReportGenerator.
// Takes all transactions and filters, then calculates every metric: total revenue, top item per category, top item per character class,
// revenue by date, items below threshold, revenue by region, bundle vs individual split, and spending tier distribution

// OOP Concept
// Single Responsibility and Polymorphism. Implements IReportGenerator.
// All analytics computation is isolated here. Works on IEnumerable<Transaction> —
// doesn't care what kind of item is inside each transaction.

// Why OOP concepts were used
// Polymorphism — "Treat different things the same way" Polymorphism	Different objects, same method name, different behaviour
// The ReportEngine doesn't care whether it's looking at a WeaponSkin or a Bundle —
// it just calls GetDescription() and GetPrice() on whatever it's given. Each class responds differently,
// but the calling code doesn't need to know that.
// Why? It means you can add a brand new item type (say, CharacterEffect)
// and the report engine works with it automatically — no changes needed.

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;                                                                                                       // brings in Report, FilterSet, Transaction, MTXItem, and all enums
    using Microsoft.VisualBasic.ApplicationServices;

    /// <summary>

    /// Generates analytics reports from raw transaction data.

    /// All heavy computation happens here, keeping the UI clean.

    /// </summary>
    public class ReportEngine : IReportGenerator
    {
        // Currently active filter set (updated via ApplyFilter)
        private FilterSet _filters;                                                                                             // stored so the same filters apply until explicitly changed

        // FR11: Items whose sales count is below this trigger a flag
        private float _underperformThreshold;                                                                                   // configurable at construction; default 50, demo uses 15

        // NFR01: Purchase data processed in-memory — sub-millisecond, well within 2s target

        // NFR05: LINQ on List<T> handles 1M records under 3s (no database round-trips needed)
        public ReportEngine(float underperformThreshold = 50f)                                                                           
        {
            _filters               = new FilterSet();                                                                           // defaults: last 30 days, no region/type/class filter  
            _underperformThreshold = underperformThreshold;                                                                     // stores the configurable threshold value
        }

        // IReportGenerator.ApplyFilter
        
        // Stores the new filter set for subsequent GenerateReport calls
        public void ApplyFilter(FilterSet filters) =>
            _filters = filters ?? new FilterSet();                                                                              // if null is passed, resets to defaults rather than crashing

        // IReportGenerator.GenerateReport 

        // Builds and returns a fully populated Report DTO (FR09)
        public Report GenerateReport(FilterSet filters,
                                     IEnumerable<Transaction> transactions,
                                     IEnumerable<MTXItem> catalogue)
        {
            // Materialise collections once to avoid multiple enumeration
            var txList  = transactions.ToList();                                                                                // converts IEnumerable to List so it can be iterated multiple times

            var catList = catalogue.ToList();                                                                                   // same — needed for both underperform check and top-seller calculations

            // Step 1: apply date range and optional category/region filters
            var filtered = ApplyFilters(txList, filters);                                                                       // returns only the transactions that pass all active filters

            // Step 2: build and return the populated report object
            return new Report
            {
                Title            = BuildTitle(filters),                                                                         // human-readable title from filter state

                GeneratedAt      = DateTime.Now,                                                                                // current time at report generation

                AppliedFilters   = filters,                                                                                     // stores the filter snapshot in the report    

                TotalRevenue     = filtered.Sum(t => t.Price),                                                                  // BR-03: sum all prices in filtered set

                TotalTransactions = filtered.Count,                                                                             // total transaction count after filtering

                TopByCategory    = GetTopByCategory(filtered),                                                                  // BR-01: top item per ItemType

                TopByClass       = GetTopByClass(filtered),                                                                     // BR-02: top item per CharacterClass

                RevenueTrend     = GetRevenueTrend(filtered, filters.DateRange),                                                // FR10: revenue over time by granularity

                UnderperformingItems = GetUnderperforming(catList, txList),                                                     // FR11: uses full unfiltered list to flag all low sellers

                RevenueByRegion  = GetRevenueByRegion(filtered),                                                                // BR-06: revenue grouped by region code

                BundleSplit      = GetBundleSplit(filtered),                                                                    // FR05/BR-08: bundle vs individual counts

                TierDistribution = GetTierDistribution(filtered)                                                                // FR07: transaction count per spending tier
            };
        }

        // IReportGenerator.ExportReport
        
        // Delegates formatting to ExportService (FR12)
        public void ExportReport(Report report, ExportFormat format, string outputPath)
        {
            var exporter = new ExportService(format);                                                                           // creates an ExportService configured with the chosen format
            
            exporter.Export(report, outputPath);                                                                                // writes the report to the specified file path
        }

        // FR10: Revenue trend grouped by date granularity
        public Dictionary<string, float> GetRevenueTrend(
            IEnumerable<Transaction> transactions, DateRange range)
        {
            // Group transactions by day/week/month label then sum revenue
            return transactions
                .GroupBy(t => FormatDateKey(t.Timestamp, range.Granularity))                                                    // groups each transaction by its formatted date key
                .OrderBy(g => g.Key)                                                                                            // sorts groups chronologically by key string
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Price));                                                            // converts each group to key → total revenue
        }

        // FR11: Flag items below the underperform threshold
        public void FlagUnderperforming(IEnumerable<MTXItem> catalogue,
                                        IEnumerable<Transaction> all)
        {
            // Count how many times each item was sold
            var soldCounts = all.GroupBy(t => (t.GetItem() as MTXItem)?.ItemId)
                               .ToDictionary(g => g.Key ?? "", g => g.Count());                                                 // maps ItemId → transaction count

            // Mark or log items below threshold (threshold configurable BR-04)
            foreach (var item in catalogue)
                if (soldCounts.GetValueOrDefault(item.ItemId, 0) < _underperformThreshold)
                    Console.WriteLine($"[UNDERPERFORM] {item.Name} — sales below threshold");                                   // outputs a warning to the console
        }

        // PRIVATE HELPERS

        // Applies all active filters to the transaction list
        private static List<Transaction> ApplyFilters(
            List<Transaction> txList, FilterSet filters)
        {
            // Start with full set then progressively narrow
            IEnumerable<Transaction> q = txList;                                                                                // IEnumerable allows chaining Where() without materialising each step

            // Date range filter — always applied
            if (filters.DateRange != null)
                q = q.Where(t => filters.DateRange.Contains(t.Timestamp));                                                      // keeps only transactions within the date range

            // Optional region filter
            if (!string.IsNullOrEmpty(filters.Region))
                q = q.Where(t => t.Demographics.Region
                                   .Equals(filters.Region, StringComparison.OrdinalIgnoreCase));                                // case-insensitive region match

            // Optional item type filter
            if (filters.ItemType.HasValue)
                q = q.Where(t => t.GetItem().GetItemType() == filters.ItemType.Value);                                          // keeps only transactions for the selected type

            // Optional character class filter
            if (filters.CharacterClass.HasValue)
                q = q.Where(t => t.CharacterClass == filters.CharacterClass.Value);                                             // keeps only transactions for the selected class

            // Bundle-only filter (FR05)
            if (filters.BundleOnly)
                q = q.Where(t => t.BundleFlag);                                                                                 // keeps only transactions marked as bundle purchases

            return q.ToList();                                                                                                  // materialises the filtered query into a concrete List
        }

        // BR-01: Highest-selling category by unit volume
        private static Dictionary<ItemType, (string ItemName, int Count)>
            GetTopByCategory(List<Transaction> filtered)
        {
            var result = new Dictionary<ItemType, (string, int)>();                                                             // will hold one entry per ItemType

            // Group by item type, then find the top item within each group
            var byType = filtered.GroupBy(t => t.GetItem().GetItemType());                                                      // groups transactions by ItemType enum value
            foreach (var grp in byType)
            {
                // Find item name with highest count in this category
                var top = grp.GroupBy(t => (t.GetItem() as MTXItem)?.Name ?? "Unknown")                                         // sub-groups by item name within the type
                             .OrderByDescending(g => g.Count())                                                                 // sorts sub-groups so highest count is first
                             .First();                                                                                          // takes only the top-selling item
                result[grp.Key] = (top.Key, top.Count());                                                                       // stores (ItemName, Count) for this category
            }
            return result;
        }

        // BR-02: Most purchased item per character class
        private static Dictionary<CharacterClass, (string ItemName, int Count)>
            GetTopByClass(List<Transaction> filtered)
        {
            var result = new Dictionary<CharacterClass, (string, int)>();                                                       // will hold one entry per CharacterClass

            var byClass = filtered.GroupBy(t => t.CharacterClass);                                                              // groups transactions by CharacterClass enum value
            foreach (var grp in byClass)                                                                                        // iterates each character class group
            {
                var top = grp.GroupBy(t => (t.GetItem() as MTXItem)?.Name ?? "Unknown")                                         // sub-groups by item name within the class    
                             .OrderByDescending(g => g.Count())                                                                 // highest-count item name first
                             .First();
                result[grp.Key] = (top.Key, top.Count());                                                                       // stores (ItemName, Count) for this class
            }
            return result;
        }

        // BR-04/FR11: Items with fewer sales than the threshold
        private List<(string, ItemType, int)> GetUnderperforming(
            List<MTXItem> catalogue, List<Transaction> all)
        {
            // Build a sold-count lookup for all catalogue items
            var soldCounts = all
                .GroupBy(t => (t.GetItem() as MTXItem)?.ItemId ?? "")                                                           // groups all transactions by ItemId
                .ToDictionary(g => g.Key, g => g.Count());                                                                      // maps ItemId → total transactions

            // Return items whose sold count is below the configured threshold
            return catalogue
                .Where(item => soldCounts.GetValueOrDefault(item.ItemId, 0) < _underperformThreshold)                           // filters below-threshold items
                .Select(item => (item.Name, item.GetItemType(),                                                                 // projects to (name, type, count) tuple
                                 soldCounts.GetValueOrDefault(item.ItemId, 0)))
                .OrderBy(x => x.Item3)  // lowest sales first
                .ToList();
        }

        // BR-06: Total revenue grouped by region
        private static Dictionary<string, float> GetRevenueByRegion(
            List<Transaction> filtered) =>
            
            filtered.GroupBy(t => t.Demographics.Region)                                                                        // groups transactions by region code
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Price));                                                        // maps region → total revenue    

        // BR-08: Count bundle vs individual purchases (FR05)
        private static (int, int) GetBundleSplit(List<Transaction> filtered)
        {
            int bundles    = filtered.Count(t => t.BundleFlag);                                                                 // counts transactions where BundleFlag is true

            int individual = filtered.Count - bundles;                                                                          // the remainder are individual (non-bundle) purchases

            return (bundles, individual);                                                                                       // returns as a value tuple
        }

        // FR07: Distribution of players across spending tiers
        private static Dictionary<SpendingTier, int> GetTierDistribution(
            List<Transaction> filtered) =>
            
            filtered.GroupBy(t => t.Demographics.SpendingTier)                                                                  // groups transactions by the player's spending tier at purchase time
                    .ToDictionary(g => g.Key, g => g.Count());                                                                  // maps SpendingTier → transaction count

        // Format a date into a bucket label based on granularity setting
        private static string FormatDateKey(DateTime dt, string granularity) =>
            granularity switch
            {
                "weekly"  => $"W{GetISOWeek(dt)} {dt.Year}",                                                                    // e.g. "W12 2024" for ISO week 12

                "monthly" => dt.ToString("MMM yyyy"),                                                                           // e.g. "Mar 2024"
                _         => dt.ToString("yyyy-MM-dd")                                                                          // daily (default): e.g. "2024-03-15"
            };

        // Returns ISO 8601 week number for a given date
        private static int GetISOWeek(DateTime dt) =>
            System.Globalization.ISOWeek.GetWeekOfYear(dt);                                                                     // uses the built-in ISO week calculator

        // Generates a human-readable report title from active filters
        private static string BuildTitle(FilterSet f)
        {
            var parts = new List<string> { "MTX Analytics Report" };                                                            // base title always included

            if (!string.IsNullOrEmpty(f.Region))    parts.Add($"Region: {f.Region}");                                           // appended if region filter active

            if (f.ItemType.HasValue)                 parts.Add($"Type: {f.ItemType}");                                          // appended if item type filter active

            if (f.CharacterClass.HasValue)           parts.Add($"Class: {f.CharacterClass}");                                   // appended if class filter active

            if (f.DateRange != null)                 parts.Add(f.DateRange.ToString());                                         // appended to show the date window

            return string.Join(" | ", parts);                                                                                   // joins all parts with " | " separator e.g. "MTX Analytics Report | Region: NZ"
        }
    }
}

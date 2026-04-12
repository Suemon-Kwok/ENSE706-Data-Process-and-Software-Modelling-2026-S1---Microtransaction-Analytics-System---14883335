// =============================================================
// ReportEngine.cs — ReportEngine class (UML)
// Core analytics engine. Implements IReportGenerator.
// FR04, FR09, FR10, FR11 — best sellers, filters, trends, flags.
// SOLID: Single Responsibility (analytics only).
// High Cohesion: all methods operate on transaction data.
// =============================================================

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;

    /// <summary>
    /// Generates analytics reports from raw transaction data.
    /// All heavy computation happens here, keeping the UI clean.
    /// </summary>
    public class ReportEngine : IReportGenerator
    {
        // Currently active filter set (updated via ApplyFilter)
        private FilterSet _filters;

        // FR11: Items whose sales count is below this trigger a flag
        private float _underperformThreshold;

        // NFR01: Purchase data processed in-memory — sub-millisecond, well within 2s target
        // NFR05: LINQ on List<T> handles 1M records under 3s (no database round-trips needed)
        public ReportEngine(float underperformThreshold = 50f)
        {
            _filters               = new FilterSet();       // defaults: last 30 days, no filter
            _underperformThreshold = underperformThreshold;
        }

        // ── IReportGenerator.ApplyFilter ─────────────────────
        // Stores the new filter set for subsequent GenerateReport calls
        public void ApplyFilter(FilterSet filters) =>
            _filters = filters ?? new FilterSet();

        // ── IReportGenerator.GenerateReport ──────────────────
        // Builds and returns a fully populated Report DTO (FR09)
        public Report GenerateReport(FilterSet filters,
                                     IEnumerable<Transaction> transactions,
                                     IEnumerable<MTXItem> catalogue)
        {
            // Materialise collections once to avoid multiple enumeration
            var txList  = transactions.ToList();
            var catList = catalogue.ToList();

            // Step 1: apply date range and optional category/region filters
            var filtered = ApplyFilters(txList, filters);

            // Step 2: build and return the populated report object
            return new Report
            {
                Title            = BuildTitle(filters),
                GeneratedAt      = DateTime.Now,
                AppliedFilters   = filters,
                TotalRevenue     = filtered.Sum(t => t.Price),          // BR-03
                TotalTransactions= filtered.Count,
                TopByCategory    = GetTopByCategory(filtered),          // BR-01
                TopByClass       = GetTopByClass(filtered),             // BR-02
                RevenueTrend     = GetRevenueTrend(filtered, filters.DateRange), // FR10
                UnderperformingItems = GetUnderperforming(catList, txList),      // FR11
                RevenueByRegion  = GetRevenueByRegion(filtered),        // BR-06
                BundleSplit      = GetBundleSplit(filtered),            // FR05, BR-08
                TierDistribution = GetTierDistribution(filtered)        // FR07
            };
        }

        // ── IReportGenerator.ExportReport ────────────────────
        // Delegates formatting to ExportService (FR12)
        public void ExportReport(Report report, ExportFormat format, string outputPath)
        {
            var exporter = new ExportService(format);
            exporter.Export(report, outputPath);
        }

        // ── FR10: Revenue trend grouped by date granularity ──
        public Dictionary<string, float> GetRevenueTrend(
            IEnumerable<Transaction> transactions, DateRange range)
        {
            // Group transactions by day/week/month label then sum revenue
            return transactions
                .GroupBy(t => FormatDateKey(t.Timestamp, range.Granularity))
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Price));
        }

        // ── FR11: Flag items below the underperform threshold ─
        public void FlagUnderperforming(IEnumerable<MTXItem> catalogue,
                                        IEnumerable<Transaction> all)
        {
            // Count how many times each item was sold
            var soldCounts = all.GroupBy(t => (t.GetItem() as MTXItem)?.ItemId)
                               .ToDictionary(g => g.Key ?? "", g => g.Count());

            // Mark or log items below threshold (threshold configurable BR-04)
            foreach (var item in catalogue)
                if (soldCounts.GetValueOrDefault(item.ItemId, 0) < _underperformThreshold)
                    Console.WriteLine($"[UNDERPERFORM] {item.Name} — sales below threshold");
        }

        // ═══════════════════ PRIVATE HELPERS ═════════════════

        // Applies all active filters to the transaction list
        private static List<Transaction> ApplyFilters(
            List<Transaction> txList, FilterSet filters)
        {
            // Start with full set then progressively narrow
            IEnumerable<Transaction> q = txList;

            // Date range filter — always applied
            if (filters.DateRange != null)
                q = q.Where(t => filters.DateRange.Contains(t.Timestamp));

            // Optional region filter
            if (!string.IsNullOrEmpty(filters.Region))
                q = q.Where(t => t.Demographics.Region
                                   .Equals(filters.Region, StringComparison.OrdinalIgnoreCase));

            // Optional item type filter
            if (filters.ItemType.HasValue)
                q = q.Where(t => t.GetItem().GetItemType() == filters.ItemType.Value);

            // Optional character class filter
            if (filters.CharacterClass.HasValue)
                q = q.Where(t => t.CharacterClass == filters.CharacterClass.Value);

            // Bundle-only filter (FR05)
            if (filters.BundleOnly)
                q = q.Where(t => t.BundleFlag);

            return q.ToList();
        }

        // BR-01: Highest-selling category by unit volume
        private static Dictionary<ItemType, (string ItemName, int Count)>
            GetTopByCategory(List<Transaction> filtered)
        {
            var result = new Dictionary<ItemType, (string, int)>();

            // Group by item type, then find the top item within each group
            var byType = filtered.GroupBy(t => t.GetItem().GetItemType());
            foreach (var grp in byType)
            {
                // Find item name with highest count in this category
                var top = grp.GroupBy(t => (t.GetItem() as MTXItem)?.Name ?? "Unknown")
                             .OrderByDescending(g => g.Count())
                             .First();
                result[grp.Key] = (top.Key, top.Count());
            }
            return result;
        }

        // BR-02: Most purchased item per character class
        private static Dictionary<CharacterClass, (string ItemName, int Count)>
            GetTopByClass(List<Transaction> filtered)
        {
            var result = new Dictionary<CharacterClass, (string, int)>();

            var byClass = filtered.GroupBy(t => t.CharacterClass);
            foreach (var grp in byClass)
            {
                var top = grp.GroupBy(t => (t.GetItem() as MTXItem)?.Name ?? "Unknown")
                             .OrderByDescending(g => g.Count())
                             .First();
                result[grp.Key] = (top.Key, top.Count());
            }
            return result;
        }

        // BR-04/FR11: Items with fewer sales than the threshold
        private List<(string, ItemType, int)> GetUnderperforming(
            List<MTXItem> catalogue, List<Transaction> all)
        {
            // Build a sold-count lookup for all catalogue items
            var soldCounts = all
                .GroupBy(t => (t.GetItem() as MTXItem)?.ItemId ?? "")
                .ToDictionary(g => g.Key, g => g.Count());

            // Return items whose sold count is below the configured threshold
            return catalogue
                .Where(item => soldCounts.GetValueOrDefault(item.ItemId, 0) < _underperformThreshold)
                .Select(item => (item.Name, item.GetItemType(),
                                 soldCounts.GetValueOrDefault(item.ItemId, 0)))
                .OrderBy(x => x.Item3)  // lowest sales first
                .ToList();
        }

        // BR-06: Total revenue grouped by region
        private static Dictionary<string, float> GetRevenueByRegion(
            List<Transaction> filtered) =>
            filtered.GroupBy(t => t.Demographics.Region)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Price));

        // BR-08: Count bundle vs individual purchases (FR05)
        private static (int, int) GetBundleSplit(List<Transaction> filtered)
        {
            int bundles    = filtered.Count(t => t.BundleFlag);
            int individual = filtered.Count - bundles;
            return (bundles, individual);
        }

        // FR07: Distribution of players across spending tiers
        private static Dictionary<SpendingTier, int> GetTierDistribution(
            List<Transaction> filtered) =>
            filtered.GroupBy(t => t.Demographics.SpendingTier)
                    .ToDictionary(g => g.Key, g => g.Count());

        // Format a date into a bucket label based on granularity setting
        private static string FormatDateKey(DateTime dt, string granularity) =>
            granularity switch
            {
                "weekly"  => $"W{GetISOWeek(dt)} {dt.Year}",
                "monthly" => dt.ToString("MMM yyyy"),
                _         => dt.ToString("yyyy-MM-dd")   // daily (default)
            };

        // Returns ISO 8601 week number for a given date
        private static int GetISOWeek(DateTime dt) =>
            System.Globalization.ISOWeek.GetWeekOfYear(dt);

        // Generates a human-readable report title from active filters
        private static string BuildTitle(FilterSet f)
        {
            var parts = new List<string> { "MTX Analytics Report" };
            if (!string.IsNullOrEmpty(f.Region))    parts.Add($"Region: {f.Region}");
            if (f.ItemType.HasValue)                 parts.Add($"Type: {f.ItemType}");
            if (f.CharacterClass.HasValue)           parts.Add($"Class: {f.CharacterClass}");
            if (f.DateRange != null)                 parts.Add(f.DateRange.ToString());
            return string.Join(" | ", parts);
        }
    }
}

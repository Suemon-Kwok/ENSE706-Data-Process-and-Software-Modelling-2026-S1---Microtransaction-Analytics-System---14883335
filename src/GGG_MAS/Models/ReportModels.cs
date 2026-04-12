// =============================================================
// ReportModels.cs — FilterSet, DateRange, Report value objects
// Used by ReportEngine to filter and return analytics data.
// FR09, FR10, FR11 — filterable, trend, underperforming.
// =============================================================

namespace GGG_MAS.Models
{
    /// <summary>
    /// Value object holding all active report filter criteria.
    /// Passed to ReportEngine.ApplyFilter() to narrow results.
    /// </summary>
    public class FilterSet
    {
        // Optional region filter — null means "all regions"
        public string? Region { get; set; }

        // Optional item type filter — null means "all types"
        public ItemType? ItemType { get; set; }

        // Optional character class filter — null means "all classes"
        public CharacterClass? CharacterClass { get; set; }

        // Date range constraint — required; defaults to last 30 days
        public DateRange DateRange { get; set; } =
            new DateRange(DateTime.Today.AddDays(-30), DateTime.Today, "daily");

        // True if the bundle-only filter is active (FR05)
        public bool BundleOnly { get; set; } = false;

        // Validates that filters are in a consistent, usable state
        public bool IsValid() => DateRange != null && DateRange.IsValid();
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>
    /// Value object representing a time window for report queries.
    /// Supports daily, weekly, monthly granularity (FR10).
    /// </summary>
    public class DateRange
    {
        public DateTime StartDate   { get; private set; }
        public DateTime EndDate     { get; private set; }

        // "daily" | "weekly" | "monthly" — controls chart x-axis (FR10)
        public string Granularity { get; private set; }

        public DateRange(DateTime start, DateTime end, string granularity = "daily")
        {
            StartDate   = start;
            EndDate     = end;
            Granularity = granularity;
        }

        // Returns true if a given date falls within this range
        public bool Contains(DateTime d) =>
            d.Date >= StartDate.Date && d.Date <= EndDate.Date;

        // Validates start is before end
        public bool IsValid() => StartDate <= EndDate;

        // Returns the number of calendar days in the range
        public int ToDays() => (int)(EndDate - StartDate).TotalDays + 1;

        public override string ToString() =>
            $"{StartDate:dd MMM yyyy} — {EndDate:dd MMM yyyy} ({Granularity})";
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>
    /// Data transfer object returned by ReportEngine.GenerateReport().
    /// Contains all analytics data needed by the dashboard views.
    /// </summary>
    public class Report
    {
        // Human-readable report title
        public string Title { get; set; } = string.Empty;

        // When this report was generated
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        // Applied filter summary
        public FilterSet AppliedFilters { get; set; } = new FilterSet();

        // Total revenue in the reporting window (BR-03)
        public float TotalRevenue { get; set; }

        // Total number of transactions
        public int TotalTransactions { get; set; }

        // Best-selling item per type: key = ItemType, value = item name + count
        public Dictionary<ItemType, (string ItemName, int Count)> TopByCategory
            { get; set; } = new();

        // Most purchased item per character class (BR-02)
        public Dictionary<CharacterClass, (string ItemName, int Count)> TopByClass
            { get; set; } = new();

        // Revenue by date for trend chart: key = date label, value = revenue (FR10)
        public Dictionary<string, float> RevenueTrend { get; set; } = new();

        // Items whose sales fall below the underperform threshold (FR11, BR-04)
        public List<(string ItemName, ItemType Type, int Sales)> UnderperformingItems
            { get; set; } = new();

        // Regional revenue breakdown (BR-06)
        public Dictionary<string, float> RevenueByRegion { get; set; } = new();

        // Bundle vs individual split: Item1 = bundle count, Item2 = individual count
        public (int BundleCount, int IndividualCount) BundleSplit { get; set; }

        // Spending tier distribution for demographic view (FR07)
        public Dictionary<SpendingTier, int> TierDistribution { get; set; } = new();
    }
}

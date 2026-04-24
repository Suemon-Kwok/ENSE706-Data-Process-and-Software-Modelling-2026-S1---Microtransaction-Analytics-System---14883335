// Name : Suemon Kwok

// Student ID : 14883335

// ReportModels.cs — FilterSet, DateRange, Report value objects

// Used by ReportEngine to filter and return analytics data.

// FR09, FR10, FR11 — filterable, trend, underperforming.

// What does this file do 
// three helper objects: FilterSet (what filters the user has applied), DateRange (a start/end date with daily/weekly/monthly granularity),
// and Report (a big container holding all the calculated output — total revenue, top sellers, trends, underperforming items, etc.).

// OOP concepts
// Value Objects (just a description of something) and Data Transfer Object pattern (carry data from one place to another). FilterSet, DateRange, and Report carry data between layers without behaviour.
// Report is a DTO — it holds pre-calculated results that the UI just displays

namespace GGG_MAS.Models                                                                                                                        // belongs to the shared model namespace
{
    /// <summary>
    
    /// Value object holding all active report filter criteria.
    
    /// Passed to ReportEngine.ApplyFilter() to narrow results.
    
    /// </summary>
    
    public class FilterSet
    {
        // Optional region filter — null means "all regions"
        public string? Region { get; set; }                                                                                                     // nullable string; e.g. "NZ"; null means no region filter applied

        // Optional item type filter — null means "all types"
        public ItemType? ItemType { get; set; }                                                                                                 // nullable enum; if set, only transactions for that item type are included

        // Optional character class filter — null means "all classes"
        public CharacterClass? CharacterClass { get; set; }                                                                                     // nullable enum; if set, only transactions for that class are included

        // Date range constraint — required; defaults to last 30 days
        public DateRange DateRange { get; set; } =
            new DateRange(DateTime.Today.AddDays(-30), DateTime.Today, "daily");                                                                // default: last 30 days at daily granularity

        // True if the bundle-only filter is active (FR05)
        public bool BundleOnly { get; set; } = false;                                                                                           // when true, only bundle transactions are included in results

        // Validates that filters are in a consistent, usable state
        public bool IsValid() => DateRange != null && DateRange.IsValid();                                                                      // ensures a date range exists and start <= end
    }

    
    /// <summary>
    
    /// Value object representing a time window for report queries.
    
    /// Supports daily, weekly, monthly granularity (FR10).
    
    /// </summary>
    
    public class DateRange
    {
        public DateTime StartDate   { get; private set; }                                                                                       // the first date included in the range (inclusive)
        
        public DateTime EndDate     { get; private set; }                                                                                       // the last date included in the range (inclusive)

        // "daily" | "weekly" | "monthly" — controls chart x-axis (FR10)
        public string Granularity { get; private set; }                                                                                         // determines how the trend chart groups data points

        public DateRange(DateTime start, DateTime end, string granularity = "daily")                                                            // granularity defaults to daily
        {
            StartDate   = start;                                                                                                                // stores the range start date

            EndDate     = end;                                                                                                                  // stores the range end date

            Granularity = granularity;                                                                                                          // stores how data will be grouped (daily / weekly / monthly)
        }

        // Returns true if a given date falls within this range
        public bool Contains(DateTime d) =>
            d.Date >= StartDate.Date && d.Date <= EndDate.Date;                                                                                 // compares date-only parts to avoid time-of-day issues

        // Validates start is before end
        public bool IsValid() => StartDate <= EndDate;                                                                                          // returns false if the user accidentally set start after end    

        // Returns the number of calendar days in the range
        public int ToDays() => (int)(EndDate - StartDate).TotalDays + 1;                                                                        // +1 because both endpoints are inclusive

        public override string ToString() =>
            $"{StartDate:dd MMM yyyy} — {EndDate:dd MMM yyyy} ({Granularity})";
    }

    
    /// <summary>
    
    /// Data transfer object returned by ReportEngine.GenerateReport().
    
    /// Contains all analytics data needed by the dashboard views.
    
    /// </summary>
    
    public class Report
    {
        // Human-readable report title
        public string Title { get; set; } = string.Empty;                                                                                       // built by ReportEngine.BuildTitle() from active filters

        // When this report was generated
        public DateTime GeneratedAt { get; set; } = DateTime.Now;                                                                               // captured at generation time for the export header

        // Applied filter summary
        public FilterSet AppliedFilters { get; set; } = new FilterSet();                                                                        // copy of the FilterSet used to generate this report

        // Total revenue in the reporting window (BR-03)
        public float TotalRevenue { get; set; }                                                                                                 // sum of all transaction prices within the filtered date range

        // Total number of transactions
        public int TotalTransactions { get; set; }                                                                                              // count of all transactions passing the applied filters

        // Best-selling item per type: key = ItemType, value = item name + count
        public Dictionary<ItemType, (string ItemName, int Count)> TopByCategory
            { get; set; } = new();                                                                                                              // maps each category to the single best-selling item name and its unit count

        // Most purchased item per character class (BR-02)
        public Dictionary<CharacterClass, (string ItemName, int Count)> TopByClass
            { get; set; } = new();                                                                                                              // maps each character class to its most popular item and purchase count

        // Revenue by date for trend chart: key = date label, value = revenue (FR10)
        public Dictionary<string, float> RevenueTrend { get; set; } = new();                                                                    // key is a formatted date string (e.g. "2024-01-15")

        // Items whose sales fall below the underperform threshold (FR11, BR-04)
        public List<(string ItemName, ItemType Type, int Sales)> UnderperformingItems
            { get; set; } = new();                                                                                                              // sorted by lowest sales count first

        // Regional revenue breakdown (BR-06)
        public Dictionary<string, float> RevenueByRegion { get; set; } = new();                                                                 // key is region code, value is total revenue

        // Bundle vs individual split: Item1 = bundle count, Item2 = individual count
        public (int BundleCount, int IndividualCount) BundleSplit { get; set; }                                                                 // used to calculate the bundle % stat card

        // Spending tier distribution for demographic view (FR07)
        public Dictionary<SpendingTier, int> TierDistribution { get; set; } = new();                                                            // maps each tier to transaction count
    }
}

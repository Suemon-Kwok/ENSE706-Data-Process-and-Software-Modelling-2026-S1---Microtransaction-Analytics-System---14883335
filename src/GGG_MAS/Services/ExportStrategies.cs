// Name: Suemon Kwok
// Student ID: 14883335
// ExportStrategies.cs — Concrete Strategy implementations (Task 6 / GoF Pattern 1)
//
// Design pattern: Strategy (GoF Behavioural)
// Each class is a ConcreteStrategy. They implement IExportStrategy using
// the actual Report field names from ReportModels.cs.

using System.Text;
using System.Text.Json;

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;

    // ── ConcreteStrategy A: CSV ───────────────────────────────────────────
    public class CsvExportStrategy : IExportStrategy
    {
        public void Export(Report report, string outputPath)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("Section,Key,Value");

            // Summary — uses actual Report field names
            sb.AppendLine($"Summary,Title,{Escape(report.Title)}");
            sb.AppendLine($"Summary,GeneratedAt,{report.GeneratedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Summary,TotalRevenue,{report.TotalRevenue:F2}");
            sb.AppendLine($"Summary,TotalTransactions,{report.TotalTransactions}");

            // Bundle split
            sb.AppendLine($"BundleSplit,BundleCount,{report.BundleSplit.BundleCount}");
            sb.AppendLine($"BundleSplit,IndividualCount,{report.BundleSplit.IndividualCount}");

            // Top by category — Dictionary<ItemType, (string ItemName, int Count)>
            foreach (var (itemType, tuple) in report.TopByCategory)
                sb.AppendLine($"TopByCategory,{itemType},{Escape(tuple.ItemName)},{tuple.Count}");

            // Top by class — Dictionary<CharacterClass, (string ItemName, int Count)>
            foreach (var (cls, tuple) in report.TopByClass)
                sb.AppendLine($"TopByClass,{cls},{Escape(tuple.ItemName)},{tuple.Count}");

            // Revenue trend — Dictionary<string, float>
            foreach (var (period, revenue) in report.RevenueTrend)
                sb.AppendLine($"RevenueTrend,{period},{revenue:F2}");

            // Revenue by region — Dictionary<string, float>
            foreach (var (region, revenue) in report.RevenueByRegion)
                sb.AppendLine($"RevenueByRegion,{region},{revenue:F2}");

            // Underperforming — List<(string ItemName, ItemType Type, int Sales)>
            foreach (var (itemName, type, sales) in report.UnderperformingItems)
                sb.AppendLine($"Underperforming,{Escape(itemName)},{type},{sales}");

            // Tier distribution — Dictionary<SpendingTier, int>
            foreach (var (tier, count) in report.TierDistribution)
                sb.AppendLine($"TierDistribution,{tier},{count}");

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        private static string Escape(string value) =>
            value.Contains(',') ? $"\"{value}\"" : value;
    }

    // ── ConcreteStrategy B: PDF (plain-text) ─────────────────────────────
    public class PdfExportStrategy : IExportStrategy
    {
        public void Export(Report report, string outputPath)
        {
            var sb    = new StringBuilder();
            var line  = new string('=', 60);
            var dash  = new string('-', 40);

            sb.AppendLine(line);
            sb.AppendLine("  GGG MICROTRANSACTION ANALYTICS SYSTEM");
            sb.AppendLine($"  {report.Title}");
            sb.AppendLine($"  Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine(line);
            sb.AppendLine();

            sb.AppendLine("SUMMARY");
            sb.AppendLine(dash);
            sb.AppendLine($"  Total revenue      : ${report.TotalRevenue:F2}");
            sb.AppendLine($"  Total transactions : {report.TotalTransactions}");
            sb.AppendLine($"  Bundle purchases   : {report.BundleSplit.BundleCount}");
            sb.AppendLine($"  Individual         : {report.BundleSplit.IndividualCount}");
            sb.AppendLine();

            sb.AppendLine("TOP SELLERS BY CATEGORY");
            sb.AppendLine(dash);
            foreach (var (itemType, tuple) in report.TopByCategory)
                sb.AppendLine($"  {itemType,-22} {tuple.ItemName} ({tuple.Count} sales)");
            sb.AppendLine();

            sb.AppendLine("TOP SELLERS BY CHARACTER CLASS");
            sb.AppendLine(dash);
            foreach (var (cls, tuple) in report.TopByClass)
                sb.AppendLine($"  {cls,-22} {tuple.ItemName} ({tuple.Count} sales)");
            sb.AppendLine();

            sb.AppendLine("REVENUE TREND");
            sb.AppendLine(dash);
            foreach (var (period, revenue) in report.RevenueTrend)
                sb.AppendLine($"  {period,-20} ${revenue:F2}");
            sb.AppendLine();

            if (report.UnderperformingItems.Count > 0)
            {
                sb.AppendLine("UNDERPERFORMING ITEMS");
                sb.AppendLine(dash);
                foreach (var (itemName, type, sales) in report.UnderperformingItems)
                    sb.AppendLine($"  {itemName,-30} {type,-20} sales: {sales}");
                sb.AppendLine();
            }

            sb.AppendLine(line);
            sb.AppendLine("  END OF REPORT");
            sb.AppendLine(line);

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }
    }

    // ── ConcreteStrategy C: JSON ──────────────────────────────────────────
    public class JsonExportStrategy : IExportStrategy
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented        = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public void Export(Report report, string outputPath)
        {
            var payload = new
            {
                title        = report.Title,
                generatedAt  = report.GeneratedAt,
                totalRevenue = report.TotalRevenue,
                totalTransactions = report.TotalTransactions,
                bundleSplit  = new
                {
                    bundleCount     = report.BundleSplit.BundleCount,
                    individualCount = report.BundleSplit.IndividualCount
                },
                topByCategory = report.TopByCategory
                    .Select(kv => new { category = kv.Key.ToString(),
                                        item     = kv.Value.ItemName,
                                        count    = kv.Value.Count }),
                topByClass = report.TopByClass
                    .Select(kv => new { characterClass = kv.Key.ToString(),
                                        item           = kv.Value.ItemName,
                                        count          = kv.Value.Count }),
                revenueTrend = report.RevenueTrend
                    .Select(kv => new { period = kv.Key, revenue = kv.Value }),
                revenueByRegion = report.RevenueByRegion
                    .Select(kv => new { region = kv.Key, revenue = kv.Value }),
                underperformingItems = report.UnderperformingItems
                    .Select(t => new { name = t.ItemName, type = t.Type.ToString(),
                                       sales = t.Sales }),
                tierDistribution = report.TierDistribution
                    .Select(kv => new { tier = kv.Key.ToString(), count = kv.Value })
            };

            string json = JsonSerializer.Serialize(payload, Options);
            File.WriteAllText(outputPath, json, Encoding.UTF8);
        }
    }
}

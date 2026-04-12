// =============================================================
// ExportService.cs — ExportService class (UML)
// Handles report export to CSV, PDF (text), and JSON.
// FR12: Marketing/Finance can export filterable reports (BR-09).
// SOLID: Single Responsibility — output formatting only.
// =============================================================

using System.Text;
using System.Text.Json;

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;

    /// <summary>
    /// Converts a Report DTO into a flat file for download.
    /// Format is selected at construction time.
    /// </summary>
    public class ExportService
    {
        // Chosen output format (CSV / PDF text / JSON)
        private readonly ExportFormat _format;

        public ExportService(ExportFormat format) => _format = format;

        // Entry point: dispatch to correct exporter based on format
        public void Export(Report report, string outputPath)
        {
            switch (_format)
            {
                case ExportFormat.CSV:  ExportToCsv(report, outputPath);  break;
                case ExportFormat.PDF:  ExportToPdf(report, outputPath);  break;
                case ExportFormat.JSON: ExportToJson(report, outputPath); break;
            }
        }

        // ── CSV export ────────────────────────────────────────
        // Produces a comma-separated values file readable in Excel
        private static void ExportToCsv(Report report, string path)
        {
            var sb = new StringBuilder();

            // Header row
            sb.AppendLine("Report Title,Generated At,Total Revenue,Total Transactions");
            sb.AppendLine($"\"{report.Title}\",{report.GeneratedAt:yyyy-MM-dd HH:mm}," +
                          $"{report.TotalRevenue:F2},{report.TotalTransactions}");

            // Top sellers per category
            sb.AppendLine();
            sb.AppendLine("Category,Top Item,Units Sold");
            foreach (var kv in report.TopByCategory)
                sb.AppendLine($"{kv.Key},{kv.Value.ItemName},{kv.Value.Count}");

            // Revenue trend
            sb.AppendLine();
            sb.AppendLine("Date,Revenue");
            foreach (var kv in report.RevenueTrend)
                sb.AppendLine($"{kv.Key},{kv.Value:F2}");

            // Underperforming items
            sb.AppendLine();
            sb.AppendLine("Underperforming Item,Type,Sales Count");
            foreach (var (name, type, sales) in report.UnderperformingItems)
                sb.AppendLine($"\"{name}\",{type},{sales}");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        // ── PDF (plain text fallback) ─────────────────────────
        // A simple readable text report when a PDF library is unavailable
        private static void ExportToPdf(Report report, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════╗");
            sb.AppendLine($"║  {report.Title,-42}║");
            sb.AppendLine($"║  Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm}            ║");
            sb.AppendLine("╚══════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"Total Revenue    : ${report.TotalRevenue:N2}");
            sb.AppendLine($"Total Transactions: {report.TotalTransactions}");
            sb.AppendLine();
            sb.AppendLine("── TOP SELLERS BY CATEGORY ──");
            foreach (var kv in report.TopByCategory)
                sb.AppendLine($"  {kv.Key,-22}: {kv.Value.ItemName} ({kv.Value.Count} units)");
            sb.AppendLine();
            sb.AppendLine("── REVENUE TREND ──");
            foreach (var kv in report.RevenueTrend)
                sb.AppendLine($"  {kv.Key,-14}: ${kv.Value:N2}");
            sb.AppendLine();
            sb.AppendLine("── UNDERPERFORMING ITEMS ──");
            foreach (var (name, type, sales) in report.UnderperformingItems)
                sb.AppendLine($"  {name} [{type}] — {sales} sales");

            // Save with .txt extension since we are not using a PDF library
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        // ── JSON export ───────────────────────────────────────
        // Machine-readable JSON suitable for downstream data pipelines
        private static void ExportToJson(Report report, string path)
        {
            // Anonymous DTO to keep the JSON structure clean and flat
            var dto = new
            {
                report.Title,
                GeneratedAt      = report.GeneratedAt.ToString("o"),
                report.TotalRevenue,
                report.TotalTransactions,
                TopByCategory    = report.TopByCategory.ToDictionary(
                                       k => k.Key.ToString(),
                                       v => new { v.Value.ItemName, v.Value.Count }),
                RevenueTrend     = report.RevenueTrend,
                Underperforming  = report.UnderperformingItems
                                         .Select(u => new { u.ItemName, u.Type, u.Sales })
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(dto, options), Encoding.UTF8);
        }
    }
}

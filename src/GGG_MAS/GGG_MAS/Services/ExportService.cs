// Name : Suemon Kwok

// Student ID : 14883335

// ExportService.cs — ExportService class (UML)

// Handles report export to CSV, PDF (text), and JSON.

// FR12: Marketing/Finance can export filterable reports (BR-09).



using System.Text;                                                                                                                            // provides StringBuilder for building file content

using System.Text.Json;                                                                                                                       // provides JsonSerializer for JSON export

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;                                                                                                                    // brings in Report, ExportFormat, and all model types

    /// <summary>
    
    /// Converts a Report DTO into a flat file for download.
    
    /// Format is selected at construction time.
    
    /// </summary>
    public class ExportService
    {
        // Chosen output format (CSV / PDF text / JSON)
        private readonly ExportFormat _format;                                                                                               // stored at construction so Export() dispatches to the correct method

        public ExportService(ExportFormat format) => _format = format;                                                                       // constructor stores the chosen format

        // Entry point: dispatch to correct exporter based on format
        public void Export(Report report, string outputPath)
        {
            switch (_format)                                                                                                                // selects which private export method to call
            {
                case ExportFormat.CSV:  ExportToCsv(report, outputPath);  break;                                                            // CSV: readable in Excel        

                case ExportFormat.PDF:  ExportToPdf(report, outputPath);  break;                                                            // PDF: formatted plain-text report

                case ExportFormat.JSON: ExportToJson(report, outputPath); break;                                                            // JSON: machine-readable output
            }
        }

        // CSV export
        
        // Produces a comma-separated values file readable in Excel
        private static void ExportToCsv(Report report, string path)
        {
            var sb = new StringBuilder();                                                                                                   // StringBuilder avoids repeated string allocations in a loop

            // Header row : column names for the summary section
            sb.AppendLine("Report Title,Generated At,Total Revenue,Total Transactions");
            
            sb.AppendLine($"\"{report.Title}\",{report.GeneratedAt:yyyy-MM-dd HH:mm}," +
                          $"{report.TotalRevenue:F2},{report.TotalTransactions}");                                                          // F2 = two decimal places for revenue

            // Top sellers per category
            sb.AppendLine();                                                                                                                // blank line between sections

            sb.AppendLine("Category,Top Item,Units Sold");                                                                                  // column headers for top-seller section
            foreach (var kv in report.TopByCategory)                                                                                        // iterates each ItemType → (name, count) pair
                sb.AppendLine($"{kv.Key},{kv.Value.ItemName},{kv.Value.Count}");

            // Revenue trend
            sb.AppendLine();
            
            sb.AppendLine("Date,Revenue");                                                                                                  // column headers for trend section
            foreach (var kv in report.RevenueTrend)                                                                                         // iterates date-label → revenue pairs
                sb.AppendLine($"{kv.Key},{kv.Value:F2}");

            // Underperforming items
            sb.AppendLine();
            
            sb.AppendLine("Underperforming Item,Type,Sales Count");                                                                         // column headers for underperform section
            foreach (var (name, type, sales) in report.UnderperformingItems)                                                                // deconstructs each tuple
                sb.AppendLine($"\"{name}\",{type},{sales}");                                                                                // quotes item name in case it contains commas

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);                                                                          // writes the complete CSV string to disk as UTF-8
        }

        // PDF (plain text fallback)
        // A simple readable text report when a PDF library is unavailable
        private static void ExportToPdf(Report report, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════╗");
            sb.AppendLine($"║  {report.Title,-42}║");                                                                                       // -42 = left-aligns title in a 42-char field
            sb.AppendLine($"║  Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm}            ║");
            sb.AppendLine("╚══════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"Total Revenue    : ${report.TotalRevenue:N2}");                                                                 // N2 = thousand-separated, 2 decimal places
            sb.AppendLine($"Total Transactions: {report.TotalTransactions}");
            sb.AppendLine();
            sb.AppendLine("── TOP SELLERS BY CATEGORY ──");
            foreach (var kv in report.TopByCategory)                                                                                        // iterates each category and its top item
                sb.AppendLine($"  {kv.Key,-22}: {kv.Value.ItemName} ({kv.Value.Count} units)");                                             // -22 = pads category name
            sb.AppendLine();
            sb.AppendLine("── REVENUE TREND ──");
            foreach (var kv in report.RevenueTrend)                                                                                         // iterates each date-label → revenue pair
                sb.AppendLine($"  {kv.Key,-14}: ${kv.Value:N2}");
            sb.AppendLine();
            sb.AppendLine("── UNDERPERFORMING ITEMS ──");
            foreach (var (name, type, sales) in report.UnderperformingItems)                                                                // deconstructs each underperform tuple
                sb.AppendLine($"  {name} [{type}] — {sales} sales");

            // Save with .txt extension since we are not using a PDF library
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);                                                                          // writes the formatted text to disk
        }

        // JSON export
        
        // Machine-readable JSON suitable for downstream data pipelines
        private static void ExportToJson(Report report, string path)
        {
            // Anonymous DTO to keep the JSON structure clean and flat
            var dto = new
            {
                report.Title,                                                                                                               // copies Title directly
                GeneratedAt      = report.GeneratedAt.ToString("o"),                                                                        // ISO 8601 format for unambiguous timestamps
                report.TotalRevenue,
                report.TotalTransactions,
                TopByCategory    = report.TopByCategory.ToDictionary(                                                                       // converts Dictionary<ItemType,tuple> to string keys
                                       k => k.Key.ToString(),
                                       v => new { v.Value.ItemName, v.Value.Count }),
                RevenueTrend     = report.RevenueTrend,                                                                                     // already Dictionary<string,float> — serialises directly
                Underperforming  = report.UnderperformingItems                                                                              // projects each tuple to an anonymous object
                                         .Select(u => new { u.ItemName, u.Type, u.Sales })
            };

            var options = new JsonSerializerOptions { WriteIndented = true };                                                               // WriteIndented = pretty-print with newlines

            File.WriteAllText(path, JsonSerializer.Serialize(dto, options), Encoding.UTF8);                                                 // serialises the DTO and writes to disk
        }
    }
}

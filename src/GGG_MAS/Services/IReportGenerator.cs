// =============================================================
// IReportGenerator.cs — ReportGenerator interface (UML)
// Contract for any class that generates analytics reports.
// SOLID: Dependency Inversion — consumers depend on abstraction.
// =============================================================

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;

    /// <summary>
    /// Interface that ReportEngine and any future report backends must implement.
    /// Enables easy substitution of report generation strategies.
    /// </summary>
    public interface IReportGenerator
    {
        // FR09: Generate a report using the supplied filter criteria
        Report GenerateReport(FilterSet filters,
                              IEnumerable<Transaction> transactions,
                              IEnumerable<MTXItem> catalogue);

        // FR12: Export the given report to a file in the chosen format
        void ExportReport(Report report, ExportFormat format, string outputPath);

        // Apply or update filters to the current view (updates internal state)
        void ApplyFilter(FilterSet filters);
    }
}

// Name : Suemon Kwok

// Student ID : 14883335

// IReportGenerator.cs — ReportGenerator interface (UML)

// Contract for any class that generates analytics reports.

// What does this file do
// another contract, this time for the report engine. Any report engine must implement GenerateReport(), ExportReport(), and ApplyFilter().
// This means you could swap out the engine without touching the rest of the code.

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;                                                                               // brings in Report, FilterSet, Transaction, MTXItem, ExportFormat

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

        // Takes a FilterSet, all transactions, and the full catalogue; returns a populated Report DTO

        // FR12: Export the given report to a file in the chosen format
        void ExportReport(Report report, ExportFormat format, string outputPath);

        // Apply or update filters to the current view (updates internal state)
        void ApplyFilter(FilterSet filters);                                                           // Stores the FilterSet so subsequent GenerateReport calls use it automatically        
    }
}

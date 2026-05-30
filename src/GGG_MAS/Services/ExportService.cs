// Name: Suemon Kwok
// Student ID: 14883335
// ExportService.cs — Context class (Task 6 / GoF Pattern 1 — Strategy)
//
// Design pattern: Strategy (GoF Behavioural)
//
// BEFORE (Phase 1):
//   ExportService required format injected at construction time:
//       new ExportService(ExportFormat.CSV).Export(report, path)
//   Adding a new format required editing this class — violating OCP.
//
// AFTER (Phase 2):
//   SetStrategy() injects whichever IExportStrategy is needed.
//   Export() delegates in one line — no switch, no format knowledge here.
//   Adding XML export = create XmlExportStrategy. Zero changes to this class.

using System.Text;
using System.Text.Json;

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;

    /// <summary>
    /// Context class for the Strategy pattern.
    /// Delegates serialisation to whichever <see cref="IExportStrategy"/> is active.
    /// Also preserves the original constructor so existing call sites (Program.cs,
    /// MainDashboardForm) do not need to change.
    /// </summary>
    public class ExportService
    {
        // ── Legacy constructor (Phase 1 compatibility) ────────────────────
        // Keeps original call sites compiling unchanged.
        // When called this way, the matching concrete strategy is injected automatically.
        private readonly ExportFormat? _legacyFormat;

        public ExportService(ExportFormat format)
        {
            _legacyFormat = format;
            // Wire up the matching strategy immediately
            _strategy = format switch
            {
                ExportFormat.CSV  => new CsvExportStrategy(),
                ExportFormat.PDF  => new PdfExportStrategy(),
                ExportFormat.JSON => new JsonExportStrategy(),
                _                 => new CsvExportStrategy()
            };
        }

        // ── Default constructor for Phase 2 direct strategy injection ─────
        public ExportService() { }

        // ── Strategy ──────────────────────────────────────────────────────
        private IExportStrategy? _strategy;

        /// <summary>Inject a strategy by interface reference (Phase 2 usage).</summary>
        public void SetStrategy(IExportStrategy strategy)
        {
            _strategy = strategy
                ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>Resolve strategy from enum (convenience overload).</summary>
        public void SetStrategy(ExportFormat format)
        {
            _strategy = format switch
            {
                ExportFormat.CSV  => new CsvExportStrategy(),
                ExportFormat.PDF  => new PdfExportStrategy(),
                ExportFormat.JSON => new JsonExportStrategy(),
                _                 => throw new ArgumentOutOfRangeException(
                                         nameof(format), $"Unknown format: {format}")
            };
        }

        // ── Export ────────────────────────────────────────────────────────
        public void Export(Report report, string outputPath)
        {
            if (_strategy is null)
                throw new InvalidOperationException(
                    "Call SetStrategy() before Export().");

            _strategy.Export(report, outputPath);
        }
    }
}

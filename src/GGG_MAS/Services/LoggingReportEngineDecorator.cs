// Name: Suemon Kwok
// Student ID: 14883335
// LoggingReportEngineDecorator.cs — Decorator pattern (Task 6 / GoF Pattern 5)
//
// Design pattern: Decorator (GoF Structural)
// Wraps any IReportGenerator and writes a timestamped audit entry before and
// after each GenerateReport() and ExportReport() call.
// Satisfies UC12: "Export action is logged for audit purposes."


/*
What design pattern is used and why?
Design pattern: Decorator — GoF Structural category.
Phase 1 had no audit logging for report generation or export actions. 
UC12 requires that every export action be logged. 
Two naive solutions were rejected: (1) adding logging directly to ReportEngine violates SRP — analytics is ReportEngine's job, not logging. (2) subclassing ReportEngine into LoggingReportEngine locks logging to one concrete class. If ReportEngine is replaced with a DatabaseReportEngine, logging disappears.

LoggingReportEngineDecorator wraps any IReportGenerator and adds timestamped audit log entries before and after each call — transparently, without modifying ReportEngine at all.

*/

/*
What does the Decorator pattern do?
Decorator attaches additional responsibilities to an object dynamically. It provides a flexible alternative to subclassing for extending functionality.
The ENSE706 lecture analogy: clothing layers. A person wearing a t-shirt, then a jumper, then a raincoat. Each layer adds behaviour (warmth, waterproofing) without changing the person underneath.

In this project: new LoggingReportEngineDecorator(new ReportEngine(15f), "audit.log") wraps ReportEngine. 
When GenerateReport() is called, the decorator logs 'requested', calls _inner.GenerateReport() (the real engine), then logs 'complete'. ReportEngine never changes.

*/

/*
Why Decorator and not subclassing?
Subclassing (inheritance) is static — it is decided at compile time. If you subclass ReportEngine into LoggingReportEngine, you can only log that specific class. 
If ReportEngine is later replaced with DatabaseReportEngine, logging is gone.

Decorator is dynamic — it wraps any object that implements IReportGenerator. 
Swap ReportEngine for DatabaseReportEngine at runtime — the decorator wraps it without code changes. The ENSE706 lecture slides describe this as composition over inheritance.

*/

/*
Which GoF category?
Structural — Decorator describes how objects are composed to extend behaviour. It uses composition (the decorator holds a reference to the wrapped object) to build up behaviour layer by layer.

*/

/*
Overriding in Decorator
LoggingReportEngineDecorator implements IReportGenerator and overrides all three methods: GenerateReport(), ExportReport(), and ApplyFilter(). Each override adds the logging behaviour and then delegates to _inner.

This is overriding because each method has the same signature as the interface definition — same name, same parameters, different body. 
The Decorator is essentially overriding interface methods to intercept the call.

*/

/*
Abstraction and Encapsulation in Decorator
Abstraction: the field private readonly IReportGenerator _inner is typed as the interface, not the concrete ReportEngine. The decorator does not know or care what concrete engine is inside — it just calls the interface methods.
Encapsulation: the WriteLog() method is private. Callers cannot directly write to the audit log — they must go through GenerateReport() or ExportReport(). The log path is stored in a private readonly string _logPath that callers cannot change after construction.

*/

/*
Tight coupling — how Decorator prevents it
Without Decorator: adding logging directly to ReportEngine tightly couples analytics code with I/O code. Every change to logging (format, destination, level) requires editing ReportEngine — a class with a completely unrelated purpose.

With Decorator: ReportEngine has zero logging code. Logging is entirely in LoggingReportEngineDecorator. These two concerns are completely decoupled. Changing the log format requires editing only the decorator file.

*/

/*
Advantages of Decorator
Adds behaviour without modifying existing classes — ReportEngine unchanged
Composable: TimingDecorator can wrap LoggingDecorator which wraps ReportEngine
Satisfies SRP: ReportEngine does analytics, decorator does logging — each has one job
Transparent: callers receive IReportGenerator — they cannot detect the decorator

*/

/*
Disadvantages of Decorator
Stacking order matters: logging-then-timing logs different data than timing-then-logging
Deep stacking makes call stack harder to inspect without a debugger
Each decorator must implement all interface methods — even ones that need no decoration
Can be overkill for a single cross-cutting concern on a small codebase

*/

using GGG_MAS.Models;

namespace GGG_MAS.Services
{
    // LoggingReportEngineDecorator is the Decorator class.
    // It implements IReportGenerator (same interface as ReportEngine — the Component)
    // so it can be used anywhere a ReportEngine is expected. Callers cannot tell the difference.
    public class LoggingReportEngineDecorator : IReportGenerator
    {
        // The real IReportGenerator being wrapped
        // _inner: the real IReportGenerator being wrapped (e.g. ReportEngine).
        // typed as the INTERFACE — not ReportEngine. This is abstraction:
        // the decorator does not know or care what concrete class is inside.
        // It could wrap ReportEngine today and DatabaseReportEngine tomorrow — same code.
        private readonly IReportGenerator _inner;

        // _logPath: the file path for the audit log (e.g. "audit.log").
        // private readonly: set once in the constructor, can never be changed externally.
        // This is encapsulation — callers cannot redirect the log to a different file after creation.
        private readonly string           _logPath;


        // Constructor: takes any IReportGenerator to wrap plus the audit log file path.
        // This is constructor injection — dependencies are provided at creation time.
        // ?? throw: null guard — crashes immediately with a clear message if null is passed,
        //           rather than failing silently later with a NullReferenceException deep in the call stack.
        public LoggingReportEngineDecorator(IReportGenerator inner, string logPath)
        {
            _inner   = inner   ?? throw new ArgumentNullException(nameof(inner));
            _logPath = logPath ?? throw new ArgumentNullException(nameof(logPath));
        }

        // IReportGenerator: GenerateReport
        // Signature matches actual interface: (FilterSet, IEnumerable<Transaction>,
        //                                      IEnumerable<MTXItem>) : Report
        // GenerateReport() — OVERRIDES the interface method to add logging around the real call.
        // PRE: log that the report was requested, including what filters are active.
        // DELEGATE: call _inner.GenerateReport() — this is the real ReportEngine doing the work.
        // POST: log that the report completed successfully, including total transactions and revenue.
        // The return value is passed straight through — the decorator adds no data of its own.
        public Report GenerateReport(
            FilterSet                filters,           // date range, region, item type, character class
            IEnumerable<Transaction> transactions,      // all transactions in the repository
            IEnumerable<MTXItem>     catalogue)         // all MTX items in the catalogue
        {
            // PRE-LOG: record that this operation was requested, with filter details for traceability.
            WriteLog($"GenerateReport requested | filters: {DescribeFilters(filters)}");

            // DELEGATION: the real analytics work happens here — inside ReportEngine.
            // The decorator does not perform any calculations itself.
            Report result = _inner.GenerateReport(filters, transactions, catalogue);

            // POST-LOG: record that the operation completed. Includes title, count, and revenue
            // so audit entries are meaningful without needing to open the report file.
            WriteLog($"GenerateReport complete  | title: \"{result.Title}\" " +
                     $"| transactions: {result.TotalTransactions} " +
                     $"| revenue: ${result.TotalRevenue:F2}");

            return result;  // transparent — caller receives the real Report, unchanged
        }

        // IReportGenerator: ExportReport
        // Signature matches actual interface: (Report, ExportFormat, string) : void
        // ExportReport() — OVERRIDES the interface method to satisfy UC12:
        // "Export action is logged for audit purposes."
        // PRE: log the export attempt with format, report title, and output path.
        // DELEGATE: let _inner write the actual file.
        // POST: confirm the export completed without an exception.
        public void ExportReport(Report report, ExportFormat format, string outputPath)
        {
            // PRE-LOG: all three parameters logged so the audit entry is fully traceable.
            // Format tells us CSV/PDF/JSON. Title identifies the report. Path confirms where it went.
            WriteLog($"ExportReport requested   | format: {format} " +
                     $"| report: \"{report.Title}\" | path: {outputPath}");

            // DELEGATION: the real export work (file writing) happens in _inner.
            _inner.ExportReport(report, format, outputPath);

            // POST-LOG: if we reach this line, the export succeeded without throwing an exception.
            WriteLog($"ExportReport complete    | format: {format} | path: {outputPath}");
        }

        // IReportGenerator: ApplyFilter
        // ApplyFilter() — passes the filter directly to _inner with no logging.
        // Filter changes are not audit-relevant events — no log entry needed.
        // => expression body shorthand for { _inner.ApplyFilter(filters); }
        // This method must still be implemented because the interface requires it.
        public void ApplyFilter(FilterSet filters) => _inner.ApplyFilter(filters);

        // Logging helper
        // WriteLog() — private helper that writes a single timestamped line to the audit log.
        // private: only this class can write to the log — callers cannot inject arbitrary entries.
        // File.AppendAllText: APPENDS to the file — existing log entries are never overwritten.
        // try/catch IOException: log failures are silently swallowed.
        //   Rationale: a logging error must NEVER crash the analytics operation the user is waiting for.
        //   In production this would escalate to an event log or monitoring dashboard.
        private void WriteLog(string message)
        {
            try
            {
                // Format: "2026-05-30 14:23:01 | GenerateReport requested | filters: ..."
                // yyyy-MM-dd HH:mm:ss gives a sortable, unambiguous timestamp for any region.
                string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
                File.AppendAllText(_logPath, entry + Environment.NewLine);
            }
            catch (IOException)
            {
                // Log failures are swallowed — never crash the analytics operation
                // Swallow I/O errors — logging must never bring down the analytics pipeline.
            }
        }

        // Uses actual FilterSet fields: DateRange.StartDate / EndDate, Region,
        // ItemType, CharacterClass — all nullable
        // DescribeFilters() — converts the FilterSet into a compact readable string for the log.
        // static: does not access any instance fields — can be called without an object.
        // private: only used internally by WriteLog calls above.
        // Nullable-safe:
        //   f.DateRange?: DateRange may be null (no date filter applied).
        //   f.Region ?? "all": if Region is null, write "all" instead.
        //   f.ItemType?.ToString() ?? "all": ItemType is nullable enum — same pattern.
        private static string DescribeFilters(FilterSet f)
        {
            if (f is null) return "none";   // guard: if no FilterSet at all, just write "none"
            return $"from={f.DateRange?.StartDate:yyyy-MM-dd} " +           // start of date range
                   $"to={f.DateRange?.EndDate:yyyy-MM-dd} " +               // end of date range
                   $"region={f.Region ?? "all"} " +                         // RegionCode or "all"
                   $"itemType={f.ItemType?.ToString() ?? "all"} " +         // ItemType enum or "all"
                   $"class={f.CharacterClass?.ToString() ?? "all"}";        // CharacterClass or "all"
        }
    }
}

// Name: Suemon Kwok
// Student ID: 14883335
// IExportStrategy.cs — Strategy pattern interface (Task 6 / GoF Pattern 1)
//
// Design pattern: Strategy (GoF Behavioural)
//
// WHY THIS PATTERN HERE:
// Phase 1 ExportService used a switch on ExportFormat to call different formatting
// blocks inline. That created three problems:
//   1. Adding a new format (e.g. XML) required editing ExportService — violating OCP.
//   2. All formatting logic was tangled in one class — violating SRP.
//   3. The class could not be tested with a fake formatter — coupling to concrete code.
//
// Extracting IExportStrategy removes all three problems. ExportService holds a
// reference to the interface; the caller injects whichever concrete strategy it
// needs. Adding XML export = create XmlExportStrategy. Zero changes to ExportService.

/*
What design pattern is used and why?
Design pattern: Strategy — GoF Behavioural category.

Phase 1 ExportService.cs (418 lines) embedded all CSV, PDF, and JSON serialisation logic as private methods inside a switch block. 
Adding XML export required editing that file — violating the Open/Closed Principle. All three formats were tangled in one class — violating Single Responsibility. The class could not be tested with a fake formatter.

The Strategy pattern extracts each algorithm (CSV, PDF, JSON) into its own class, all sharing the IExportStrategy interface. 
ExportService becomes a pure context class — it holds a reference to the active strategy and calls _strategy.Export(report, path) in one line.

*/

/*
What does the Strategy pattern do?
Strategy defines a family of algorithms, encapsulates each one in a separate class, and makes them interchangeable at runtime without changing the class that uses them.
In this project: CsvExportStrategy, PdfExportStrategy, and JsonExportStrategy are three interchangeable algorithms. ExportService (the Context) does not care which one is active. 
The user's format selection at runtime determines which strategy is injected via SetStrategy().

*/

/*
Why Strategy and not Template Method?
Template Method was considered. Template Method defines a skeleton algorithm in a base class and lets subclasses fill in specific steps. 
It would work if CSV, PDF, and JSON shared common steps (e.g. open file → write header → write body → close file).

They do not. CSV produces flat comma-separated text, PDF produces formatted paragraph blocks, and JSON produces a nested object tree. 
There is no shared skeleton — the entire algorithm differs. Strategy is the correct choice when the whole algorithm varies, not just a step inside it.

*/

/*
Which GoF category?
Behavioural — Strategy is about how objects communicate and delegate behaviour at runtime. 
It defines a family of interchangeable algorithms and makes them swappable without changing the class that calls them.
*/

/*
What is polymorphism and why is it essential to Strategy?

Polymorphism means one interface, many implementations. The same method call produces different behaviour depending on which concrete object is behind the interface reference.
In Strategy: _strategy.Export(report, path) is one line of code. At runtime it executes CsvExportStrategy.Export(), PdfExportStrategy.Export(), or JsonExportStrategy. Export() — three completely different behaviours 
Without ExportService knowing or caring which one runs.

Without polymorphism, Strategy does not exist. Polymorphism is the mechanism that makes the swap invisible to the context class. 
This is why the interface IExportStrategy is critical — it is the contract that enables polymorphic dispatch.

*/

/*
What is the difference between overriding and overloading?' See section below — 
this comes up when discussing how strategies implement the interface method.
*/

/*
Overriding vs Overloading — how they apply here
Method overriding: a subclass (or interface implementor) provides its own version of a method defined in a parent class or interface. Same name, same parameters, different implementation.
Example in this project: CsvExportStrategy.Export(Report, string) overrides IExportStrategy.Export(Report, string). Same signature — different body. The CSV strategy writes comma-separated text; the PDF strategy writes formatted lines. This is overriding.

Method overloading: the same class has multiple methods with the same name but different parameter lists. The compiler picks the right one based on the arguments.
Example in this project: ExportService.SetStrategy(IExportStrategy strategy) and ExportService.SetStrategy(ExportFormat format) — same name, different parameters. When called with an enum, the second overload runs. When called with an interface reference, the first runs. This is overloading.

Overriding is resolved at RUNTIME (polymorphism). 
Overloading is resolved at COMPILE TIME (the compiler picks based on argument types).
*/

/*
Tight coupling — how Strategy solves it
Phase 1 tight coupling: MainDashboardForm called new ExportService(ExportFormat.CSV).Export(...). 
It was coupled to both the ExportService class AND the ExportFormat enum AND the specific format. Any change to how CSV worked required tracing back to MainDashboardForm.

Phase 2 loose coupling: MainDashboardForm calls exportService.SetStrategy(ExportFormat.CSV); exportService.Export(report, path). 
ExportService is coupled only to IExportStrategy — an interface, not a concrete class. CsvExportStrategy can be completely rewritten without MainDashboardForm knowing.

*/

/*
Abstraction and Encapsulation in Strategy
Abstraction: IExportStrategy hides HOW export works. ExportService only sees the contract Export(Report, string). 
Whether that writes a CSV file or a JSON blob is completely hidden behind the interface.

Encapsulation: each strategy class encapsulates its own formatting logic privately. 
CsvExportStrategy has a private Escape() helper method. JsonExportStrategy has private JsonSerializerOptions. These are internal details — callers never see them.

*/

/*
Advantages of Strategy
Open/Closed: add XML export by creating XmlExportStrategy — zero changes to ExportService
Single Responsibility: each strategy class has one job — format and write one file type
Testable: each strategy can be unit tested independently with a fake Report
Runtime swappable: user can choose CSV mid-session, then JSON, without restarting

*/

/*
Disadvantages of Strategy
More classes: 5 files instead of 1 — new developer must understand the pattern to trace the call
Indirection: Export() delegates to _strategy.Export() — slightly harder to debug
SetStrategy() must be called before Export() — forgetting causes a runtime InvalidOperationException
Overkill for simple cases with only 2 fixed formats — pattern adds complexity that must be justified

*/

using GGG_MAS.Models;

namespace GGG_MAS.Services
{
    /// <summary>
    /// Strategy contract for all report export formats.
    /// Implementations must serialise a <see cref="Report"/> to a file
    /// at the specified output path.
    /// </summary>

    // IExportStrategy is the Strategy interface — the contract all concrete strategies must fulfil.
    // interface = pure contract, no code. Any class implementing this MUST provide Export().
    // This is what enables polymorphism: ExportService holds an IExportStrategy reference
    // and calls Export() without knowing which concrete class is behind it.

    public interface IExportStrategy
    {
        /// <summary>
        /// Serialises the report and writes it to <paramref name="outputPath"/>.
        /// </summary>
        /// <param name="report">The report DTO produced by ReportEngine.</param>
        /// <param name="outputPath">Fully qualified destination file path.</param>
        /// <exception cref="IOException">
        /// Thrown when the file system cannot write to the specified path.
        /// </exception>

        // Export() is the single method all strategies must implement.
        // report: the data to serialise — produced by ReportEngine.
        // outputPath: the full file path to write to (e.g. "C:\reports\sales.csv").
        // void: no return value — the result is the file written to disk.
        //
        // Each concrete strategy (CSV, PDF, JSON) implements this method differently:
        // CsvExportStrategy writes comma-separated rows.
        // PdfExportStrategy writes formatted plain-text paragraphs.
        // JsonExportStrategy writes an indented JSON object tree.
        // ExportService never knows which implementation runs — it calls Export() and trusts the interface.
        //
        // IOException is declared here to signal to callers that file write failures are possible
        // and should be caught in the calling code (MainDashboardForm).

        void Export(Report report, string outputPath);
    }
}

// Name: Suemon Kwok
// Student ID: 14883335
// LegacyExportServiceAdapter.cs — Adapter pattern (Task 6 / GoF Pattern 4 — Structural)
//
// Design pattern: Adapter (GoF Structural)
//
// INTENT (GoF definition):
// Convert the interface of a class into another interface that clients expect.
// Adapter lets classes work together that could not otherwise because of
// incompatible interfaces.
//
// WHY THIS PATTERN HERE:
// Phase 1 ExportService has this signature:
//     public ExportService(ExportFormat format)
//     public void Export(Report report, string outputPath)
//
// Phase 2 introduces IExportStrategy:
//     void Export(Report report, string outputPath)
//
// The method name is the same but the construction contract is incompatible —
// Phase 1 requires the format at construction time while IExportStrategy is
// format-agnostic (the strategy IS the format). This means the old ExportService
// cannot be used wherever IExportStrategy is expected without modification.
//
// LegacyExportServiceAdapter wraps the old ExportService (the Adaptee) and
// exposes it through IExportStrategy (the Target interface). The rest of the
// application never knows it is talking to the old class.
//
// STRUCTURAL ROLES:
//   Target    → IExportStrategy           (what the client expects)
//   Adaptee   → ExportService (Phase 1)   (what already exists, incompatible)
//   Adapter   → LegacyExportServiceAdapter (this class — bridges the gap)
//   Client    → ExportService (Phase 2 context) / MainDashboardForm
//
// BENEFIT:
// Allows the Phase 1 ExportService to be reused in a Phase 2 context without
// modifying it. If the Phase 1 code is later replaced entirely, only this
// adapter file needs to be deleted — no other class changes.

/*
What design pattern is used and why?
Design pattern: Adapter — GoF Structural category.
Phase 1 ExportService required the format at construction time: new ExportService(ExportFormat.CSV). 

Phase 2 IExportStrategy is format-agnostic — the strategy IS the format. These two contracts are incompatible. 
LegacyExportServiceAdapter wraps the old class (Adaptee) and exposes it through the new interface (Target) without modifying either class.

*/

/*
What does the Adapter pattern do?
Adapter converts the interface of an existing class into an interface that clients expect. It lets two incompatible classes work together by acting as a translator between them.

Real-world analogy used in the ENSE706 lecture slides: a New Zealand power adapter lets a European charger plug into a NZ socket. The charger and socket are both unchanged — the adapter bridges the incompatibility.

In this project: LegacyExportService is the European charger (the Adaptee). IExportStrategy is the NZ socket (the Target). 
LegacyExportServiceAdapter is the adapter — it holds a LegacyExportService internally and delegates Export() calls to it.

*/

/*
Object Adapter vs Class Adapter — which is this and why?
Object Adapter (this project) uses composition: the adapter holds a private field _adaptee: LegacyExportService. It wraps the adaptee. This is the standard approach because it works even when the adaptee class is sealed or otherwise not inheritable.

Class Adapter uses inheritance: the adapter would extend LegacyExportService AND implement IExportStrategy. 
This only works in languages that allow multiple inheritance (C++ supports it; C# does not for classes). 
In C# you can inherit one class and implement multiple interfaces, but if the adaptee is sealed (which LegacyExportService could be), class adapter is impossible.

Rule: object adapter = composition (has-a). Class adapter = inheritance (is-a). Composition is generally preferred because it is more flexible and does not lock the adapter into the adaptee's inheritance hierarchy.

Why use composition over inheritance for the adapter?' 
Answer: composition allows wrapping sealed classes, enables runtime swapping of adaptees, and avoids inheriting unwanted behaviour from the adaptee class.
*/

/*
Abstraction and Encapsulation in Adapter
Abstraction: from ExportService's perspective, LegacyExportServiceAdapter is just another IExportStrategy. ExportService calls _strategy.Export(report, path) — it never knows it is talking to legacy code.

Encapsulation: the adaptee field private readonly LegacyExportService _adaptee is private. Callers cannot reach the legacy service directly. All access goes through Export(), which translates the call.

*/

/*
Which GoF category?
Structural — Adapter is about how classes and objects are composed to form larger structures. It describes the structural relationship between the adapter, adaptee, and target interface.

*/

/*
Why Adapter and not Facade?
Facade simplifies a complex subsystem by providing a single, simpler interface to it. 
Adapter specifically converts one incompatible interface into another. The key difference: Facade reduces complexity; Adapter resolves incompatibility.

This is an Adapter because the incompatibility is specific and structural
Phase 1 requires format at construction, Phase 2 requires format-agnostic construction. Facade would not address this.

*/

/*
Advantages of Adapter
Reuses existing code without modifying it — LegacyExportService unchanged
Single Responsibility: adapter only translates — no extra logic
If legacy class is retired, delete one file — no other class changes
Open/Closed: extends behaviour without modifying the original class

*/

/*
Disadvantages of Adapter
Adds an extra layer of indirection — slightly harder to trace calls
Can hide poor design: legacy code may be adapted instead of properly rewritten
Risk of becoming permanent despite being intended as transitional
Object adapter holds a reference — slightly more memory than a class adapter

*/
using GGG_MAS.Models;

namespace GGG_MAS.Services
{
    /// <summary>
    /// Adapter that wraps the Phase 1 <see cref="LegacyExportService"/> (Adaptee)
    /// and exposes it as an <see cref="IExportStrategy"/> (Target interface).
    /// </summary>

    // LegacyExportServiceAdapter is the ADAPTER class.
    // It implements IExportStrategy (the Target interface) so ExportService can treat it
    // like any other strategy — it has no idea legacy code is running underneath.
    // : IExportStrategy means this class promises to implement Export(Report, string).

    public class LegacyExportServiceAdapter : IExportStrategy
    {
        // Adaptee
        // The old ExportService whose interface is incompatible with IExportStrategy.
        // _adaptee: the Phase 1 LegacyExportService being wrapped.
        // private: callers cannot access the legacy service directly — all calls go through this adapter.
        // readonly: once assigned in the constructor, _adaptee can never be replaced.
        //           This guarantees the adapter always wraps the same legacy object.
        private readonly LegacyExportService _adaptee;

        /// <summary>
        /// Constructs the adapter by creating the legacy service with the
        /// specified format. The format is fixed at construction time, which
        /// is exactly how the old API worked.
        /// </summary>
        
        // Constructor: takes the ExportFormat that LegacyExportService requires at creation time.
        // This is the incompatibility — IExportStrategy has no constructor parameter,
        // but LegacyExportService needs to know the format upfront.
        // The adapter hides this requirement from callers by accepting it here and passing it to the adaptee.
        public LegacyExportServiceAdapter(ExportFormat format)
        {

            // Create and store the legacy service with the chosen format.
            // From this point on, _adaptee is the European charger — ready to work, wrong plug shape.
            _adaptee = new LegacyExportService(format);
        }

        // Target interface implementation

        /// <summary>
        /// Satisfies <see cref="IExportStrategy.Export"/> by delegating to
        /// the wrapped legacy service. The client calls this method through
        /// the IExportStrategy reference and never knows LegacyExportService exists.
        /// </summary>

        // Export() is the Target interface method — the NZ socket shape.
        // ExportService calls this method through the IExportStrategy reference.
        // It never knows that _adaptee (the legacy service) runs underneath.
        // This is the translation step: Target.Export() --> Adaptee.Export()
        // Both have the same signature here, but in a real adapter this is where
        // parameter mapping, data transformation, or protocol conversion would occur.

        public void Export(Report report, string outputPath)
        {
            // Delegation: translate the Target call into an Adaptee call.
            // Both methods have the same signature here, but in a real adapter
            // this is where parameter mapping, data transformation, or
            // protocol conversion would occur.
            // DELEGATION: forward the call to the adaptee using its own Export() method.
            // The adapter does no work itself — it purely translates and passes through.
            // This single line is the entire purpose of the Object Adapter pattern.
            _adaptee.Export(report, outputPath);
        }
    }


    // LegacyExportService — represents the Phase 1 ExportService (the Adaptee).
    // In your real project this is the existing ExportService.cs in the zip.
    // It is reproduced here in simplified form to make the adapter self-contained
    // and compilable without modifying the original file.


    /// <summary>
    /// Phase 1 export service. Format is fixed at construction time.
    /// This class is the Adaptee — its interface is incompatible with
    /// <see cref="IExportStrategy"/> because it requires format injection
    /// via the constructor rather than being format-agnostic.
    /// </summary>

    // LegacyExportService is the Adaptee — it cannot implement IExportStrategy directly
    // because its interface is incompatible (format required at construction, not via strategy injection).
    public class LegacyExportService
    {
        // _format: the export format is fixed at construction time.
        // This is the incompatibility — IExportStrategy has no constructor parameter;
        // the strategy IS the format. LegacyExportService requires it upfront.
        private readonly ExportFormat _format;

        // Construction-time format — this is the incompatibility.
        // IExportStrategy has no constructor parameter; the strategy IS the format.
        // Constructor stores the format chosen at creation time.
        // Expression body (=>) shorthand for { _format = format; }

        public LegacyExportService(ExportFormat format) => _format = format;
        
        // Export() is the Adaptee's method. Same signature as IExportStrategy.Export()
        // but this class was not designed with IExportStrategy in mind.
        // It reads _format (set at construction) and dispatches to the matching private

        public void Export(Report report, string outputPath)
        {
            // Delegates to the same format-specific logic as Phase 1.
            // In your actual project this body is already implemented in ExportService.cs.
            // switch on the format stored at construction time — Phase 1 approach.
            // This is the design weakness Strategy replaces: one class, all three format methods.
            switch (_format)
            {
                case ExportFormat.CSV:
                    ExportToCsv(report, outputPath);    // write comma-separated values
                    break;
                case ExportFormat.PDF:
                    ExportToPdf(report, outputPath);    // write formatted plain-text PDF
                    break;
                case ExportFormat.JSON:
                    ExportToJson(report, outputPath);   // write JSON object tree
                    break;
                    // No default: unrecognised formats are silently ignored (another Phase 1 weakness)
            }
        }

        // Format methods are stubs here — full implementations exist in ExportService.cs
        // These are stub implementations — full versions exist in ExportService.cs.
        // In the real codebase these methods contain the complete CSV/PDF/JSON logic.
        // private: callers cannot invoke format methods directly — they must call Export().
        private static void ExportToCsv(Report r, string p)  => File.WriteAllText(p, $"CSV:{r.Title}");
        private static void ExportToPdf(Report r, string p)  => File.WriteAllText(p, $"PDF:{r.Title}");
        private static void ExportToJson(Report r, string p) => File.WriteAllText(p, $"JSON:{r.Title}");
    }
}

# Microtransaction Analytics System (MAS)
### ENSE706 — Data Process and Software Modelling | Auckland University of Technology | 2026 S1

**Student:** Suemon Kwok | **Student ID:** 14883335

---

> **Academic Disclaimer:** This is an academic project completed as part of ENSE706 at Auckland University of Technology. The organisation referenced — Grinding Gear Games (GGG) — is a real New Zealand-based company. The student has no affiliation with Grinding Gear Games whatsoever. This company was selected solely because it met the project's selection criteria: a real NZ/AU-based organisation with 200+ employees, at least five distinct departments, and at least one business-critical information system. All system requirements, UML models, and implementations are entirely the student's own academic work and do not represent any actual system or product of Grinding Gear Games.

---

## About This Project

The **Microtransaction Analytics System (MAS)** is a software engineering academic project that analyses, models, and implements a business-critical analytics platform for tracking and reporting on cosmetic microtransaction data — modelled around the context of [Grinding Gear Games](https://www.grindinggear.com/), the Auckland-based developer behind *Path of Exile*.

GGG's entire revenue model is built on voluntary cosmetic microtransactions — weapon skins, armour sets, pets, portal effects, hideout decorations, and stash tab expansions. The problem this system addresses is the absence of a centralised analytics platform: purchase records are currently fragmented across transactional databases and game server logs, with no unified reporting or aggregation layer. This forces business units (Marketing, Finance, Game Development, and Data Analytics) to rely on manual data extraction and ad-hoc spreadsheet compilation to answer fundamental commercial questions.

The MAS consolidates this data into a single, role-aware internal analytics platform.

---

## What the System Does

- **Transaction Tracking** — Records every microtransaction event with item type, price, timestamp, character class, and player region
- **Sales Reporting** — Generates filterable reports by date range, region, item category, and character class
- **Revenue Trend Visualisation** — Displays daily, weekly, and monthly revenue aggregations
- **Underperformance Flagging** — Automatically flags items whose sales fall below configurable thresholds
- **Player Segmentation** — Classifies players by spending tier (Casual / Regular / High-Value) and purchase history
- **Multi-format Export** — Exports reports in CSV, PDF, or JSON
- **Role-Based Access Control** — Enforces four distinct user roles: Data Analyst, Marketing Manager, Finance Officer, and Game Developer

---

## Project Structure

This project was completed across two phases:

### Phase 1 — Progress Report (Assignment 1)
Covers organisational analysis, requirements engineering, UML modelling, and initial C# implementation.

- **Task 1** — Organisational and System Analysis (GGG overview, departmental structure, stakeholders, system scope)
- **Task 2** — Requirements Engineering (14 functional requirements, 6 non-functional requirements, stakeholder traceability matrix)
- **Task 3** — UML Requirements Modelling (Use Case Diagram + 5 fully dressed use case descriptions)
- **Task 4** — Initial Structural Design (UML Class Diagram with 27 classes, C# implementation, OOP principles, anticipated design weaknesses)

### Phase 2 — Final Report (Assignment 2)
Covers design refinement through GoF design patterns, comparative analysis, and critical reflection.

- **Task 5** — Refined UML Models (Updated class diagram, 3 sequence diagrams, 1 activity diagram, 1 state machine diagram)
- **Task 6** — Design Pattern Application (5 GoF patterns: Strategy, Factory Method, Singleton, Adapter, Decorator)
- **Task 7** — Comparative Design Analysis (Before vs after metrics, cohesion, coupling, extensibility)
- **Task 8** — Critical Reflection (Trade-offs, limitations, lessons learned)

---

## Design Patterns Applied (Phase 2)

| Pattern | Type | Problem Addressed |
|---|---|---|
| **Strategy** | Behavioural | `ExportService` had all format logic in one switch block (418 lines → 86 lines after refactor) |
| **Factory Method** | Creational | 20 scattered `new()` item calls in `SeedData` tightly coupled to all 7 concrete subclasses |
| **Singleton** | Creational | `AuthService` could be instantiated multiple times, risking split-brain session state |
| **Adapter** | Structural | Phase 1's `LegacyExportService` was incompatible with the new `IExportStrategy` interface |
| **Decorator** | Structural | Audit logging needed for UC12 without modifying `ReportEngine` (SRP) |

---

## Technical Stack

- **Language:** C# (.NET)
- **UI Framework:** Windows Forms (WinForms)
- **Modelling:** UML (Class, Sequence, Activity, State Machine, Use Case diagrams)
- **Architecture:** Object-oriented, layered (UI → Service → Repository → Domain)
- **Version Control:** Git / GitHub

---

## System Requirements

- Windows OS (WinForms dependency)
- .NET SDK (see project file for version)
- Visual Studio or compatible C# IDE

## How to Run

1. Clone the repository:
   ```
   git clone https://github.com/Suemon-Kwok/ENSE706-Data-Process-and-Software-Modelling-2026-S1---Microtransaction-Analytics-System---14883335.git
   ```
2. Open the solution file (`.sln`) in Visual Studio
3. Build the solution (`Ctrl+Shift+B`)
4. Run the project (`F5`)
5. Log in using one of the seeded demo accounts (see `SeedData.cs` for credentials)

---

## Key Requirements Summary

### Functional
- FR01–FR05: Sales capture and transaction tracking
- FR06–FR08: Player demographics and segmentation
- FR09–FR12: Reporting, analytics, and export
- FR13–FR14: Role-based access and security

### Non-Functional
- **NFR01** — Transactions processed within 2 seconds
- **NFR02** — 99.9% uptime during league launch windows
- **NFR03** — Compliance with the NZ Privacy Act 2020
- **NFR04** — Supports 50+ concurrent internal users
- **NFR05** — Dashboard renders within 3 seconds for up to 1,000,000 records
- **NFR06** — Operable without formal training

---

## Repository Contents

```
/
├── MAS/                   # C# source code
│   ├── Models/            # Domain classes (MTXItem, Transaction, Player, etc.)
│   ├── Services/          # Business logic and GoF pattern implementations
│   ├── Repositories/      # Data access layer
│   ├── Forms/             # WinForms UI
│   └── Program.cs         # Entry point
├── Docs/                  # Reports and UML diagrams
└── README.md
```

---

## References

- Gamma, E., Helm, R., Johnson, R., & Vlissides, J. (1994). *Design Patterns: Elements of Reusable Object-Oriented Software*. Addison-Wesley.
- Grinding Gear Games. (2024). *Path of Exile – Official site*. https://www.pathofexile.com
- Grinding Gear Games. (2026). *Grinding Gear Games*. https://www.grindinggear.com/
- Martin, R. C. (2003). *Agile Software Development, Principles, Patterns, and Practices*. Prentice Hall.
- New Zealand Companies Office. (2006). *Grinding Gear Games Limited*. https://app.companiesoffice.govt.nz
- Siddique, A. (2026). *Design Patterns* [Lecture slides, ENSE706]. Auckland University of Technology.
- Sommerville, I. (2016). *Software Engineering* (10th ed.). Pearson Education.

---

*Auckland University of Technology — ENSE706 Data Process and Software Modelling — Semester 1, 2026*

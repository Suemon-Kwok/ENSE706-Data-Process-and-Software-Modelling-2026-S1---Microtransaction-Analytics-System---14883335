// Name: Suemon Kwok
// Student ID: 14883335
// PatternUsageDemo.cs — Demonstrates all GoF patterns (Task 6)
//
// NOT production code — shows the marker each pattern in one place.
// Call PatternUsageDemo.Run() from Program.cs if desired, or just read it.

using GGG_MAS.Models;
using GGG_MAS.Services;

namespace GGG_MAS
{
    internal static class PatternUsageDemo
    {
        internal static void Run()
        {
            Console.WriteLine("=== GoF Pattern Demonstration ===\n");

            // ── PATTERN 3: Singleton ──────────────────────────────────────
            Console.WriteLine("--- Pattern 3: Singleton (AuthService) ---");

            AuthService auth1 = AuthService.GetInstance();
            AuthService auth2 = AuthService.GetInstance();

            Console.WriteLine($"Same instance? {ReferenceEquals(auth1, auth2)}");  // True

            bool ok = auth1.Login("analyst", "analyst123");
            Console.WriteLine($"Login succeeded: {ok}");
            Console.WriteLine($"Current user:    {auth1.CurrentUser?.Username}");
            Console.WriteLine($"Is logged in:    {auth1.IsLoggedIn}");
            Console.WriteLine($"Can export:      {auth1.CanExport()}");
            Console.WriteLine();

            // ── PATTERN 2: Factory Method ─────────────────────────────────
            Console.WriteLine("--- Pattern 2: Factory Method (MTXItemFactory) ---");

            var factory = new MTXItemFactory();

            // Caller only knows MTXItem — never references WeaponSkin directly
            MTXItem weapon = factory.Create(
                ItemType.WeaponSkin, "WPN_001", "Infernal Blade Skin",
                24.99f, new DateTime(2026, 1, 10));

            MTXItem pet = factory.Create(
                ItemType.Pet, "PET_001", "Faerie Dragon",
                19.99f, new DateTime(2026, 2, 1));

            // Bundle uses overload with correct discountPct parameter
            Bundle bundle = factory.CreateBundle(
                "BND_001", "Starter Cosmetic Bundle",
                34.99f, new DateTime(2026, 3, 1),
                0.20f,
                new[] { weapon, pet });

            Console.WriteLine($"Created: {weapon.GetDescription()}  price=${weapon.GetPrice()}");
            Console.WriteLine($"Created: {pet.GetDescription()}     price=${pet.GetPrice()}");
            Console.WriteLine($"Created: {bundle.GetDescription()}");
            Console.WriteLine();

            // ── PATTERN 1: Strategy ───────────────────────────────────────
            Console.WriteLine("--- Pattern 1: Strategy (ExportService + IExportStrategy) ---");

            var report = BuildSampleReport();
            string tempDir = Path.GetTempPath();

            // Phase 2 usage: inject strategy via interface
            var exportService = new ExportService();

            exportService.SetStrategy(ExportFormat.CSV);
            exportService.Export(report, Path.Combine(tempDir, "mas_report.csv"));
            Console.WriteLine($"CSV exported  → {Path.Combine(tempDir, "mas_report.csv")}");

            exportService.SetStrategy(ExportFormat.PDF);
            exportService.Export(report, Path.Combine(tempDir, "mas_report.txt"));
            Console.WriteLine($"PDF exported  → {Path.Combine(tempDir, "mas_report.txt")}");

            exportService.SetStrategy(ExportFormat.JSON);
            exportService.Export(report, Path.Combine(tempDir, "mas_report.json"));
            Console.WriteLine($"JSON exported → {Path.Combine(tempDir, "mas_report.json")}");

            // Direct interface injection — proves Open/Closed satisfied
            IExportStrategy custom = new CsvExportStrategy();
            exportService.SetStrategy(custom);
            Console.WriteLine("Strategy injected via IExportStrategy — OCP satisfied.");

            Console.WriteLine("\n=== Demo complete ===");
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private static Report BuildSampleReport()
        {
            var report = new Report
            {
                Title            = "Demo Report",
                GeneratedAt      = DateTime.Now,
                TotalRevenue     = 1672.18f,
                TotalTransactions = 82,
                BundleSplit      = (28, 54),
            };

            // TopByCategory uses Dictionary<ItemType, (string ItemName, int Count)>
            report.TopByCategory[ItemType.WeaponSkin]        = ("Shadow Dagger Skin", 12);
            report.TopByCategory[ItemType.Pet]               = ("Faerie Dragon",      30);
            report.TopByCategory[ItemType.PortalEffect]      = ("Infernal Portal",    14);
            report.TopByCategory[ItemType.HideoutDecoration] = ("Arcane Orb",          7);

            // TopByClass uses Dictionary<CharacterClass, (string ItemName, int Count)>
            report.TopByClass[CharacterClass.Templar] = ("Starter Cosmetic Bundle", 9);
            report.TopByClass[CharacterClass.Ranger]  = ("Starter Cosmetic Bundle", 9);
            report.TopByClass[CharacterClass.Shadow]  = ("Void Golem",              7);

            // RevenueTrend uses Dictionary<string, float>
            report.RevenueTrend["2026-03-19"] = 94.96f;
            report.RevenueTrend["2026-03-20"] =  4.99f;
            report.RevenueTrend["2026-03-21"] = 111.86f;

            // UnderperformingItems uses List<(string ItemName, ItemType Type, int Sales)>
            report.UnderperformingItems.Add(("Regal Throne",       ItemType.HideoutDecoration, 1));
            report.UnderperformingItems.Add(("Ice Portal",          ItemType.PortalEffect,      2));
            report.UnderperformingItems.Add(("Shadow Dagger Skin",  ItemType.WeaponSkin,        3));

            return report;
        }
    }
}

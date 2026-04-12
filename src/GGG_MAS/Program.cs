// =============================================================
// Program.cs — Application entry point
// Bootstraps all services, seeds demo data, and launches
// the login screen. Composes the dependency graph manually
// (no IoC container needed for this scope).
// =============================================================

using GGG_MAS.Forms;
using GGG_MAS.Models;
using GGG_MAS.Repositories;
using GGG_MAS.Services;

namespace GGG_MAS
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Enable Windows visual styles for native-looking controls
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Bootstrap repositories (in-memory stores) ─────
            var txRepo   = new InMemoryTransactionRepository();   // stores transactions
            var itemRepo = new InMemoryItemRepository();          // stores MTX catalogue

            // ── Seed catalogue items ───────────────────────────
            // All item types from the UML diagram are represented (FR02)
            var catalogue = SeedData.CreateCatalogue();
            foreach (var item in catalogue)
                itemRepo.Add(item);

            // ── Seed demo players ──────────────────────────────
            var players = SeedData.CreatePlayers();

            // ── Seed 250 sample transactions (FR01) ───────────
            SeedData.SeedTransactions(txRepo, itemRepo, players);

            // ── Compose services ──────────────────────────────
            var txService  = new TransactionService(txRepo, itemRepo);
            var reportEngine = new ReportEngine(underperformThreshold: 15f);

            // FR13: create four role-based system users
            var systemUsers = SeedData.CreateSystemUsers();
            var auth        = new AuthService(systemUsers);

            // ── Show login screen first ────────────────────────
            bool loggedIn = false;

            while (!loggedIn)
            {
                // Show login dialog; if user closes it without logging in, exit
                using var loginForm = new LoginForm(auth);
                if (loginForm.ShowDialog() != DialogResult.OK)
                    return;   // user closed the window — exit the application

                loggedIn = auth.IsLoggedIn;
            }

            // ── Launch main dashboard ──────────────────────────
            // All services are injected — dependencies flow downward only
            Application.Run(new MainDashboardForm(
                auth, reportEngine, txRepo, itemRepo, txService, players));

            // If the user logs out and the form closes, restart the loop
            // (not needed in this scope — single session per run)
        }
    }
}

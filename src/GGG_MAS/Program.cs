// Name: Suemon Kwok
// Student ID: 14883335
// Program.cs — Application entry point
//
// Phase 2 change: AuthService is now a Singleton.
// Instead of new AuthService(systemUsers), all code calls AuthService.GetInstance().
// SeedData still creates the catalogue, players, and transactions as before.
// The Singleton's private constructor seeds its own user list internally,
// so CreateSystemUsers() is no longer passed to AuthService.

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Repositories ──────────────────────────────────────────────
            var txRepo   = new InMemoryTransactionRepository();
            var itemRepo = new InMemoryItemRepository();

            // ── Seed catalogue items ──────────────────────────────────────
            var catalogue = SeedData.CreateCatalogue();
            foreach (var item in catalogue)
                itemRepo.Add(item);

            // ── Seed demo players ─────────────────────────────────────────
            var players = SeedData.CreatePlayers();

            // ── Seed 250 sample transactions ──────────────────────────────
            SeedData.SeedTransactions(txRepo, itemRepo, players);

            // ── Compose services ──────────────────────────────────────────
            var txService    = new TransactionService(txRepo, itemRepo);
            var reportEngine = new ReportEngine(underperformThreshold: 15f);

            // Phase 2: obtain AuthService via Singleton — no constructor call.
            // The Singleton seeds its own user list (analyst, marketing,
            // developer, finance) identically to SeedData.CreateSystemUsers().
            var auth = AuthService.GetInstance();

            // ── Login loop ────────────────────────────────────────────────
            bool loggedIn = false;
            while (!loggedIn)
            {
                using var loginForm = new LoginForm(auth);
                if (loginForm.ShowDialog() != DialogResult.OK)
                    return;

                loggedIn = auth.IsLoggedIn;
            }

            // ── Launch dashboard ──────────────────────────────────────────
            Application.Run(new MainDashboardForm(
                auth, reportEngine, txRepo, itemRepo, txService, players));
        }
    }
}

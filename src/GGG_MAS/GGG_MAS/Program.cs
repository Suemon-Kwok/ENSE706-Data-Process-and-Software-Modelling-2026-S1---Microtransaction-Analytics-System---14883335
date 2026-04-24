//Name : Suemon Kwok 

// Student ID : 14883335

// Program.cs — Application entry point

// Bootstraps all services, seeds demo data, and launches

// the login screen. Composes the dependency graph manually

// (no IoC container needed beyond this project scope.

// What does this file do
// the starting gun. It runs first when the app launches.
// It creates everything (repositories, services, fake data, users), shows the login screen, and once you're logged in, opens the dashboard.
// Think of it as the builder that assembles all the parts before handing control to the user.

using GGG_MAS.Forms;                                                                        // imports the LoginForm and MainDashboardForm UI classes

using GGG_MAS.Models;                                                                       // imports model classes like PlayerAccount, MTXItem, etc.

using GGG_MAS.Repositories;                                                                 // imports the in-memory repository implementations

using GGG_MAS.Services;                                                                     // imports AuthService, ReportEngine, TransactionService, SeedData

namespace GGG_MAS                                                                           // declares the root namespace for the whole application
{
    internal static class Program // static class means it cannot be instantiated : entry point only
    {
        [STAThread]                                                                         // required for Windows Forms: marks the main thread as Single-Threaded Apartment (COM interop)

        static void Main()                                                                  // the application's entry point : runs first when the .exe is launched
        {
            // Enable Windows visual styles for native-looking controls
            Application.EnableVisualStyles(); // makes controls like buttons use the current Windows theme

            Application.SetCompatibleTextRenderingDefault(false);                           // use GDI+ text rendering (better quality than GDI)

            //Bootstrap repositories (in-memory stores)
            var txRepo   = new InMemoryTransactionRepository();                             // stores transactions: creates the in-memory store that holds all transactions

            var itemRepo = new InMemoryItemRepository();                                    // stores MTX catalogue: creates the in-memory store that holds all MTX catalogue items

            //Seed catalogue items

            // All item types from the UML diagram are represented (FR02)
            var catalogue = SeedData.CreateCatalogue();                                     // generates the list of WeaponSkins, Pets, Bundles, etc.
            foreach (var item in catalogue)                                                 // loops over every generated item
                itemRepo.Add(item);                                                         // stores each item in the in-memory item repository

            //Seed demo players
            var players = SeedData.CreatePlayers();                                         // creates 10 sample PlayerAccount objects with different regions and tiers

            //Seed 250 sample transactions (FR01)
            SeedData.SeedTransactions(txRepo, itemRepo, players);                           // generates 250 random transactions spread across the last 90 days

            //Compose services
            var txService  = new TransactionService(txRepo, itemRepo);                      // creates the service that records new purchases

            var reportEngine = new ReportEngine(underperformThreshold: 15f);                // creates the analytics engine; items with <15 sales are flagged

            // FR13: create four role-based system users
            var systemUsers = SeedData.CreateSystemUsers();                                 // creates analyst, marketing, developer, finance accounts

            var auth        = new AuthService(systemUsers);                                 // creates the authentication service and loads the user list

            //Show login screen first
            bool loggedIn = false;                                                          // flag that tracks whether authentication has succeeded

            while (!loggedIn)                                                               // keep showing the login form until the user successfully signs in
            {
                // Show login dialog; if user closes it without logging in, exit
                using var loginForm = new LoginForm(auth);                                  // creates the login dialog, injecting the auth service
                if (loginForm.ShowDialog() != DialogResult.OK)                              // shows the form as a modal dialog; waits for the user to close it
                    return;                                                                 // user closed the window — exit the application

                loggedIn = auth.IsLoggedIn;                                                 // check the auth service to see if the login succeeded
            }

            //Launch main dashboard
           
            // All services are injected — dependencies flow downward only
            Application.Run(new MainDashboardForm(                                          // starts the Windows Forms message loop with the dashboard as the main form
                auth, reportEngine, txRepo, itemRepo, txService, players));                 // injects all services and data into the dashboard

            // If the user logs out and the form closes, restart the loop

        }
    }
}

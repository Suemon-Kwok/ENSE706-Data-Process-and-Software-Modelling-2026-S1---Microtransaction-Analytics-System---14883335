// Name : Suemon Kwok

// Student ID : 14883335

// AuthService.cs — Role-based authentication service

// FR13, FR14: user login with role-controlled access.

// NFR03: passwords are never logged or stored in plain text.

// What does this file do
// handles login/logout. Searches the user list for a matching username, hashes the password attempt and compares it.
// Sets CurrentUser on success. Also contains SeedData (inside the same file) —
// which is the factory that creates the demo catalogue, players, system users, and 250 sample transactions at startup

// OOP Concepts
// Encapsulation (no raw credential exposure) and Single Responsibility (Auth only handles login/logout;
// SeedData only creates demo data)

// 


namespace GGG_MAS.Services                                                                                                                              // brings in SystemUser, UserRole, etc.
{
    using GGG_MAS.Models;

    /// <summary>
   
    /// Manages user session. Only one user can be logged in at a time.
    
    /// </summary>
    public class AuthService                                                                                                                            // holds all known staff accounts, passed in at startup
    {
        // Pre-loaded list of valid system users (demo data)
        private readonly List<SystemUser> _users;

        // Currently authenticated user; null means not logged in
        public SystemUser? CurrentUser { get; private set; }                                                                                            // null when no session is active; set on successful login

        public AuthService(IEnumerable<SystemUser> users)
        {
            _users = users.ToList();                                                                                                                    // materialises the enumerable into a List so it can be searched multiple times
        }

        // Returns true if a user is currently authenticated
        // NFR02: Services are stateless — support 99.9% uptime, no single point of failure
        public bool IsLoggedIn => CurrentUser != null;                                                                                                  // expression property; returns true when CurrentUser has been set

        /// <summary>

        /// Attempts login. Returns true on success.

        /// Failed attempts are silently rejected (no error detail — security).

        /// </summary>
        public bool Login(string username, string password)
        {
            // Find user by username (case-insensitive for usability)
            var user = _users.FirstOrDefault(u =>                                                                               
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));                                                                       // searches list for matching username, ignoring case

            // Authenticate the password hash — returns false if user not found
            if (user != null && user.Authenticate(password))                                                                                            // null-check first to avoid calling Authenticate on null
            {
                CurrentUser = user;                                                                                                                     // stores the authenticated user as the active session
                return true;                                                                                                                            // signals success to the caller (Program.cs)
            }
            return false;                                                                                                                               // never reveal whether user exists or password is wrong
        }

        // Ends the current session
        public void Logout() => CurrentUser = null;                                                                                                     // clears the session; the login loop in Program.cs will then show the login form
    }
}


// SeedData.cs : Generates realistic demo data for the MAS

// Pre-loads catalogue items, players, and sample transactions

// so the application is immediately usable after launch.

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;                                                                                                                               // brings in MTXItem subtypes, PlayerAccount, SystemUser, etc.
    using GGG_MAS.Repositories;                                                                                                                         // brings in ITransactionRepository and IItemRepository

    /// <summary>

    /// Seeds the in-memory repositories with demo data.

    /// Run once at application startup.

    /// </summary>
    public static class SeedData                                                                                                                        // static class — all methods are utility helpers; no instance is needed
    {
        // Random instance with fixed seed for reproducible demo data
        private static readonly Random Rng = new Random(42);                                                                                            // seed 42 ensures the same 250 transactions are generated every run

        // MTX item catalogue
        // Pre-built set covering all ItemType categories (FR02)
        public static List<MTXItem> CreateCatalogue()
        {
            var items = new List<MTXItem>
            {
                // Weapon Skins
                new WeaponSkin("WPN_001","Infernal Sword Skin",  24.99f, new DateTime(2023,1,15), "Sword",  3),
                
                new WeaponSkin("WPN_002","Void Bow Skin",        19.99f, new DateTime(2023,3,20), "Bow",    2),
                
                new WeaponSkin("WPN_003","Crystal Wand Skin",    14.99f, new DateTime(2023,6,10), "Wand",   2),
                
                new WeaponSkin("WPN_004","Shadow Dagger Skin",   12.99f, new DateTime(2024,1,5),  "Dagger", 1),

                // Armour Skins
                new ArmourSkin("ARM_001","Celestial Helm",       18.99f, new DateTime(2023,2,1),  ArmourSlot.Helm,   "CelestialIdle"),
                
                new ArmourSkin("ARM_002","Demon Chest Plate",    29.99f, new DateTime(2023,4,15), ArmourSlot.Chest,  "DemonRoar"),
                
                new ArmourSkin("ARM_003","Storm Gloves",         11.99f, new DateTime(2023,7,20), ArmourSlot.Gloves, "StormPulse"),
                
                new ArmourSkin("ARM_004","Frost Boots",           9.99f, new DateTime(2024,2,10), ArmourSlot.Boots,  "FrostStep"),

                // Pets
                new PetItem("PET_001","Celestial Cat",          14.99f, new DateTime(2023,1,1), "StarKitten", "SitAndPurr"),
                
                new PetItem("PET_002","Void Golem",             19.99f, new DateTime(2023,5,5), "Golem",      "Stomp"),
                
                new PetItem("PET_003","Faerie Dragon",          24.99f, new DateTime(2024,3,1), "Faerie",     "Flutter"),

                // Portal Effects
                new PortalEffect("PRT_001","Infernal Portal",   16.99f, new DateTime(2023,1,20), "FireVortex",  "LavaCrackle"),
                
                new PortalEffect("PRT_002","Celestial Portal",  19.99f, new DateTime(2023,6,1),  "StarField",   "CosmicHum"),
                
                new PortalEffect("PRT_003","Ice Portal",        12.99f, new DateTime(2024,1,15), "FrostCrystal","IceCrack"),

                // Hideout Decorations
                new HideoutDecoration("HID_001","Regal Throne",  9.99f, new DateTime(2023,3,10), "Furniture", 1),
                
                new HideoutDecoration("HID_002","Arcane Orb",    7.99f, new DateTime(2023,8,5),  "Magic",     2),

                // Stash Tabs
                new StashTabExpansion("STH_001","Currency Tab",  14.99f, new DateTime(2022,6,1), "Currency", 1),
                
                new StashTabExpansion("STH_002","Map Tab",       12.99f, new DateTime(2022,9,1), "Map",      1),
                
                new StashTabExpansion("STH_003","Premium Tab",    4.99f, new DateTime(2022,1,1), "Premium",  1),

                // Bundle
                new Bundle("BND_001","Starter Cosmetic Bundle", 34.99f, new DateTime(2023,1,1), 0.20f)
            };
            return items;                                                                                                                           // returns the full catalogue list to be loaded into the item repository
        }

        //Sample players
        public static List<PlayerAccount> CreatePlayers()
        {
            return new List<PlayerAccount>
            {
                new PlayerAccount("P001","ExileKing",    "NZ",  730, SpendingTier.HighValue, false),                                              // NZ whale, 2-year-old account
                
                new PlayerAccount("P002","ShadowArrow",  "AU",  365, SpendingTier.Regular,   false),                                              // AU regular, 1-year account
                
                new PlayerAccount("P003","WitchQueen",   "NZ",  180, SpendingTier.Casual,    true),                                               // NZ first-time buyer
                
                new PlayerAccount("P004","IronMarauder", "US",  900, SpendingTier.HighValue, false),                                              // US whale, oldest account
                
                new PlayerAccount("P005","TempestScion", "EU",   90, SpendingTier.Casual,    true),                                               // EU first-time buyer, new account
                
                new PlayerAccount("P006","ArcaneWitch",  "NZ",  500, SpendingTier.Regular,   false),                                              // NZ regular, experienced player
                
                new PlayerAccount("P007","DuelMaster",   "AU",  200, SpendingTier.Regular,   false),                                              // AU regular spender
                
                new PlayerAccount("P008","ShadowStep",   "US",  120, SpendingTier.Casual,    false),                                              // US casual, not a first-time buyer
                
                new PlayerAccount("P009","PortalMage",   "EU",  600, SpendingTier.HighValue, false),                                              // EU whale, experienced player
                
                new PlayerAccount("P010","NewExile",     "NZ",    5, SpendingTier.Casual,    true),                                               // NZ brand new first-time buyer
            };
        }

        // System users (internal staff)
        // FR13: four roles with different access levels
        public static List<SystemUser> CreateSystemUsers()
        {
            return new List<SystemUser>
            {
                new SystemUser("U001", "analyst",   "analyst123",  UserRole.Analyst),                                                              // can view reports and record purchases
                
                new SystemUser("U002", "marketing", "mktg123",     UserRole.MarketingManager),                                                     // can view reports and export data
                
                new SystemUser("U003", "developer", "dev123",      UserRole.Developer),                                                            // full access including configuration
                
                new SystemUser("U004", "finance",   "finance123",  UserRole.FinanceOfficer),                                                       // can view reports and export data
            };
        }

        // Sample transactions
        // Generates 250 realistic transactions spanning the last 90 days (FR01)
        public static void SeedTransactions(ITransactionRepository txRepo,
                                            IItemRepository        itemRepo,
                                            List<PlayerAccount>    players)
        {
            var catalogue   = itemRepo.GetAll().ToList();                                                                                           // gets all items from the repository as a list
            var classes     = Enum.GetValues<CharacterClass>();                                                                                     // gets an array of all CharacterClass enum values

            // Weight distribution: popular items get more purchases
            // Weapon skins and pets are the most popular (matches BR-01 scenario)
            var weights = new Dictionary<string, int>
            {
                ["WPN_001"]=28,["WPN_002"]=20,["WPN_003"]=12,["WPN_004"]=6,                                                                         // weapon skin popularity weights

                ["ARM_001"]=18,["ARM_002"]=25,["ARM_003"]=9, ["ARM_004"]=5,                                                                         // armour skin popularity weights

                ["PET_001"]=22,["PET_002"]=15,["PET_003"]=30,                                                                                       // pet popularity weights — Faerie Dragon most popular

                ["PRT_001"]=14,["PRT_002"]=16,["PRT_003"]=4,                                                                                        // portal effect popularity weights

                ["HID_001"]=3, ["HID_002"]=2,                                                                                                       // hideout decorations are rarely purchased

                ["STH_001"]=20,["STH_002"]=12,["STH_003"]=8,                                                                                        // stash tab popularity weights

                ["BND_001"]= 18                                                                                                                     // bundle popularity weight
            };

            // Build a weighted pool of item IDs
            var pool = weights.SelectMany(kv =>
                Enumerable.Repeat(kv.Key, kv.Value)).ToList();                                                                                      // flattens dictionary into a list with repetitions matching the weights

            // Generate 250 transactions spread over the last 90 days
            for (int i = 0; i < 250; i++)                                                                                                           // loop runs exactly 250 times to create 250 transactions
            {
                var item    = itemRepo.FindById(pool[Rng.Next(pool.Count)]);                                                                        // picks a random item from the weighted pool
                if (item == null) continue;

                var player  = players[Rng.Next(players.Count)];                                                                                     // picks a random player from the player list

                var cls     = classes[Rng.Next(classes.Length)];                                                                                    // picks a random character class

                bool bundle = item is Bundle || Rng.NextDouble() < 0.15;                                                                            // always true for Bundle items; 15% chance otherwise

                // Spread timestamps across last 90 days for meaningful trend data
                var timestamp = DateTime.Now.AddDays(-Rng.Next(0, 90))                                                                              // random day in the last 90 days
                                             .AddHours(-Rng.Next(0, 24))                                                                            // random hour of that day
                                             .AddMinutes(-Rng.Next(0, 60));                                                                         // random minute of that hour

                var tx = new Transaction(
                    txId:      Guid.NewGuid().ToString("N")[..12].ToUpper(),                                                                        // generates a GUID, takes first 12 chars, uppercases it

                    timestamp: timestamp,                                                                                                           // the randomly spread timestamp

                    price:     item.GetPrice(),                                                                                                     // snapshot the price from the item at this moment

                    bundleFlag:bundle,                                                                                                              // whether this is treated as a bundle purchase

                    charClass: cls,                                                                                                                 // the randomly chosen character class

                    item:      item,                                                                                                                // the item being purchased

                    player: player                                                                                                                  // the player making the purchase
                );

                if (tx.Validate())                                                                                                                  // only persist transactions that pass all validation checks
                {
                    item.IncrementSales();                                                                                                          // increments the item's internal sales counter

                    player.RecordPurchase(item.ItemId);                                                                                             // adds the item to the player's purchase history

                    txRepo.Add(tx);                                                                                                                 // saves the transaction to the in-memory repository
                }
            }
        }
    }
}

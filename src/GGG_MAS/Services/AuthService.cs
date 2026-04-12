// =============================================================
// AuthService.cs — Role-based authentication service
// FR13, FR14: user login with role-controlled access.
// NFR03: passwords are never logged or stored in plain text.
// =============================================================

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;

    /// <summary>
    /// Manages user session. Only one user can be logged in at a time.
    /// </summary>
    public class AuthService
    {
        // Pre-loaded list of valid system users (demo data)
        private readonly List<SystemUser> _users;

        // Currently authenticated user; null means not logged in
        public SystemUser? CurrentUser { get; private set; }

        public AuthService(IEnumerable<SystemUser> users)
        {
            _users = users.ToList();
        }

        // Returns true if a user is currently authenticated
        public bool IsLoggedIn => CurrentUser != null;

        /// <summary>
        /// Attempts login. Returns true on success.
        /// Failed attempts are silently rejected (no error detail — security).
        /// </summary>
        public bool Login(string username, string password)
        {
            // Find user by username (case-insensitive for usability)
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            // Authenticate the password hash — returns false if user not found
            if (user != null && user.Authenticate(password))
            {
                CurrentUser = user;
                return true;
            }
            return false;  // never reveal whether user exists or password is wrong
        }

        // Ends the current session
        public void Logout() => CurrentUser = null;
    }
}

// =============================================================
// SeedData.cs — Generates realistic demo data for the MAS
// Pre-loads catalogue items, players, and sample transactions
// so the application is immediately usable after launch.
// =============================================================
namespace GGG_MAS.Services
{
    using GGG_MAS.Models;
    using GGG_MAS.Repositories;

    /// <summary>
    /// Seeds the in-memory repositories with demo data.
    /// Run once at application startup.
    /// </summary>
    public static class SeedData
    {
        // Random instance with fixed seed for reproducible demo data
        private static readonly Random Rng = new Random(42);

        // ── MTX item catalogue ────────────────────────────────
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
            return items;
        }

        // ── Sample players ────────────────────────────────────
        public static List<PlayerAccount> CreatePlayers()
        {
            return new List<PlayerAccount>
            {
                new PlayerAccount("P001","ExileKing",    "NZ",  730, SpendingTier.HighValue, false),
                new PlayerAccount("P002","ShadowArrow",  "AU",  365, SpendingTier.Regular,   false),
                new PlayerAccount("P003","WitchQueen",   "NZ",  180, SpendingTier.Casual,    true),
                new PlayerAccount("P004","IronMarauder", "US",  900, SpendingTier.HighValue, false),
                new PlayerAccount("P005","TempestScion", "EU",   90, SpendingTier.Casual,    true),
                new PlayerAccount("P006","ArcaneWitch",  "NZ",  500, SpendingTier.Regular,   false),
                new PlayerAccount("P007","DuelMaster",   "AU",  200, SpendingTier.Regular,   false),
                new PlayerAccount("P008","ShadowStep",   "US",  120, SpendingTier.Casual,    false),
                new PlayerAccount("P009","PortalMage",   "EU",  600, SpendingTier.HighValue, false),
                new PlayerAccount("P010","NewExile",     "NZ",    5, SpendingTier.Casual,    true),
            };
        }

        // ── System users (internal staff) ─────────────────────
        // FR13: four roles with different access levels
        public static List<SystemUser> CreateSystemUsers()
        {
            return new List<SystemUser>
            {
                new SystemUser("U001", "analyst",   "analyst123",  UserRole.Analyst),
                new SystemUser("U002", "marketing", "mktg123",     UserRole.MarketingManager),
                new SystemUser("U003", "developer", "dev123",      UserRole.Developer),
                new SystemUser("U004", "finance",   "finance123",  UserRole.FinanceOfficer),
            };
        }

        // ── Sample transactions ────────────────────────────────
        // Generates 250 realistic transactions spanning the last 90 days (FR01)
        public static void SeedTransactions(ITransactionRepository txRepo,
                                            IItemRepository        itemRepo,
                                            List<PlayerAccount>    players)
        {
            var catalogue   = itemRepo.GetAll().ToList();
            var classes     = Enum.GetValues<CharacterClass>();

            // Weight distribution: popular items get more purchases
            // Weapon skins and pets are the most popular (matches BR-01 scenario)
            var weights = new Dictionary<string, int>
            {
                ["WPN_001"]=28,["WPN_002"]=20,["WPN_003"]=12,["WPN_004"]=6,
                ["ARM_001"]=18,["ARM_002"]=25,["ARM_003"]=9, ["ARM_004"]=5,
                ["PET_001"]=22,["PET_002"]=15,["PET_003"]=30,
                ["PRT_001"]=14,["PRT_002"]=16,["PRT_003"]=4,
                ["HID_001"]=3, ["HID_002"]=2,
                ["STH_001"]=20,["STH_002"]=12,["STH_003"]=8,
                ["BND_001"]=18
            };

            // Build a weighted pool of item IDs
            var pool = weights.SelectMany(kv =>
                Enumerable.Repeat(kv.Key, kv.Value)).ToList();

            // Generate 250 transactions spread over the last 90 days
            for (int i = 0; i < 250; i++)
            {
                var item    = itemRepo.FindById(pool[Rng.Next(pool.Count)]);
                if (item == null) continue;

                var player  = players[Rng.Next(players.Count)];
                var cls     = classes[Rng.Next(classes.Length)];
                bool bundle = item is Bundle || Rng.NextDouble() < 0.15;

                // Spread timestamps across last 90 days for meaningful trend data
                var timestamp = DateTime.Now.AddDays(-Rng.Next(0, 90))
                                             .AddHours(-Rng.Next(0, 24))
                                             .AddMinutes(-Rng.Next(0, 60));

                var tx = new Transaction(
                    txId:      Guid.NewGuid().ToString("N")[..12].ToUpper(),
                    timestamp: timestamp,
                    price:     item.GetPrice(),
                    bundleFlag:bundle,
                    charClass: cls,
                    item:      item,
                    player:    player
                );

                if (tx.Validate())
                {
                    item.IncrementSales();
                    player.RecordPurchase(item.ItemId);
                    txRepo.Add(tx);
                }
            }
        }
    }
}

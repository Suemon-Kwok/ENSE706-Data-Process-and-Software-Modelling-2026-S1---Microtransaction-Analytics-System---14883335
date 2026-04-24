// Name : Suemon Kwok

// Student ID : 14883335

// PlayerAccount.cs — PlayerAccount class (UML)

// Represents an external player's account for demographics.

// FR06, FR07, FR08 — region, tier, repeat buyer tracking.

// NFR03: Only non-personal identifiers stored (privacy).

// What does this file do
// represents a player. It stores their region, spending tier, and purchase history (as a private HashSet).
// It has RecordPurchase() (to log a purchase) and UpdateTier() (to recalculate Casual/Regular/HighValue based on total spend).
// No real names or emails are stored — privacy by design

// OOP Concepts
// Encapsulation. Purchase history is a private HashSet; only exposed as IReadOnlyCollection so nothing outside can add to it directly.
// UpdateTier() is the only way to change spending tier.

// Why OOP concepts were used
// Encapsulation — "Keep internals private" Encapsulation	Private data, public doors — control who can change what
// the purchase history is a private HashSet. Nobody outside can just add items to it directly —
// they have to go through RecordPurchase(). Same with _price and _salesCount in MTXItem — only GetPrice() and IncrementSales() can touch them.
// Why? It prevents bugs. If any part of the code could change the sales count directly, you'd have no guarantee it's correct.
// Encapsulation means only the right code can change the right data

namespace GGG_MAS.Models
{
    /// <summary>
    
    /// Holds anonymised player metadata used in demographic reports.
    
    /// Personal data minimised to comply with NZ Privacy Act 2020.
    
    /// </summary>
    
    public class PlayerAccount
    {
        // Opaque account identifier — no name or email stored
        public string AccountId { get; private set; }                                                                               // unique ID (e.g. "P001"); not linked to real identity

        // Display alias used in reports (not a real name)
        public string Username { get; private set; }                                                                                // in-game handle shown in the dashboard (e.g. "ExileKing")

        // Player's geographic region for regional sales segmentation (FR06)
        public string Region { get; private set; }                                                                                  // two-letter region code: "NZ", "AU", "US", "EU", "APAC"

        // Age of the account in days (not birth date — privacy)
        public int AccountAgeDays { get; private set; }                                                                             // how old the account is; avoids storing the actual date of birth

        // Computed spending tier based on total historical spend (FR07)
        public SpendingTier SpendingTier { get; private set; }                                                                      // Casual / Regular / HighValue; updated by UpdateTier()    

        // True if this is the player's first ever purchase (FR08)
        public bool IsFirstTimeBuyer { get; private set; }                                                                          // flips to false after the second purchase is recorded

        // List of item IDs this player has previously purchased
        private readonly HashSet<string> _purchasedItemIds;                                                                         // HashSet automatically deduplicates item IDs

        // Constructor
        public PlayerAccount(string accountId, string username,
                             string region, int accountAgeDays,
                             SpendingTier tier, bool firstTimeBuyer)
        {
            AccountId        = accountId;                                                                                           // sets the opaque account identifier

            Username         = username;                                                                                            // sets the display alias     

            Region           = region;                                                                                              // sets the geographic region code

            AccountAgeDays   = accountAgeDays;                                                                                      // sets account age in days

            SpendingTier     = tier;                                                                                                // sets the initial spending tier    

            IsFirstTimeBuyer = firstTimeBuyer;                                                                                      // sets whether this is a first-time buyer

            _purchasedItemIds = new HashSet<string>();                                                                              // initialises an empty set for purchase history
        }

        // Returns a read-only copy of the player's purchase history IDs
        
        public IReadOnlyCollection<string> GetPurchaseHistory() =>
            _purchasedItemIds;                                                                                                      // returns the set as read-only so callers cannot add or remove items

        // FR07: Returns the player's spending tier

        public SpendingTier GetTier() => SpendingTier;                                                                              // exposes the current tier value via the interface contract    

        // FR08: Returns true if the player has bought this item before

        public bool IsRepeatBuyer(ISalesItem item)
        {
            // Cast to MTXItem to access ItemId (safe — all items extend MTXItem)
            if (item is MTXItem mtx)                                                                                                // pattern-matches and casts item to MTXItem
                return _purchasedItemIds.Contains(mtx.ItemId);
            return false;                                                                                                           // returns false if the cast fails (should never happen in practice)
        }

        // Called by TransactionService after a successful purchase
        
        public void RecordPurchase(string itemId)
        {
            _purchasedItemIds.Add(itemId);                                                                                          // adds the item ID to the history set; HashSet ignores duplicates
            // Update first-time buyer flag after recording
            if (IsFirstTimeBuyer && _purchasedItemIds.Count > 1)                                                                    // if this is now a second purchase
                IsFirstTimeBuyer = false;                                                                                           // the player is no longer a first-time buyer
        }

        // Updates the spending tier when total spend threshold is crossed
        
        public void UpdateTier(float totalSpend)
        {
            SpendingTier = totalSpend switch                                                                                        // switch expression assigns the matching tier
            {
                > 200f => SpendingTier.HighValue,                                                                                   // more than $200 total = high value (whale)
                > 50f  => SpendingTier.Regular,                                                                                     // between $50 and $200 = regular spender
                _      => SpendingTier.Casual                                                                                       // under $50 = casual spender (default case)
            };
        }

        // Returns a readable summary used in dropdowns and reports

        public override string ToString() => $"{Username} ({Region}) [{SpendingTier}]";
    }
}

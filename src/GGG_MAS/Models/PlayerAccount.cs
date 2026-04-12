// =============================================================
// PlayerAccount.cs — PlayerAccount class (UML)
// Represents an external player's account for demographics.
// FR06, FR07, FR08 — region, tier, repeat buyer tracking.
// NFR03: Only non-personal identifiers stored (privacy).
// =============================================================

namespace GGG_MAS.Models
{
    /// <summary>
    /// Holds anonymised player metadata used in demographic reports.
    /// Personal data minimised to comply with NZ Privacy Act 2020.
    /// </summary>
    public class PlayerAccount
    {
        // Opaque account identifier — no name or email stored
        public string AccountId { get; private set; }

        // Display alias used in reports (not a real name)
        public string Username { get; private set; }

        // Player's geographic region for regional sales segmentation (FR06)
        public string Region { get; private set; }

        // Age of the account in days (not birth date — privacy)
        public int AccountAgeDays { get; private set; }

        // Computed spending tier based on total historical spend (FR07)
        public SpendingTier SpendingTier { get; private set; }

        // True if this is the player's first ever purchase (FR08)
        public bool IsFirstTimeBuyer { get; private set; }

        // List of item IDs this player has previously purchased
        private readonly HashSet<string> _purchasedItemIds;

        // Constructor
        public PlayerAccount(string accountId, string username,
                             string region, int accountAgeDays,
                             SpendingTier tier, bool firstTimeBuyer)
        {
            AccountId        = accountId;
            Username         = username;
            Region           = region;
            AccountAgeDays   = accountAgeDays;
            SpendingTier     = tier;
            IsFirstTimeBuyer = firstTimeBuyer;
            _purchasedItemIds = new HashSet<string>();
        }

        // Returns a read-only copy of the player's purchase history IDs
        public IReadOnlyCollection<string> GetPurchaseHistory() =>
            _purchasedItemIds;

        // FR07: Returns the player's spending tier
        public SpendingTier GetTier() => SpendingTier;

        // FR08: Returns true if the player has bought this item before
        public bool IsRepeatBuyer(ISalesItem item)
        {
            // Cast to MTXItem to access ItemId (safe — all items extend MTXItem)
            if (item is MTXItem mtx)
                return _purchasedItemIds.Contains(mtx.ItemId);
            return false;
        }

        // Called by TransactionService after a successful purchase
        public void RecordPurchase(string itemId)
        {
            _purchasedItemIds.Add(itemId);  // HashSet ignores duplicates
            // Update first-time buyer flag after recording
            if (IsFirstTimeBuyer && _purchasedItemIds.Count > 1)
                IsFirstTimeBuyer = false;
        }

        // Updates the spending tier when total spend threshold is crossed
        public void UpdateTier(float totalSpend)
        {
            SpendingTier = totalSpend switch
            {
                > 200f => SpendingTier.HighValue,
                > 50f  => SpendingTier.Regular,
                _      => SpendingTier.Casual
            };
        }

        public override string ToString() => $"{Username} ({Region}) [{SpendingTier}]";
    }
}

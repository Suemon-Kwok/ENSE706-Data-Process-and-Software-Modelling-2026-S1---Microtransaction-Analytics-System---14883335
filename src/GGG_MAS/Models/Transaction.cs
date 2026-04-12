// =============================================================
// Transaction.cs — Transaction class (UML)
// Records a single MTX purchase event.
// FR01: Records item, price, timestamp, player, character class.
// FR05: bundleFlag distinguishes bundle vs individual purchase.
// =============================================================

namespace GGG_MAS.Models
{
    /// <summary>
    /// Immutable record of a single microtransaction event.
    /// Created by TransactionService and stored in TransactionRepository.
    /// </summary>
    public class Transaction
    {
        // Unique transaction identifier (GUID-based)
        public string TxId { get; private set; }

        // Exact moment the purchase was confirmed (FR01)
        public DateTime Timestamp { get; private set; }

        // Amount paid in NZD at time of purchase
        public float Price { get; private set; }

        // True when item was purchased as part of a bundle (FR05)
        public bool BundleFlag { get; private set; }

        // The character class the player was using at purchase time (FR03)
        public CharacterClass CharacterClass { get; private set; }

        // The item purchased — stored as interface for LSP compliance
        private readonly ISalesItem _item;

        // The player who made the purchase
        private readonly PlayerAccount _player;

        // Demographic snapshot captured at purchase time (FR06, FR07)
        public DemographicRecord Demographics { get; private set; }

        public Transaction(string txId, DateTime timestamp, float price,
                           bool bundleFlag, CharacterClass charClass,
                           ISalesItem item, PlayerAccount player)
        {
            TxId           = txId;
            Timestamp      = timestamp;
            Price          = price;
            BundleFlag     = bundleFlag;
            CharacterClass = charClass;
            _item          = item;
            _player        = player;

            // Capture demographic snapshot at transaction time
            Demographics = new DemographicRecord(
                player.Region,
                timestamp.Date,
                !player.IsFirstTimeBuyer,   // repeat buyer if not first
                player.GetTier()
            );
        }

        // Returns the purchased item (returns interface — low coupling)
        public ISalesItem GetItem() => _item;

        // Returns the purchasing player account
        public PlayerAccount GetPlayer() => _player;

        // Validates that all required transaction fields are populated
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(TxId) &&
            Price > 0f &&
            _item != null &&
            _player != null;

        public override string ToString() =>
            $"[{TxId}] {Timestamp:yyyy-MM-dd HH:mm} | " +
            $"{_item?.GetItemType()} | ${Price:F2} | {_player?.Username}";
    }

    // =============================================================
    // DemographicRecord.cs — DemographicRecord class (UML)
    // Snapshot of player demographics at purchase time.
    // FR06, FR07: region and spending tier for segmentation.
    // =============================================================

    /// <summary>
    /// Immutable demographic snapshot captured when a transaction occurs.
    /// Decoupled from PlayerAccount for privacy and historical accuracy.
    /// </summary>
    public class DemographicRecord
    {
        // Geographic region at time of purchase (FR06)
        public string Region { get; private set; }

        // Date portion of the transaction (stripped of time for grouping)
        public DateTime PurchaseDate { get; private set; }

        // True if the player had purchased before this transaction (FR08)
        public bool IsRepeatBuyer { get; private set; }

        // Spending tier at the time of this purchase (FR07)
        public SpendingTier SpendingTier { get; private set; }

        public DemographicRecord(string region, DateTime purchaseDate,
                                 bool isRepeatBuyer, SpendingTier tier)
        {
            Region        = region;
            PurchaseDate  = purchaseDate;
            IsRepeatBuyer = isRepeatBuyer;
            SpendingTier  = tier;
        }

        // FR07: True when the player is in the highest spending tier
        public bool IsWhale() => SpendingTier == SpendingTier.HighValue;

        // Returns region identifier for regional segmentation (BR-06)
        public string GetRegion() => Region;
    }
}

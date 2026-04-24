// Name : Suemon Kwok

// Student ID : 14883335

// Transaction.cs — Transaction class (UML)

// Records a single MTX purchase event.

// FR01: Records item, price, timestamp, player, character class.

// FR05: bundleFlag distinguishes bundle vs individual purchase.

// What does this file do 
// a single purchase event, frozen in time. It records the item, player, price, timestamp, character class, and bundle flag.
// It also captures a DemographicRecord snapshot (region + spending tier at time of purchase), so even if the player's tier changes later,
// the historical record stays accurate.

// OOP Concept
// Encapsulation — "Keep internals private" Encapsulation	Private data, public doors — control who can change what
// Encapsulation and Abstraction. _item is stored as ISalesItem (the interface), not the concrete type — loose coupling.
// DemographicRecord is an immutable value object, decoupled from the mutable PlayerAccount.

namespace GGG_MAS.Models                                                                                                                // belongs to the shared model namespace
{
    /// <summary>
    
    /// Immutable record of a single microtransaction event.
    
    /// Created by TransactionService and stored in TransactionRepository.
    
    /// </summary>
    
    public class Transaction
    {
        // Unique transaction identifier (GUID-based)
        public string TxId { get; private set; }                                                                                        // 12-character uppercase ID derived from a GUID (e.g. "A3F7B2C1D009")

        // Exact moment the purchase was confirmed (FR01)
        public DateTime Timestamp { get; private set; }                                                                                 // date and time the transaction was created

        // Amount paid in NZD at time of purchase
        public float Price { get; private set; }                                                                                        // snapshot of the item's price at the time of purchase

        // True when item was purchased as part of a bundle (FR05)
        public bool BundleFlag { get; private set; }                                                                                    // used to split bundle vs individual counts in reports

        // The character class the player was using at purchase time (FR03)
        public CharacterClass CharacterClass { get; private set; }                                                                      // records which class was active when the purchase was made

        // The item purchased — stored as interface for LSP compliance
        private readonly ISalesItem _item;                                                                                              // private and read-only; stored as interface so any ISalesItem subtype is valid

        // The player who made the purchase
        private readonly PlayerAccount _player;                                                                                         // private reference to the PlayerAccount that made this purchase

        // Demographic snapshot captured at purchase time (FR06, FR07)
        public DemographicRecord Demographics { get; private set; }                                                                     // immutable snapshot of the player's region and tier at purchase time

        public Transaction(string txId, DateTime timestamp, float price,
                           bool bundleFlag, CharacterClass charClass,
                           ISalesItem item, PlayerAccount player)
        {
            TxId           = txId;                                                                                                      // stores the unique transaction ID

            Timestamp      = timestamp;                                                                                                 // stores the exact purchase time

            Price          = price;                                                                                                     // stores the price at time of purchase

            BundleFlag     = bundleFlag;                                                                                                // stores whether the purchase was part of a bundle

            CharacterClass = charClass;                                                                                                 // stores which character class the player used

            _item          = item;                                                                                                      // stores the purchased item (via interface reference)

            _player        = player;                                                                                                    // stores the player who made the purchase    

            // Capture demographic snapshot at transaction time
            Demographics = new DemographicRecord(
                
                player.Region,                                                                                                          // copies region from the player at time of purchase

                timestamp.Date,                                                                                                         // strips the time component so grouping by date is clean

                !player.IsFirstTimeBuyer,                                                                                               // if not a first-time buyer, this is a repeat purchase

                player.GetTier()                                                                                                        // snapshots the spending tier at the moment of purchase
            );
        }

        // Returns the purchased item (returns interface — low coupling)
        public ISalesItem GetItem() => _item;                                                                                           // callers get the item via ISalesItem, not the concrete type

        // Returns the purchasing player account
        public PlayerAccount GetPlayer() => _player;                                                                                    // exposes the player reference for spend calculations

        // Validates that all required transaction fields are populated
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(TxId) &&                                                                                         // TxId must not be empty    

            Price > 0f &&                                                                                                               // price must be a positive number

            _item != null &&                                                                                                            // an item must be associated

            _player != null;                                                                                                            // a player must be associated

        public override string ToString() =>
            $"[{TxId}] {Timestamp:yyyy-MM-dd HH:mm} | " +
            $"{_item?.GetItemType()} | ${Price:F2} | {_player?.Username}";                                                              // F2 = two decimal places
    }

    
    // DemographicRecord.cs — DemographicRecord class (UML)
    
    // Snapshot of player demographics at purchase time.
    
    // FR06, FR07: region and spending tier for segmentation.
    

    /// <summary>
    
    /// Immutable demographic snapshot captured when a transaction occurs.
    
    /// Decoupled from PlayerAccount for privacy and historical accuracy.
    
    /// </summary>
    
    public class DemographicRecord
    {
        // Geographic region at time of purchase (FR06)
        public string Region { get; private set; }                                                                                      // region code copied from PlayerAccount at purchase time    

        // Date portion of the transaction (stripped of time for grouping)
        public DateTime PurchaseDate { get; private set; }                                                                             // date-only; used to group transactions in trend charts     

        // True if the player had purchased before this transaction (FR08)
        public bool IsRepeatBuyer { get; private set; }                                                                                // true = this player has bought before; false = first-time buyer

        // Spending tier at the time of this purchase (FR07)
        public SpendingTier SpendingTier { get; private set; }                                                                         // tier snapshot; does not change if tier is updated later

        public DemographicRecord(string region, DateTime purchaseDate,
                                 bool isRepeatBuyer, SpendingTier tier)
        {
            Region        = region;                                                                                                   // stores the region code at purchase time

            PurchaseDate  = purchaseDate;                                                                                             // stores the date (time stripped) of the purchase     

            IsRepeatBuyer = isRepeatBuyer;                                                                                            // stores whether the player had previously purchased   

            SpendingTier  = tier;                                                                                                     // stores the spending tier at the time of purchase
        }

        // FR07: True when the player is in the highest spending tier
        public bool IsWhale() => SpendingTier == SpendingTier.HighValue;                                                             // convenience method for filtering high-value players

        // Returns region identifier for regional segmentation (BR-06)
        public string GetRegion() => Region;                                                                                         // used by ReportEngine to group revenue by region
    }
}

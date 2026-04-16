// Name : Suemon Kwok

// Student ID : 14883335

// MTXItem.cs — Abstract class: MTXItem (UML)

// Base for all MTX items. Implements ISalesItem.

// Demonstrates: Abstraction, Inheritance, High Cohesion.


namespace GGG_MAS.Models                                                                                            // belongs to the shared model namespace
{
    /// <summary>
    
    /// Abstract base for every purchasable MTX item.
    
    /// Concrete subclasses add item-specific behaviour.
    
    /// </summary>
    public abstract class MTXItem : ISalesItem                                                                      // abstract class — cannot be instantiated directly; must be subclassed
    {
        // Unique item identifier (e.g. "WPN_001")
        public string ItemId { get; private set; }                                                                  // read-only outside the class; set only in the constructor

        // Display name shown in the shop
        public string Name { get; private set; }                                                                    // human-readable item name used in UI and reports

        // Category bucket used for reporting filters
        public ItemType ItemType { get; private set; }                                                              // stores the ItemType enum value (e.g. WeaponSkin, Pet)

        // Price in NZD
        private float _price;                                                                                       // backing field kept private so subclasses cannot accidentally change the price

        // Date the item was added to the shop
        public DateTime ReleaseDate { get; private set; }                                                           // used for sorting and trend analysis

        // Running total of units sold across all transactions
        private int _salesCount;                                                                                    // private so only IncrementSales() can modify it : prevents accidental manipulation

        // Constructor — all fields required to prevent invalid state
        protected MTXItem(string itemId, string name, ItemType itemType,
                          float price, DateTime releaseDate)                                                        // protected so only subclasses can call it
        {
            ItemId      = itemId;                                                                                   // assigns the unique item ID

            Name        = name;                                                                                     // assigns the display name

            ItemType    = itemType;                                                                                 // assigns the category enum value

            _price      = price;                                                                                    // sets the price backing field

            ReleaseDate = releaseDate;                                                                              // records when the item went on sale

            _salesCount = 0;                                                                                        // every new item starts with zero sales
        }

        // ISalesItem: return current price
        public float GetPrice() => _price;                                                                          // expression-bodied method; returns the private price field

        // ISalesItem: return item category
        public ItemType GetItemType() => ItemType;                                                                  // satisfies the ISalesItem interface contract

        // ISalesItem: return total units sold
        public int GetSalesCount() => _salesCount;                                                                  // returns the private counter; callers cannot modify it directly     

        // Called by TransactionService when a purchase is confirmed
        public void IncrementSales() => _salesCount++;                                                              // adds 1 to the sales counter each time the item is bought    

        // Override to provide item-specific description for UI display
        public abstract string GetDescription();                                                                    // forces every subclass to provide its own description string


        // Allow readable name in dropdowns / reports
        public override string ToString() => $"{Name} [{ItemType}]";                                                // returns "Infernal Sword Skin [WeaponSkin]" style string
    }
}

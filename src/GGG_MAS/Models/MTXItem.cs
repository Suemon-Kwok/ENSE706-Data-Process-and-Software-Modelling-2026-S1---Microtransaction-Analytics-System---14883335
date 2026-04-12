// =============================================================
// MTXItem.cs — Abstract class: MTXItem (UML)
// Base for all MTX items. Implements ISalesItem.
// Demonstrates: Abstraction, Inheritance, High Cohesion.
// =============================================================

namespace GGG_MAS.Models
{
    /// <summary>
    /// Abstract base for every purchasable MTX item.
    /// Concrete subclasses add item-specific behaviour.
    /// </summary>
    public abstract class MTXItem : ISalesItem
    {
        // Unique item identifier (e.g. "WPN_001")
        public string ItemId { get; private set; }

        // Display name shown in the shop
        public string Name { get; private set; }

        // Category bucket used for reporting filters
        public ItemType ItemType { get; private set; }

        // Price in NZD
        private float _price;

        // Date the item was added to the shop
        public DateTime ReleaseDate { get; private set; }

        // Running total of units sold across all transactions
        private int _salesCount;

        // Constructor — all fields required to prevent invalid state
        protected MTXItem(string itemId, string name, ItemType itemType,
                          float price, DateTime releaseDate)
        {
            ItemId      = itemId;
            Name        = name;
            ItemType    = itemType;
            _price      = price;
            ReleaseDate = releaseDate;
            _salesCount = 0;
        }

        // ISalesItem: return current price
        public float GetPrice() => _price;

        // ISalesItem: return item category
        public ItemType GetItemType() => ItemType;

        // ISalesItem: return total units sold
        public int GetSalesCount() => _salesCount;

        // Called by TransactionService when a purchase is confirmed
        public void IncrementSales() => _salesCount++;

        // Override to provide item-specific description for UI display
        public abstract string GetDescription();

        // Allow readable name in dropdowns / reports
        public override string ToString() => $"{Name} [{ItemType}]";
    }
}

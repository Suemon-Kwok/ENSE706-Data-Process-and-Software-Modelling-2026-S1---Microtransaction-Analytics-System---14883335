// =============================================================
// ISalesItem.cs — Interface: SalesItem (UML)
// Abstraction layer for all purchasable MTX items.
// Supports SOLID: Interface Segregation + Dependency Inversion.
// =============================================================

namespace GGG_MAS.Models
{
    /// <summary>
    /// Contract every MTX item must satisfy.
    /// Provides price, type, and sales count queries.
    /// </summary>
    public interface ISalesItem
    {
        // Returns the item's price in NZD
        float GetPrice();

        // Returns the item category (weapon, pet, etc.)
        ItemType GetItemType();

        // Returns total units sold across all transactions
        int GetSalesCount();
    }
}

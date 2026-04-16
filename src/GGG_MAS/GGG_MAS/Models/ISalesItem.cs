// Name : Suemon Kwok

// Student ID : 14883335

// ISalesItem.cs — Interface: SalesItem (UML)

// Abstraction layer for all purchasable MTX items.

// Supports SOLID: Interface Segregation + Dependency Inversion.


namespace GGG_MAS.Models                                                                        // belongs to the shared model namespace
{
    
    /// <summary>
    
    /// Contract every MTX item must satisfy.
    
    /// Provides price, type, and sales count queries.
    
    /// </summary>
    
    public interface ISalesItem                                                                // defines the minimum contract any purchasable item must fulfil
    {
        // Returns the item's price in NZD
        float GetPrice();                                                                      // callers retrieve the price without knowing the concrete class       

        // Returns the item category (weapon, pet, etc.)
        ItemType GetItemType();                                                                // returns which ItemType enum value this item belongs to

        // Returns total units sold across all transactions
        int GetSalesCount();                                                                   // returns how many times this item has been purchased in total
    }
}

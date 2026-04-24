// Name : Suemon Kwok

// Student ID : 14883335

// ISalesItem.cs — Interface: SalesItem (UML)

// Abstraction layer for all purchasable MTX items.

// Supports SOLID: Interface Segregation + Dependency Inversion.

// What does this file do
// a contract (interface). It says: "any item that can be sold must have GetPrice(), GetItemType(), and GetSalesCount()."
// It's a promise, not actual code. Everything downstream (transactions, reports) works through this promise rather
// than caring whether they're dealing with a WeaponSkin or a Bundle.


// OOP Concepts used
// Interface and Dependency Inversion. Defines a contract that every purchasable item must follow.
// Transaction and ReportEngine depend on this interface, not on concrete item classes.

// Why OOP concepts were used
// Interfaces — "A promise, not an implementation" Interface	A contract — "I promise I can do X, Y, Z"
// ISalesItem is a contract: "I promise I have GetPrice(), GetItemType(), and GetSalesCount()."
// IReportGenerator promises certain report methods. ITransactionRepository promises storage methods.
// Why? The TransactionService doesn't care how items are stored — it just knows the repository can store them.
// This means you could swap the in-memory list for a real SQL database and TransactionService wouldn't need to change a single line.

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

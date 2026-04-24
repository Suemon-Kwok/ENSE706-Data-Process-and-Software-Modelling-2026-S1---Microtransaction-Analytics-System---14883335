// Name : Suemon Kwok

// Student ID : 14883335


// TransactionService.cs — TransactionService

// Records purchases, updates player state, delegates to repos.

// FR01–FR03: records item, timestamp, player, character class.

// What does this file do
// records new purchases from the UI. Looks up the item, creates a Transaction object,
// calls IncrementSales() on the item, calls RecordPurchase() on the player, recalculates the player's spending tier, then saves to the repository.
// Also contains the two in-memory repositories (InMemoryTransactionRepository and InMemoryItemRepository)

// OOP concepts
// Dependency Inversion and Low Coupling. Depends on ITransactionRepository and IItemRepository interfaces,
// not concrete implementations — you could swap in a SQL database without touching this file.

// Why OOP concepts were used
// Dependency Inversion — "Depend on promises, not specifics" Dependency Inversion	Depend on the promise, not the specific thing
// TransactionService takes ITransactionRepository and IItemRepository in its constructor —
// not the concrete InMemoryTransactionRepository class. It works with the interface, not the implementation.
// Why? It's the same reason above — flexibility and testability. In a real job, you'd swap the in-memory store for a real database.
// With dependency inversion, you just plug in a different object that honours the same promise.

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;                                                                                           // brings in Transaction, PlayerAccount, CharacterClass, ISalesItem

    using GGG_MAS.Repositories;                                                                                     // brings in ITransactionRepository and IItemRepository

    /// <summary>

    /// Orchestrates the creation and storage of Transaction records.

    /// Called when a new purchase is submitted via the UI.

    /// </summary>
    public class TransactionService
    {
        private readonly ITransactionRepository _txRepo;                                                            // stores transactions; injected so it can be swapped for a real DB

        private readonly IItemRepository        _itemRepo;                                                          // looks up catalogue items by ID; injected for the same reason

        public TransactionService(ITransactionRepository txRepo,
                                  IItemRepository itemRepo)
        {
            _txRepo   = txRepo;                                                                                     // stores the transaction repository reference
            _itemRepo = itemRepo;                                                                                   // stores the item repository reference
        }

        /// <summary>
        
        /// Records a new MTX purchase.
        
        /// Validates, creates the Transaction, updates the player account,
        
        /// and persists to the repository.
        
        /// </summary>
        public Transaction RecordPurchase(string itemId, PlayerAccount player,
                                          CharacterClass charClass, bool isBundle)
        {
            // Look up the item in the catalogue
            var item = _itemRepo.FindById(itemId)
                       ?? throw new ArgumentException($"Item '{itemId}' not found.");

            // Build the immutable Transaction record (FR01)
            var tx = new Transaction(                                                                                          
                
                txId:          Guid.NewGuid().ToString("N")[..12].ToUpper(),                                       // generates a unique 12-char uppercase ID from a GUID  

                timestamp:     DateTime.Now,                                                                       // captures the exact moment the purchase is made

                price:         item.GetPrice(),                                                                    // snapshots the item price at purchase time 
                    
                bundleFlag:    isBundle,                                                                           // FR05: flags whether this was a bundle purchase    

                charClass:     charClass,                                                                          // FR03: records which character class was active 
                
                item:          item,                                                                               // the purchased item

                player: player                                                                                     // the player making the purchase     
            );

            // Validate before persisting
            if (!tx.Validate())
                throw new InvalidOperationException("Transaction failed validation.");

            // Increment the item's internal sales counter
            item.IncrementSales();                                                                                  // updates the item's _salesCount so GetSalesCount() reflects this purchase

            // Update the player's purchase history and tier (FR07, FR08)
            player.RecordPurchase(itemId);                                                                          // adds this item to the player's purchase history set

            player.UpdateTier(CalculateTotalSpend(player));                                                         // recalculates the player's spending tier based on all their spend

            // Persist to in-memory repository
            _txRepo.Add(tx);                                                                                        // saves the completed transaction to the repository
            return tx;                                                                                              // returns the new Transaction so the UI can display a confirmation     
        }

        // Calculates the player's total historical spend from all transactions
        private float CalculateTotalSpend(PlayerAccount player)
        {
            return _txRepo.GetAll()
                          .Where(t => t.GetPlayer().AccountId == player.AccountId)                                  // filters to only this player's transactions
                          .Sum(t => t.Price);                                                                       // sums all their transaction prices to get total lifetime spend
        }
    }
}


// Repositories.cs — Repository interfaces and implementations

// Low coupling: services depend only on interfaces.

// Reusability: swap in-memory store for a real DB easily.

namespace GGG_MAS.Repositories
{
    using GGG_MAS.Models;                                                                                           // brings in Transaction, MTXItem, DateRange

    // ITransactionRepository

    /// <summary>

    /// Contract for transaction storage. Swap for EF/SQL easily.

    /// </summary>
    public interface ITransactionRepository
    {
        void Add(Transaction tx);                                                                                   // adds a single transaction to the store

        IReadOnlyList<Transaction> GetAll();                                                                        // returns every stored transaction as read-only

        IReadOnlyList<Transaction> GetByDateRange(DateRange range);                                                 // returns only transactions within the given date range
    }

    // IItemRepository

    /// <summary>
    
    /// Contract for MTX catalogue storage.
    
    /// </summary>
    
    public interface IItemRepository
    {
        void Add(MTXItem item);                                                                                     // adds a single item to the catalogue
        MTXItem? FindById(string itemId);                                                                           // returns the item with the given ID, or null if not found
        IReadOnlyList<MTXItem> GetAll();                                                                            // returns the entire catalogue as read-only
    }

    // In-memory implementations

    /// <summary>
   
    /// Thread-safe in-memory transaction store using lock for NFR04.
    
    /// </summary>
    
    public class InMemoryTransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _store = new();                                                          // backing list that holds all transactions in memory
        
        private readonly object            _lock  = new();                                                          // lock object used to synchronise concurrent access (NFR04)
        
        // Add a transaction; lock ensures thread safety
       
        public void Add(Transaction tx)                                                                             
        {
            lock (_lock) { _store.Add(tx); }                                                                        // acquires the lock before adding so two threads cannot write simultaneously
        }

        // Returns an immutable snapshot of all transactions
        public IReadOnlyList<Transaction> GetAll()
        {
            lock (_lock) { return _store.AsReadOnly(); }                                                            // AsReadOnly() wraps the list so callers cannot add or remove items
        }

        // Filters by date range without materialising unnecessary records
        public IReadOnlyList<Transaction> GetByDateRange(DateRange range)
        {
            lock (_lock)
            {
                return _store.Where(t => range.Contains(t.Timestamp))                                               // keeps only transactions within the date range
                             .ToList()                                                                              // materialises the filtered result
                             .AsReadOnly();                                                                         // returns as read-only so callers cannot modify it
            }
        }
    }

    /// <summary>
    
    /// In-memory MTX item catalogue keyed by ItemId for O(1) lookup.
    
    /// </summary>
    public class InMemoryItemRepository : IItemRepository
    {
        private readonly Dictionary<string, MTXItem> _store = new();                                                // Dictionary gives O(1) item lookup by ItemId

        public void Add(MTXItem item) => _store[item.ItemId] = item;                                                // uses ItemId as the key; overwrites if the same ID is added twice

        // Returns null if not found; callers handle missing items
        public MTXItem? FindById(string itemId) =>
            _store.TryGetValue(itemId, out var item) ? item : null;
        
        // TryGetValue avoids a KeyNotFoundException; returns null instead when the ID does not exist
        
        public IReadOnlyList<MTXItem> GetAll() =>
            _store.Values.ToList().AsReadOnly();                                                                    // copies the dictionary values to a list and wraps as read-only
    }
}

// =============================================================
// TransactionService.cs — TransactionService
// Records purchases, updates player state, delegates to repos.
// FR01–FR03: records item, timestamp, player, character class.
// SOLID: Single Responsibility — purchase workflow only.
// Low Coupling: depends on interfaces/abstractions.
// =============================================================

namespace GGG_MAS.Services
{
    using GGG_MAS.Models;
    using GGG_MAS.Repositories;

    /// <summary>
    /// Orchestrates the creation and storage of Transaction records.
    /// Called when a new purchase is submitted via the UI.
    /// </summary>
    public class TransactionService
    {
        private readonly ITransactionRepository _txRepo;       // stores transactions
        private readonly IItemRepository        _itemRepo;     // looks up catalogue items

        public TransactionService(ITransactionRepository txRepo,
                                  IItemRepository itemRepo)
        {
            _txRepo   = txRepo;
            _itemRepo = itemRepo;
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
                txId:          Guid.NewGuid().ToString("N")[..12].ToUpper(),
                timestamp:     DateTime.Now,   // exact moment of purchase
                price:         item.GetPrice(),
                bundleFlag:    isBundle,        // FR05
                charClass:     charClass,       // FR03
                item:          item,
                player:        player
            );

            // Validate before persisting
            if (!tx.Validate())
                throw new InvalidOperationException("Transaction failed validation.");

            // Increment the item's internal sales counter
            item.IncrementSales();

            // Update the player's purchase history and tier (FR07, FR08)
            player.RecordPurchase(itemId);
            player.UpdateTier(CalculateTotalSpend(player));

            // Persist to in-memory repository
            _txRepo.Add(tx);
            return tx;
        }

        // Calculates the player's total historical spend from all transactions
        private float CalculateTotalSpend(PlayerAccount player)
        {
            return _txRepo.GetAll()
                          .Where(t => t.GetPlayer().AccountId == player.AccountId)
                          .Sum(t => t.Price);
        }
    }
}

// =============================================================
// Repositories.cs — Repository interfaces and implementations
// Low coupling: services depend only on interfaces.
// Reusability: swap in-memory store for a real DB easily.
// =============================================================
namespace GGG_MAS.Repositories
{
    using GGG_MAS.Models;

    // ── ITransactionRepository ────────────────────────────────
    /// <summary>
    /// Contract for transaction storage. Swap for EF/SQL easily.
    /// </summary>
    public interface ITransactionRepository
    {
        void Add(Transaction tx);
        IReadOnlyList<Transaction> GetAll();
        IReadOnlyList<Transaction> GetByDateRange(DateRange range);
    }

    // ── IItemRepository ───────────────────────────────────────
    /// <summary>
    /// Contract for MTX catalogue storage.
    /// </summary>
    public interface IItemRepository
    {
        void Add(MTXItem item);
        MTXItem? FindById(string itemId);
        IReadOnlyList<MTXItem> GetAll();
    }

    // ── In-memory implementations ─────────────────────────────

    /// <summary>
    /// Thread-safe in-memory transaction store using lock for NFR04.
    /// </summary>
    public class InMemoryTransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _store = new();  // backing list
        private readonly object            _lock  = new();  // concurrent access lock (NFR04)

        // Add a transaction; lock ensures thread safety
        public void Add(Transaction tx)
        {
            lock (_lock) { _store.Add(tx); }
        }

        // Returns an immutable snapshot of all transactions
        public IReadOnlyList<Transaction> GetAll()
        {
            lock (_lock) { return _store.AsReadOnly(); }
        }

        // Filters by date range without materialising unnecessary records
        public IReadOnlyList<Transaction> GetByDateRange(DateRange range)
        {
            lock (_lock)
            {
                return _store.Where(t => range.Contains(t.Timestamp))
                             .ToList()
                             .AsReadOnly();
            }
        }
    }

    /// <summary>
    /// In-memory MTX item catalogue keyed by ItemId for O(1) lookup.
    /// </summary>
    public class InMemoryItemRepository : IItemRepository
    {
        private readonly Dictionary<string, MTXItem> _store = new();

        public void Add(MTXItem item) => _store[item.ItemId] = item;

        // Returns null if not found; callers handle missing items
        public MTXItem? FindById(string itemId) =>
            _store.TryGetValue(itemId, out var item) ? item : null;

        public IReadOnlyList<MTXItem> GetAll() =>
            _store.Values.ToList().AsReadOnly();
    }
}

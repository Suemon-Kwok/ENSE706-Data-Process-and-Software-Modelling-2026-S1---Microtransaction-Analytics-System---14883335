// Name : Suemon Kwok

// Student ID : 14883335

// MTXItem.cs — Abstract class: MTXItem (UML)

// Base for all MTX items. Implements ISalesItem.

// Demonstrates: Abstraction, Inheritance, High Cohesion.

// What does this file do
// the template for every item in the shop. It's abstract, meaning you can never create a plain "MTXItem" — you must use a specific subclass.
// It holds shared fields like Name, Price, SalesCount, and the IncrementSales() method.
// It also forces every subclass to implement GetDescription().

// OOP Concents
// Abstraction and Encapsulation. abstract class cannot be created directly. _price and _salesCount are private —
// only readable via GetPrice() and GetSalesCount(), only modifiable via IncrementSales()

// Why OOP concepts were used
// Abstraction — "Show only what's needed" Abstraction	A skeleton that forces subclasses to fill in the details
// MTXItem is an abstract class — you can never create a plain MTXItem.
// You must use a specific type like WeaponSkin or PetItem.
// It provides the shared skeleton (price, sales count, name) but forces subclasses to fill in GetDescription() themselves.
// Why? There's no such thing as a generic "item" in real life — it's always a specific thing.
// Abstraction enforces that while sharing common code.

// Inheritance — "Don't repeat yourself" Inheritance	Subclasses get the parent's code for free
// WeaponSkin, ArmourSkin, PetItem, Bundle etc. all inherit from MTXItem.
// They automatically get all the shared fields (name, price, sales counter) without rewriting them.
// Each just adds its own unique stuff — WeaponSkin adds WeaponClass and EffectTier.
// Why? Without inheritance, you'd copy-paste the same price/sales logic into 7 classes.
// Change one thing and you'd have to update all 7. Inheritance means you change it once.

/*
Abstract class abstraction: MTXItem is abstract — it cannot be instantiated directly. 
It provides shared code (price, sales counter, name) but forces every subclass to implement GetDescription(). 
You can never write new MTXItem() — you must write new WeaponSkin()
*/

/*
Encapsulation — what it is and how it applies here
Encapsulation means keeping internal state private and controlling access through public methods.
In MTXItem: private float _price and private int _salesCount cannot be changed from outside. GetPrice() provides read-only access. IncrementSales() is the only way to change _salesCount — it adds 1 and nothing else. No external code can arbitrarily set sales to 0 or subtract from it.
In SystemUser: the password is never stored as plaintext — only as a SHA-256 hash in a private field. Authenticate() is the only way to verify credentials.

Why is _price private and not public?' Answer: if _price were public, any code anywhere could set it to 0 or a negative number. Private + controlled method = invariant protection. 
 */

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

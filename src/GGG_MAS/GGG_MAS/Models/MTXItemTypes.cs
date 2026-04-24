// Name : Suemon Kwok

// Student ID : 14883335

// MTXItemTypes.cs — Concrete MTXItem subclasses (UML)

// WeaponSkin, ArmourSkin, PetItem, PortalEffect,

// HideoutDecoration, StashTabExpansion, Bundle

// What does this file do
// the seven specific item types: WeaponSkin, ArmourSkin, PetItem, PortalEffect, HideoutDecoration, StashTabExpansion, and Bundle.
// Each inherits from MTXItem and adds its own unique fields (e.g. WeaponSkin adds WeaponClass and EffectTier).
// Bundle also contains a private list of other MTXItem objects

// OOP Concepts
// Inheritance and Polymorphism. All seven classes inherit from MTXItem.
// Each overrides GetDescription() differently.
// Bundle also demonstrates Composition — it contains a List<MTXItem>.

// Why OOP concepts were used
// Composition	An object contains other objects (Bundle holding items)
// Composition (inside Bundle)
// The Bundle class contains a List<MTXItem> — a bundle is made up of other items.
// This is composition: "has-a" rather than "is-a".
// Why? A bundle isn't a special kind of item that magically includes others through inheritance —
// it literally holds other items. Composition models that naturally.
// Line 162 shows how Bundle has a private List<MTXItem> _items.
// The AddItem() method allows adding items to the bundle, and GetItems() returns a copy of that list.
// This way, the Bundle class manages its own collection of items without exposing the internal list directly.

namespace GGG_MAS.Models                                                                                                                       // all concrete item types live in the same model namespace as MTXItem
{
    // Weapon skin: applies a visual effect to a weapon
    public class WeaponSkin : MTXItem                                                                                                          // inherits all base fields and ISalesItem behaviour from MTXItem
    {
        public string WeaponClass { get; private set; }                                                                                        // e.g. "Sword", "Bow" — which weapon type this skin targets

        public int    EffectTier  { get; private set; }                                                                                        // 1 = basic, 3 = legendary — visual complexity rating


        public WeaponSkin(string id, string name, float price,
                          DateTime released, string weaponClass, int effectTier)
            : base(id, name, ItemType.WeaponSkin, price, released)                                                                             // calls MTXItem constructor; hard-codes ItemType.WeaponSkin
        {
            WeaponClass = weaponClass;                                                                                                         // stores which weapon type the skin is for 

            EffectTier  = effectTier;                                                                                                          // stores the visual tier level
        }

        // Applies the skin texture to the specified weapon slot
        public void ApplyToWeapon() { /* visual engine hook */ }                                                                               // placeholder for a real game engine call

        public override string GetDescription() =>
            $"Weapon Skin | Class: {WeaponClass} | Tier: {EffectTier}";
    }

    // Armour skin: applies visual set to an armour slot
    public class ArmourSkin : MTXItem                                                                                                           // inherits from MTXItem; represents a cosmetic armour reskin
    {
        public ArmourSlot SlotType { get; private set; }                                                                                        // which armour slot (helm, chest, etc.) this skin targets

        public string     AnimSet  { get; private set; }                                                                                        // the name of the animation bundle bundled with this skin

        public ArmourSkin(string id, string name, float price,
                          DateTime released, ArmourSlot slot, string animSet)
            : base(id, name, ItemType.ArmourSkin, price, released)                                                                              // calls MTXItem constructor with ArmourSkin type
        {
            SlotType = slot;                                                                                                                    // stores the targeted armour slot

            AnimSet  = animSet;                                                                                                                 // stores the animation bundle name
        }

        // Applies the skin to the specified armour slot
        public void ApplyToArmour() { /* visual engine hook */ }                                                                                // placeholder for game engine integration

        public override string GetDescription() =>
            $"Armour Skin | Slot: {SlotType} | Anim: {AnimSet}";
    }

    // ── Pet item: cosmetic companion that follows the player ─
    public class PetItem : MTXItem                                                                                                              // inherits from MTXItem; represents a cosmetic pet companion
    {
        public string PetName  { get; private set; }                                                                                            // the in-world creature name (e.g. "StarKitten")

        public string IdleAnim { get; private set; }                                                                                            // the animation key played when the pet is idle

        public PetItem(string id, string name, float price,
                       DateTime released, string petName, string idleAnim)
            : base(id, name, ItemType.Pet, price, released)                                                                                     // calls MTXItem constructor with Pet type
        {
            PetName  = petName;                                                                                                                 // stores the creature's display name

            IdleAnim = idleAnim;                                                                                                                // stores the idle animation key
        }

        // Activates the pet to trail the player
        public void Follow() { /* AI follow hook */ }                                                                                           // placeholder for the pet AI pathfinding trigger

        public override string GetDescription() =>
            $"Pet | Name: {PetName} | Idle: {IdleAnim}";
    }

    // ── Portal effect: visual/audio skin for the town portal ─
    public class PortalEffect : MTXItem                                                                                                         // inherits from MTXItem; represents a cosmetic portal skin
    {
        public string EffectStyle { get; private set; }                                                                                         // the shader style key (e.g. "FireVortex")

        public string SoundPack   { get; private set; }                                                                                         // the audio bundle name for the portal soun

        public PortalEffect(string id, string name, float price,
                            DateTime released, string style, string sound)
            : base(id, name, ItemType.PortalEffect, price, released)                                                                            // calls MTXItem with PortalEffect type
        {
            EffectStyle = style;                                                                                                                // stores the visual shader style

            SoundPack   = sound;                                                                                                                // stores the audio pack name
        }

        // Applies effect & sound to the player's portal
        public void ApplyToPortal() { /* shader hook */ }                                                                                       // placeholder for the portal shader activation call

        public override string GetDescription() =>
            $"Portal Effect | Style: {EffectStyle} | Sound: {SoundPack}";
    }

    // Hideout decoration: placeable cosmetic item
    public class HideoutDecoration : MTXItem                                                                                                    // inherits from MTXItem; represents a decoration for the player's hideout
    {
        public string DecorType     { get; private set; }                                                                                       // the theme category (e.g. "Furniture", "Magic")
        public int    PlacementSlot { get; private set; }                                                                                       // number of hideout layout slots this decoration occupies

        public HideoutDecoration(string id, string name, float price,
                                 DateTime released, string decorType, int slot)
            : base(id, name, ItemType.HideoutDecoration, price, released)                                                                       // calls MTXItem with HideoutDecoration type
        {
            DecorType     = decorType;                                                                                                          // stores the decoration theme/category
            
            PlacementSlot = slot;                                                                                                               // stores how many placement grid slots it takes
        }

        // Adds decoration to the player's hideout layout
        public void PlaceInHideout() { /* hideout engine hook */ }                                                                              // placeholder for the hideout placement engine call

        public override string GetDescription() =>
            $"Hideout Decor | Type: {DecorType} | Slot: {PlacementSlot}";
    }

    // Stash tab expansion: unlocks additional inventory
    public class StashTabExpansion : MTXItem                                                                                                    // inherits from MTXItem; represents extra stash storage
    {
        public string TabType  { get; private set; }                                                                                            // the type of tab unlocked (e.g. "Currency", "Map", "Premium")

        public int    TabCount { get; private set; }                                                                                            // how many new tabs are granted by this purchase

        public StashTabExpansion(string id, string name, float price,
                                 DateTime released, string tabType, int tabCount)
            : base(id, name, ItemType.StashTab, price, released)                                                                                // calls MTXItem with StashTab type
        {
            TabType  = tabType;                                                                                                                 // stores the kind of tab being unlocked

            TabCount = tabCount;                                                                                                                // stores how many tabs are granted
        }

        // Grants additional stash tab slots to the player account
        public void ExpandStash() { /* inventory engine hook */ }                                                                               // placeholder for the stash expansion engine call

        public override string GetDescription() =>
            $"Stash Tab | Type: {TabType} | Count: {TabCount}";
    }

    // Bundle: a discounted collection of MTXItems
    public class Bundle : MTXItem                                                                                                               // inherits from MTXItem; groups multiple items at a discounted price
    {
        public string         BundleId   { get; private set; }                                                                                  // unique bundle code (reuses the item ID)
        
        public float          DiscountPct { get; private set; }                                                                                 // e.g. 0.20 means 20% discount applied to sum of items

        private List<MTXItem> _items;                                                                                                           // private list holding the individual items inside the bundle

        public Bundle(string id, string name, float price,
                      DateTime released, float discountPct)
            : base(id, name, ItemType.Bundle, price, released)                                                                                  // calls MTXItem with Bundle type    
        {
            BundleId    = id;                                                                                                                   // stores the bundle's unique identifier

            DiscountPct = discountPct;                                                                                                          // stores the decimal discount rate

            _items      = new List<MTXItem>();                                                                                                  // initialises an empty list ready to receive items
        }

        // Add an individual item into this bundle
        public void AddItem(MTXItem item) => _items.Add(item);                                                                                  // appends an MTXItem to the bundle's internal item list

        // Returns all items included in this bundle
        
        public List<MTXItem> GetItems() => new List<MTXItem>(_items);                                                                           // returns a copy of the list so callers cannot modify the original

        // Calculates bundle price after discount
        public float TotalPrice() =>
            _items.Sum(i => i.GetPrice()) * (1f - DiscountPct);                                                                                 // sums all item prices then multiplies by (1 - discount)

        public override string GetDescription() =>
            $"Bundle | Items: {_items.Count} | Discount: {DiscountPct:P0}";                                                                     // P0 formats as a percentage with no decimals
    }
}

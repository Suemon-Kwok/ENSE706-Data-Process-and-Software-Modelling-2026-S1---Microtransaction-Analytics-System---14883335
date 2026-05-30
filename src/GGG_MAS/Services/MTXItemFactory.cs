// Name: Suemon Kwok
// Student ID: 14883335
// MTXItemFactory.cs — Factory Method pattern (Task 6 / GoF Pattern 2)
//
// Design pattern: Factory Method (GoF Creational)
//
// WHY THIS PATTERN HERE:
// Phase 1 Program.cs / SeedData.cs used direct new() calls for every item.
// MTXItemFactory.Create() centralises construction so callers only need
// ItemType enum + shared attributes — never a concrete subclass reference.

/*
What design pattern is used and why?
Design pattern: Factory Method — GoF Creational category.
Phase 1 SeedData.cs used 20 direct new WeaponSkin(...), new PetItem(...) etc. calls scattered across the file. Program.cs was tightly coupled to all 7 concrete item subclasses. 
Any constructor signature change broke all 20 call sites simultaneously. There was no central place to add shared post-construction logic.

MTXItemFactory.Create(ItemType, id, name, price, date) centralises all construction. 
Callers receive an MTXItem reference — they never reference concrete subclasses directly.

*/

/*
What does the Factory Method pattern do?
Factory Method defines an interface for creating an object but lets the factory decide which concrete class to instantiate.
The caller never uses new() on the concrete type directly.

In this project: Create(ItemType.WeaponSkin, ...) constructs and returns a WeaponSkin typed as MTXItem. Create(ItemType.Pet, ...) 
constructs a PetItem — same call signature, different concrete type. The caller never knows or cares which subclass was made.

*/


/*
Why Factory Method and not Abstract Factory?
Abstract Factory creates families of related objects (e.g. a Windows UI factory that creates Windows-style Button + Checkbox + TextBox together).
It is for related objects that must be consistent with each other.

This project needs to create individual item objects of different types — not related families. 
Factory Method is the simpler, correct fit: one factory, one product hierarchy, creation controlled by a type parameter.

*/

/*
Which GoF category?
Creational — Factory Method controls how objects are created by encapsulating the new() call behind a method, decoupling the caller from the concrete class.

*/

/*
Overloading in Factory Method
MTXItemFactory demonstrates method overloading: the generic Create(ItemType, id, name, price, date) provides simple creation with defaults. 
The named overloads CreateWeaponSkin(id, name, price, date, weaponClass, effectTier), CreateArmourSkin(...), CreateBundle(...) allow subclass-specific parameters when defaults are not appropriate.

All these methods have the same concept (create an item) but different parameter lists. 
The compiler picks the correct one based on what arguments the caller passes.

*/

/*
Abstraction in Factory Method
The return type of Create() is MTXItem — the abstract base class — not WeaponSkin or PetItem. 
This is abstraction in action: the caller works with the abstract type and never needs to know the concrete type.

Why this matters: if a new item type is added (e.g. MountSkin), the caller does not change. 
It still calls Create(ItemType.MountSkin, ...) and receives an MTXItem. Only the factory changes — one new case in the switch expression.

*/

/*
Tight coupling — how Factory Method solves it
Phase 1 tight coupling: SeedData.cs had new WeaponSkin(id, name, price, date, "Sword", 2) — directly coupled to WeaponSkin's constructor. Change WeaponSkin's constructor → find and fix every direct new() call.
Phase 2 loose coupling: Callers use factory.Create(ItemType.WeaponSkin, id, name, price, date). WeaponSkin's specific constructor arguments are hidden inside the factory. Change WeaponSkin's constructor → fix only MTXItemFactory.cs.

*/

/*
Advantages of Factory Method
Decouples callers from concrete subclasses — callers only know MTXItem
Single place to change when constructor signatures change — only factory.cs
Supports post-construction logic centrally — e.g. register with event bus, apply discount
Named overloads (CreateWeaponSkin) improve discoverability for specific types

*/

/*
Disadvantages of Factory Method
Reduces IDE discoverability: search for 'new PetItem' returns nothing — must know to look at factory
Switch expression must be updated for every new item type — not purely open/closed
Adds a layer of indirection for simple cases where direct new() would be clearer
Factory class can grow large if item type count is very high

*/

using GGG_MAS.Models;

namespace GGG_MAS.Services
{
    // MTXItemFactory is the Creator class in the Factory Method pattern.
    // It provides Create() as the factory method — the single point of item construction.
    // No inheritance required for this Factory: it is a concrete class with overloaded creation methods.
    public class MTXItemFactory
    {
        // Factory Method
        // Constructor parameter names match actual subclass constructors exactly.
        // Create() — the core factory method.
        // Takes ONLY the parameters shared by all MTXItem subclasses.
        // Subclass-specific parameters (weaponClass, slotType, etc.) use sensible defaults here.
        // Callers that need full control use the named overloads below (CreateWeaponSkin etc.)
        //
        // Return type is MTXItem (abstract base class) — NOT WeaponSkin, NOT PetItem.
        // This is abstraction: the caller receives the base type and stays decoupled from the concrete class.
        public MTXItem Create(
            ItemType type,          // which subclass to construct — determined at runtime by the caller
            string   itemId,        // unique catalogue identifier e.g. "WPN_001"
            string   name,          // display name shown in reports and shop e.g. "Infernal Blade Skin"
            float    price,         // sale price in NZD
            DateTime releaseDate)   // date the item became available in the shop
        {
            // switch expression (C# 8+): cleaner than switch statement — each arm returns a value.
            // The factory resolves the correct concrete subclass based on the ItemType enum.
            // Each arm calls the matching subclass constructor with the shared params + sensible defaults.
            // Callers receive MTXItem — they never see WeaponSkin, PetItem, etc.
            return type switch
            {
                // WeaponSkin(string id, string name, float price, DateTime released,
                //            string weaponClass, int effectTier)
                // WeaponSkin: a cosmetic skin applied to a weapon.
                // weaponClass: "Generic" = default weapon category (overridable via CreateWeaponSkin).
                // effectTier: 1 = base visual quality tier (1 lowest, higher = more elaborate effect).
                ItemType.WeaponSkin =>
                    new WeaponSkin(itemId, name, price, releaseDate,
                                   "Generic", 1),

                // ArmourSkin(string id, string name, float price, DateTime released,
                //            ArmourSlot slot, string animSet)
                // ArmourSkin: a cosmetic skin applied to one armour slot.
                // ArmourSlot.Chest = default slot (overridable via CreateArmourSkin).
                // "Default" = default animation set name.
                ItemType.ArmourSkin =>
                    new ArmourSkin(itemId, name, price, releaseDate,
                                   ArmourSlot.Chest, "Default"),

                // PetItem(string id, string name, float price, DateTime released,
                //         string petName, string idleAnim)
                // PetItem: a cosmetic companion that follows the player.
                // "Companion" = generic pet species name (overridable via CreatePet).
                // "Idle" = default idle animation name.
                ItemType.Pet =>
                    new PetItem(itemId, name, price, releaseDate,
                                "Companion", "Idle"),

                // PortalEffect(string id, string name, float price, DateTime released,
                //              string style, string sound)
                // PortalEffect: a cosmetic visual effect applied to town portals.
                // "Default" style and "Default" sound effect — overridable if needed.
                ItemType.PortalEffect =>
                    new PortalEffect(itemId, name, price, releaseDate,
                                     "Default", "Default"),

                // HideoutDecoration(string id, string name, float price, DateTime released,
                //                   string decorType, int slot)
                // HideoutDecoration: a cosmetic object placed in the player's hideout.
                // "Furniture" = default decoration category.
                // 1 = default slot number in the hideout grid.
                ItemType.HideoutDecoration =>
                    new HideoutDecoration(itemId, name, price, releaseDate,
                                          "Furniture", 1),

                // StashTabExpansion(string id, string name, float price, DateTime released,
                //                   string tabType, int tabCount)
                // StashTabExpansion: adds additional tab(s) to the player's stash storage.
                // "Premium" = tab type (Premium tabs have more features than Normal tabs).
                // 1 = adds one tab by default.
                ItemType.StashTab =>
                    new StashTabExpansion(itemId, name, price, releaseDate,
                                          "Premium", 1),

                // Bundle(string id, string name, float price, DateTime released,
                //        float discountPct)
                // Bundle: a discounted collection of multiple MTX items sold together.
                // 0.10f = 10% discount applied to the bundle price vs buying items individually.
                // Items are added separately via bundle.AddItem() after creation.
                ItemType.Bundle =>
                    new Bundle(itemId, name, price, releaseDate, 0.10f),

                // Default arm: throws if an unrecognised ItemType is passed.
                // This is a fail-fast approach — better to crash loudly with a clear message
                // than to silently return null and fail later with a confusing NullReferenceException.
                _ => throw new ArgumentOutOfRangeException(
                         nameof(type), $"Unrecognised ItemType: {type}")
            };
        }

        // Overloaded variants with full parameters
        // These are OVERLOADS of the create concept — same purpose (make an item),
        // different parameter lists. The compiler picks the right one at compile time
        // based on the arguments provided. This is method OVERLOADING (compile-time resolution).
        // CreateWeaponSkin: full-parameter overload for when defaults are not appropriate.
        // weaponClass: specific weapon category e.g. "Sword", "Bow", "Staff".
        // effectTier: visual quality tier 1-5 (higher = more elaborate particle effects).
        // => expression body shorthand — returns a new WeaponSkin directly.
        public WeaponSkin CreateWeaponSkin(
            string id, string name, float price, DateTime released,
            string weaponClass, int effectTier)
            => new WeaponSkin(id, name, price, released, weaponClass, effectTier);

        // CreateArmourSkin: full-parameter overload for armour cosmetics.
        // ArmourSlot: enum specifying which slot (Helmet, Chest, Gloves, Boots, etc.)
        // animSet: name of the animation set (e.g. "EliteAnim", "CasualAnim").
        public ArmourSkin CreateArmourSkin(
            string id, string name, float price, DateTime released,
            ArmourSlot slot, string animSet)
            => new ArmourSkin(id, name, price, released, slot, animSet);

        // CreatePet: full-parameter overload for companion cosmetics.
        // petName: the species/creature name shown in the shop (e.g. "Faerie Dragon").
        // idleAnim: name of the idle animation played when the pet is not moving.
        public PetItem CreatePet(
            string id, string name, float price, DateTime released,
            string petName, string idleAnim)
            => new PetItem(id, name, price, released, petName, idleAnim);

        // CreateBundle: full-parameter overload for bundled item sets.
        // discountPct: the discount percentage e.g. 0.20f = 20% off vs buying items individually.
        // items: optional collection of MTXItem objects to add to the bundle immediately.
        //        IEnumerable<MTXItem>? — the ? makes it nullable (optional parameter).
        public Bundle CreateBundle(
            string id, string name, float price, DateTime released,
            float discountPct, IEnumerable<MTXItem>? items = null)
        {
            // Create the bundle first with its pricing and discount rate.
            var bundle = new Bundle(id, name, price, released, discountPct);

            // If items were provided, add them all to the bundle's internal list.
            // items != null check: only iterate if the caller provided items — avoids null iteration.
            // bundle.AddItem() is the Bundle class's own method for managing its item collection.
            if (items != null)
                foreach (var item in items) 
                    bundle.AddItem(item);   // each item is an MTXItem — polymorphism in action

            // Return the populated bundle typed as Bundle (subclass of MTXItem).
            // Callers that store it as MTXItem get the abstract type; callers that store
            // it as Bundle retain access to Bundle-specific methods like AddItem().
            return bundle;
        }
    }
}

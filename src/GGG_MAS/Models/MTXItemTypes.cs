// =============================================================
// MTXItemTypes.cs — Concrete MTXItem subclasses (UML)
// WeaponSkin, ArmourSkin, PetItem, PortalEffect,
// HideoutDecoration, StashTabExpansion, Bundle
// Demonstrates: Inheritance from MTXItem, Modularity.
// =============================================================

namespace GGG_MAS.Models
{
    // ── Weapon skin: applies a visual effect to a weapon ─────
    public class WeaponSkin : MTXItem
    {
        public string WeaponClass { get; private set; }  // e.g. "Sword", "Bow"
        public int    EffectTier  { get; private set; }  // 1 = basic, 3 = legendary

        public WeaponSkin(string id, string name, float price,
                          DateTime released, string weaponClass, int effectTier)
            : base(id, name, ItemType.WeaponSkin, price, released)
        {
            WeaponClass = weaponClass;
            EffectTier  = effectTier;
        }

        // Applies the skin texture to the specified weapon slot
        public void ApplyToWeapon() { /* visual engine hook */ }

        public override string GetDescription() =>
            $"Weapon Skin | Class: {WeaponClass} | Tier: {EffectTier}";
    }

    // ── Armour skin: applies visual set to an armour slot ────
    public class ArmourSkin : MTXItem
    {
        public ArmourSlot SlotType { get; private set; }  // helm, chest, etc.
        public string     AnimSet  { get; private set; }  // animation bundle name

        public ArmourSkin(string id, string name, float price,
                          DateTime released, ArmourSlot slot, string animSet)
            : base(id, name, ItemType.ArmourSkin, price, released)
        {
            SlotType = slot;
            AnimSet  = animSet;
        }

        // Applies the skin to the specified armour slot
        public void ApplyToArmour() { /* visual engine hook */ }

        public override string GetDescription() =>
            $"Armour Skin | Slot: {SlotType} | Anim: {AnimSet}";
    }

    // ── Pet item: cosmetic companion that follows the player ─
    public class PetItem : MTXItem
    {
        public string PetName  { get; private set; }   // creature name
        public string IdleAnim { get; private set; }   // idle animation key

        public PetItem(string id, string name, float price,
                       DateTime released, string petName, string idleAnim)
            : base(id, name, ItemType.Pet, price, released)
        {
            PetName  = petName;
            IdleAnim = idleAnim;
        }

        // Activates the pet to trail the player
        public void Follow() { /* AI follow hook */ }

        public override string GetDescription() =>
            $"Pet | Name: {PetName} | Idle: {IdleAnim}";
    }

    // ── Portal effect: visual/audio skin for the town portal ─
    public class PortalEffect : MTXItem
    {
        public string EffectStyle { get; private set; }  // shader style
        public string SoundPack   { get; private set; }  // audio bundle

        public PortalEffect(string id, string name, float price,
                            DateTime released, string style, string sound)
            : base(id, name, ItemType.PortalEffect, price, released)
        {
            EffectStyle = style;
            SoundPack   = sound;
        }

        // Applies effect & sound to the player's portal
        public void ApplyToPortal() { /* shader hook */ }

        public override string GetDescription() =>
            $"Portal Effect | Style: {EffectStyle} | Sound: {SoundPack}";
    }

    // ── Hideout decoration: placeable cosmetic item ───────────
    public class HideoutDecoration : MTXItem
    {
        public string DecorType     { get; private set; }  // theme category
        public int    PlacementSlot { get; private set; }  // max slots used

        public HideoutDecoration(string id, string name, float price,
                                 DateTime released, string decorType, int slot)
            : base(id, name, ItemType.HideoutDecoration, price, released)
        {
            DecorType     = decorType;
            PlacementSlot = slot;
        }

        // Adds decoration to the player's hideout layout
        public void PlaceInHideout() { /* hideout engine hook */ }

        public override string GetDescription() =>
            $"Hideout Decor | Type: {DecorType} | Slot: {PlacementSlot}";
    }

    // ── Stash tab expansion: unlocks additional inventory ────
    public class StashTabExpansion : MTXItem
    {
        public string TabType  { get; private set; }  // e.g. "Currency", "Map"
        public int    TabCount { get; private set; }  // number of new tabs

        public StashTabExpansion(string id, string name, float price,
                                 DateTime released, string tabType, int tabCount)
            : base(id, name, ItemType.StashTab, price, released)
        {
            TabType  = tabType;
            TabCount = tabCount;
        }

        // Grants additional stash tab slots to the player account
        public void ExpandStash() { /* inventory engine hook */ }

        public override string GetDescription() =>
            $"Stash Tab | Type: {TabType} | Count: {TabCount}";
    }

    // ── Bundle: a discounted collection of MTXItems ──────────
    public class Bundle : MTXItem
    {
        public string         BundleId   { get; private set; }   // unique bundle code
        public float          DiscountPct { get; private set; }  // e.g. 0.20 = 20% off
        private List<MTXItem> _items;                            // contained items

        public Bundle(string id, string name, float price,
                      DateTime released, float discountPct)
            : base(id, name, ItemType.Bundle, price, released)
        {
            BundleId    = id;
            DiscountPct = discountPct;
            _items      = new List<MTXItem>();
        }

        // Add an individual item into this bundle
        public void AddItem(MTXItem item) => _items.Add(item);

        // Returns all items included in this bundle
        public List<MTXItem> GetItems() => new List<MTXItem>(_items);

        // Calculates bundle price after discount
        public float TotalPrice() =>
            _items.Sum(i => i.GetPrice()) * (1f - DiscountPct);

        public override string GetDescription() =>
            $"Bundle | Items: {_items.Count} | Discount: {DiscountPct:P0}";
    }
}

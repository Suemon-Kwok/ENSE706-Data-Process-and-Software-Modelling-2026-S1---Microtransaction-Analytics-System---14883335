// =============================================================
// Enums.cs — All enumeration types for the MAS system
// Matches the UML Class Diagram enumerations exactly.
// =============================================================

namespace GGG_MAS.Models
{
    // FR02: Item type categories for MTX purchases
    public enum ItemType
    {
        WeaponSkin,
        ArmourSkin,
        Pet,
        PortalEffect,
        HideoutDecoration,
        StashTab,
        Bundle
    }

    // FR03: Path of Exile character classes
    public enum CharacterClass
    {
        Marauder,
        Ranger,
        Witch,
        Duelist,
        Templar,
        Shadow,
        Scion
    }

    // FR07: Player spending tier classification
    public enum SpendingTier
    {
        Casual,      // < $50 total spend
        Regular,     // $50 – $200 total spend
        HighValue    // > $200 total spend (whale)
    }

    // FR13: Internal staff roles controlling access (FR14)
    public enum UserRole
    {
        Analyst,
        MarketingManager,
        Developer,
        FinanceOfficer
    }

    // Armour slot types for ArmourSkin items
    public enum ArmourSlot
    {
        Helm,
        Chest,
        Gloves,
        Boots
    }

    // Export format options for reports (FR12)
    public enum ExportFormat
    {
        CSV,
        PDF,
        JSON
    }

    // Dashboard view types for AnalyticsDashboard
    public enum ViewType
    {
        Sales,
        Demographic,
        Trend,
        Underperforming
    }
}

// Name : Suemon Kwok

// Student ID : 14883335

// Enums.cs — All enumeration types for the MAS system

// Matches the UML Class Diagram enumerations.



namespace GGG_MAS.Models                                                                                   // all model types live in this shared namespace
{
    // FR02: Item type categories for MTX purchases
    public enum ItemType                                                                                   // defines the categories a microtransaction item can belong to     
    {
        WeaponSkin,                                                                                        // visual reskin applied to a player's weapon

        ArmourSkin,                                                                                        // visual reskin applied to a piece of armour

        Pet,                                                                                               // cosmetic companion that follows the player  

        PortalEffect,                                                                                      // visual/audio effect on the player's town portal

        HideoutDecoration,                                                                                 // a placeable cosmetic object in the player's hideout

        StashTab,                                                                                          // an expansion that unlocks additional inventory space

        Bundle                                                                                             // a discounted collection of multiple MTX items
    }

    // FR03: Path of Exile character classes
    public enum CharacterClass                                                                              // the seven playable classes in Path of Exile
    {
        Marauder,                                                                                           // strength-based melee class

        Ranger,                                                                                             // dexterity-based ranged class

        Witch,                                                                                              // intelligence-based caster class

        Duelist,                                                                                            // strength/dexterity hybrid class

        Templar,                                                                                            // strength/intelligence hybrid class

        Shadow,                                                                                             // dexterity/intelligence hybrid class

        Scion                                                                                               // balanced class unlocked after completing the game
    }

    // FR07: Player spending tier classification
    public enum SpendingTier                                                                                // used to segment players by how much they have spent
    {
        Casual,                                                                                             // < $50 total spend

        Regular,                                                                                            // $50 – $200 total spend
        
        HighValue                                                                                           // > $200 total spend (whale)
    }

    // FR13: Internal staff roles controlling access (FR14)
    public enum UserRole                                                                                    // defines which GGG staff role a SystemUser has been assigned
    {
        Analyst,                                                                                            // can view reports and record purchases        

        MarketingManager,                                                                                   // can view reports and export data

        Developer,                                                                                          // can view reports, record purchases, and change configuration

        FinanceOfficer                                                                                      // can view reports and export data
    }

    // Armour slot types for ArmourSkin items
    public enum ArmourSlot                                                                                  // the four armour equipment slots an ArmourSkin can target
    {
        Helm,                                                                                               // head slot

        Chest,                                                                                              // body armour slot     

        Gloves,                                                                                             // glove slot

        Boots                                                                                               // boot slot
    }

    // Export format options for reports (FR12)
    public enum ExportFormat                                                                                // the three file formats that reports can be exported to
    {
        CSV,                                                                                                // comma-separated values — opens in Excel

        PDF,                                                                                                // formatted text report (plain-text fallback, no PDF library required)

        JSON                                                                                                // machine-readable JSON suitable for data pipelines
    }

    // Dashboard view types for AnalyticsDashboard
    public enum ViewType                                                                                    // the four analytics views available in the main dashboard
    {
        Sales,                                                                                              // top sellers by category and character class         

        Demographic,                                                                                        // revenue broken down by region and spending tier

        Trend,                                                                                              // revenue over time (daily / weekly / monthly)

        Underperforming                                                                                     // items whose sales fall below the configured threshold
    }
}

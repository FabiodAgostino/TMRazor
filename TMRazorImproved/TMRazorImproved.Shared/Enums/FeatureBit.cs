namespace TMRazorImproved.Shared.Enums
{
    /// <summary>
    /// Bit di funzionalità usati da alcuni shard per abilitare/disabilitare feature specifiche di Razor.
    /// Equivale alla classe FeatureBit del legacy Razor/Client/Client.cs.
    /// Il server invia una bitmask (in genere via 0xBF sub 0x28 o protocollo custom) che Razor rispetta.
    /// Nella maggior parte dei shard questa funzionalità NON è attiva — il nuovo codice usa
    /// flag booleani di configurazione diretti. Questa enum è fornita per compatibilità.
    /// </summary>
    public enum FeatureBit : uint
    {
        WeatherFilter       = 1u << 0,
        LightFilter         = 1u << 1,
        SmartLT             = 1u << 2,
        RangeCheckLT        = 1u << 3,
        AutoOpenDoors       = 1u << 4,
        UnequipBeforeCast   = 1u << 5,
        AutoPotionEquip     = 1u << 6,
        BlockHealPoisoned   = 1u << 7,
        // Bit 8-15: riservati (LoopingMacros, UseOnceAgent, RestockAgent, SellAgent, BuyAgent, etc.)
        OverheadHealth      = 1u << 16,
        AutolootAgent       = 1u << 17,
        BoneCutterAgent     = 1u << 18,
        // Bit 19: riservato (AdvancedMacros)
        AutoRemount         = 1u << 20,
        AutoBandage         = 1u << 21,
        // Bit 22-26: riservati
        PacketAgent         = 1u << 27,
        FpsOverride         = 1u << 28,
    }
}

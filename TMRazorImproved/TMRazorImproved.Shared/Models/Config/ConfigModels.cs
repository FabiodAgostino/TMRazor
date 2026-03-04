using System;
using System.Collections.Generic;

namespace TMRazorImproved.Shared.Models.Config
{
    /// <summary>
    /// Impostazioni globali dell'applicazione (non legate al profilo personaggio).
    /// </summary>
    public class GlobalSettings
    {
        public string LastProfile { get; set; } = "Default";
        public string ClientPath { get; set; } = string.Empty;
        public string DataPath { get; set; } = string.Empty;
        public string ScriptsPath { get; set; } = string.Empty;
        public bool AutoLaunch { get; set; }
        public bool DebugMode { get; set; }
        public string Language { get; set; } = "en";
        public string Theme { get; set; } = "Dark"; // Light, Dark, HighContrast, Auto
        public string AccentColor { get; set; } = "Default"; // Default or Hex string

        // Engine Patches
        public bool PatchEncryption { get; set; } = true;
        public bool AllowMultiClient { get; set; } = true;
        public bool NegotiateFeatures { get; set; } = true;
        public bool RemoveStaminaCheck { get; set; }
    }

    /// <summary>
    /// Impostazioni specifiche per un profilo utente/personaggio.
    /// </summary>
    public class UserProfile
    {
        public string Name { get; set; } = "Default";
        
        // General Options
        public bool FiltersEnabled { get; set; } = true;
        
        // Classic Filters
        public bool FilterLight { get; set; }
        public bool FilterWeather { get; set; }
        public bool FilterSound { get; set; }
        public bool FilterDeath { get; set; }
        public bool FilterStaff { get; set; }
        public bool FilterPoison { get; set; }
        public bool FilterSnoop { get; set; }
        public bool FilterBardMusic { get; set; }
        public bool FilterFootsteps { get; set; }
        public bool FilterKarmaFame { get; set; }
        public bool FilterSeason { get; set; }
        
        // Advanced Filters
        public bool HighlightFlags { get; set; }
        public bool ColorizeFlags { get; set; }
        public bool ColorizeSelfFlags { get; set; }
        public bool StaticFields { get; set; }
        public bool BlockTradeRequest { get; set; }
        public bool BlockPartyInvite { get; set; }
        public bool MobFilterEnabled { get; set; }
        public bool AutoScreenshotOnDeath { get; set; }
        
        public List<CounterDefinition> Counters { get; set; } = new();
        
        public List<GraphChangeData> GraphFilters { get; set; } = new();
        
        // Auto Agents (moved from Misc logic usually)
        public bool AutoCarver { get; set; }
        public uint AutoCarverBlade { get; set; }
        public bool BoneCutter { get; set; }
        public uint BoneCutterBlade { get; set; }
        public bool AutoRemount { get; set; }
        public uint RemountSerial { get; set; }
        
        // Display Tweaks
        public int ForceWidth { get; set; } = 0; // 0 = disabled
        public int ForceHeight { get; set; } = 0;
        public bool ScaleItems { get; set; }
        public bool AlwaysOnTop { get; set; }
        public double UiOpacity { get; set; } = 1.0;
        public bool ShowNames { get; set; } = true;
        public bool ShowHealth { get; set; } = true;
        public bool HighlightTarget { get; set; } = true;
        public bool IncomingNames { get; set; } = true;
        public bool ShowIncomingDamage { get; set; } = true;
        
        // Agent Settings (Multi-list support)
        public List<AutoLootConfig> AutoLootLists { get; set; } = new() { new AutoLootConfig { Name = "Default" } };
        public string ActiveAutoLootList { get; set; } = "Default";

        public List<ScavengerConfig> ScavengerLists { get; set; } = new() { new ScavengerConfig { Name = "Default" } };
        public string ActiveScavengerList { get; set; } = "Default";

        public List<OrganizerConfig> OrganizerLists { get; set; } = new() { new OrganizerConfig { Name = "Default" } };
        public string ActiveOrganizerList { get; set; } = "Default";

        public List<RestockConfig> RestockLists { get; set; } = new() { new RestockConfig { Name = "Default" } };
        public string ActiveRestockList { get; set; } = "Default";

        public List<VendorConfig> VendorLists { get; set; } = new() { new VendorConfig { Name = "Default" } };
        public string ActiveVendorList { get; set; } = "Default";

        public BandageHealConfig BandageHeal { get; set; } = new();
        public List<DressList> DressLists { get; set; } = new() { new DressList { Name = "Default" } };
        public string ActiveDressList { get; set; } = "Default";

        public List<TargetFilter> TargetLists { get; set; } = new() { new TargetFilter { Name = "Default" } };
        public string ActiveTargetList { get; set; } = "Default";
        
        public TargetingConfig Targeting { get; set; } = new();
        
        public List<FriendsConfig> FriendsLists { get; set; } = new() { new FriendsConfig { Name = "Default" } };
        public string ActiveFriendsList { get; set; } = "Default";

        public SpellGridConfig SpellGrid { get; set; } = new();
        
        // Hotkeys
        public List<HotkeyDefinition> Hotkeys { get; set; } = new();
    }

    public class FriendsConfig
    {
        public string Name { get; set; } = "New List";
        public bool AutoAcceptParty { get; set; }
        public bool PreventAttack { get; set; }
        public bool IncludeParty { get; set; }
        public bool SLFriend { get; set; }
        public bool TBFriend { get; set; }
        public bool COMFriend { get; set; }
        public bool MINFriend { get; set; }

        public List<FriendPlayer> Players { get; set; } = new();
        public List<FriendGuild> Guilds { get; set; } = new();
    }

    public class FriendPlayer
    {
        public string Name { get; set; } = "Unknown";
        public uint Serial { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public class FriendGuild
    {
        public string Name { get; set; } = "Unknown";
        public bool Enabled { get; set; } = true;
    }

    public class CounterDefinition
    {
        public string Name { get; set; } = string.Empty;
        public ushort Graphic { get; set; }
        public ushort Hue { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public string Abbreviation { get; set; } = string.Empty;
    }

    public class GraphChangeData
    {
        public bool Enabled { get; set; } = true;
        public ushort RealID { get; set; }
        public ushort NewID { get; set; }
        public int NewHue { get; set; } = -1; // -1 = no change
    }

    public class TargetFilter
    {
        public string Name { get; set; } = "New Filter";
        public List<int> BodyIDs { get; set; } = new();
        public List<int> Hues { get; set; } = new();
        public List<byte> Notorieties { get; set; } = new();
        
        // Filter Flags (0=Both, 1=Yes, 2=No)
        public int Poisoned { get; set; }
        public int Blessed { get; set; }
        public int IsHuman { get; set; }
        public int IsGhost { get; set; }
        public int Warmode { get; set; }
        public int Paralyzed { get; set; }
        
        public int Range { get; set; } = 12;
    }

    public class AgentConfigBase
    {
        public string Name { get; set; } = "New List";
        public bool Enabled { get; set; }
    }

    public class AutoLootConfig : AgentConfigBase
    {
        public uint Container { get; set; }
        public int Delay { get; set; } = 600;
        public int MaxRange { get; set; } = 2;
        public bool NoOpenCorpse { get; set; }
        public bool AutoStart { get; set; }
        public bool AllowHidden { get; set; }
        public List<LootItem> ItemList { get; set; } = new();
    }

    public class ScavengerConfig : AgentConfigBase
    {
        public uint Container { get; set; }
        public int Range { get; set; } = 2;
        public int Delay { get; set; } = 600;
        public bool AutoStart { get; set; }
        public bool AllowHidden { get; set; }
        public List<LootItem> ItemList { get; set; } = new();
    }

    public class OrganizerConfig : AgentConfigBase
    {
        public uint Source { get; set; }
        public uint Destination { get; set; }
        public int Delay { get; set; } = 600;
        public bool Stack { get; set; } = true;
        public bool Loop { get; set; }
        public bool ShowCompleteMessage { get; set; } = true;
        public List<LootItem> ItemList { get; set; } = new();
    }

    public class RestockConfig : AgentConfigBase
    {
        public uint Source { get; set; }
        public uint Destination { get; set; }
        public int Delay { get; set; } = 600;
        public bool AutoStart { get; set; }
        public List<LootItem> ItemList { get; set; } = new();
    }

    public class BandageHealConfig
    {
        public bool Enabled { get; set; }
        public uint BandageSerial { get; set; }
        public int HpStart { get; set; } = 90;
        public bool PoisonPriority { get; set; } = true;
        public bool HealPoison { get; set; } = true;
        public bool HealMortal { get; set; } = true;
        public bool HiddenStop { get; set; } = true;
        public int CustomDelay { get; set; } = 0; // 0 = automatico basato su DEX
        public int MaxRange { get; set; } = 1;
        public bool ShowCountdown { get; set; }
        public bool AutoStart { get; set; }
        public string TargetType { get; set; } = "Self"; // Self, Last, Friend, Target

        // Nuovi campi migrati
        public bool IgnoreCount { get; set; }
        public bool TimeWithBuff { get; set; }
        public bool UseCustomBandage { get; set; }
        public int CustomBandageID { get; set; }
        public int CustomBandageColor { get; set; }
        public bool SendTextMsg { get; set; }
        public string TextMsgTarget { get; set; } = "[band";
        public string TextMsgSelf { get; set; } = "[bandself";
        public bool UseNormalTarget { get; set; }
    }

    public class VendorConfig : AgentConfigBase
    {
        public bool BuyEnabled { get; set; }
        public bool SellEnabled { get; set; }
        public uint BuyBag { get; set; }
        public uint SellBag { get; set; }
        public bool LogPurchases { get; set; }
        public bool CompareName { get; set; }
        public int MaxSellAmount { get; set; } = 500;
        public List<LootItem> BuyList { get; set; } = new();
        public List<LootItem> SellList { get; set; } = new();
    }

    public class VendorItem
    {
        public ushort Graphic { get; set; }
        public int Amount { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TargetingConfig
    {
        public TargetPriority Priority { get; set; } = TargetPriority.Closest;
        public int Range { get; set; } = 12;
        public bool TargetInnocents { get; set; } = false;
        public bool TargetFriends { get; set; } = false;
    }

    public enum TargetPriority
    {
        Closest,
        LowestHp,
        Random
    }

    public class DressList
    {
        public string Name { get; set; } = "New List";
        public int DragDelay { get; set; } = 600;
        public bool RemoveConflict { get; set; } = true;
        public bool Use3D { get; set; }
        public uint UndressBag { get; set; }
        public Dictionary<byte, uint> LayerItems { get; set; } = new(); // Layer -> Serial
    }

    public class HotkeyDefinition
    {
        public string Action { get; set; } = string.Empty;
        public int KeyCode { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool PassThrough { get; set; } = true;
    }
}

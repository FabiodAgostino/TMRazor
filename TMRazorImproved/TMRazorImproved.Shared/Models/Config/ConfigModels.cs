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
        public bool AutoLaunch { get; set; }
        public bool DebugMode { get; set; }
        public string Language { get; set; } = "en";
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
        
        // Agent Settings (Esempi)
        public AutoLootConfig AutoLoot { get; set; } = new();
        public ScavengerConfig Scavenger { get; set; } = new();
        public OrganizerConfig Organizer { get; set; } = new();
        public BandageHealConfig BandageHeal { get; set; } = new();
        public List<DressList> DressLists { get; set; } = new();
        public TargetingConfig Targeting { get; set; } = new();
        public List<uint> Friends { get; set; } = new(); // Lista di seriali amici
        public VendorConfig Vendor { get; set; } = new();
        
        // Hotkeys
        public List<HotkeyDefinition> Hotkeys { get; set; } = new();
    }

    public class AutoLootConfig
    {
        public bool Enabled { get; set; }
        public uint Container { get; set; }
        public List<uint> ItemList { get; set; } = new();
    }

    public class ScavengerConfig
    {
        public bool Enabled { get; set; }
        public uint Container { get; set; }
        public int Range { get; set; } = 2;
        public List<uint> ItemList { get; set; } = new();
    }

    public class OrganizerConfig
    {
        public bool Enabled { get; set; }
        public uint Source { get; set; }
        public uint Destination { get; set; }
        public List<uint> ItemList { get; set; } = new();
    }

    public class BandageHealConfig
    {
        public bool Enabled { get; set; }
        public uint BandageSerial { get; set; }
        public int HpStart { get; set; } = 90;
        public bool PoisonPriority { get; set; } = true;
        public bool HiddenStop { get; set; } = true;
        public int CustomDelay { get; set; } = 0; // 0 = automatico basato su DEX
    }

    public class VendorConfig
    {
        public bool BuyEnabled { get; set; }
        public bool SellEnabled { get; set; }
        public List<VendorItem> BuyList { get; set; } = new();
        public List<VendorItem> SellList { get; set; } = new();
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
        public Dictionary<byte, uint> LayerItems { get; set; } = new(); // Layer -> Serial
    }

    public class HotkeyDefinition
    {
        public string Action { get; set; } = string.Empty;
        public int KeyCode { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
    }
}

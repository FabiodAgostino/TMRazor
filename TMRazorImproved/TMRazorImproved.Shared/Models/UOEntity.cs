using System;

namespace TMRazorImproved.Shared.Models
{
    /// <summary>
    /// Classe base per ogni entità in gioco (Mobile o Item).
    /// </summary>
    public abstract class UOEntity
    {
        public object SyncRoot { get; } = new();
        public uint Serial { get; }
        public ushort Graphic { get; set; }
        public ushort Hue { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public UOPropertyList? Properties { get; set; }

        protected UOEntity(uint serial)
        {
            Serial = serial;
        }

        public int DistanceTo(UOEntity other)
        {
            if (other == null) return 1000;
            return Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));
        }

        public override bool Equals(object? obj) => obj is UOEntity entity && Serial == entity.Serial;
        public override int GetHashCode() => Serial.GetHashCode();
    }

    /// <summary>
    /// Rappresenta un personaggio (giocatore, NPC o mostro).
    /// </summary>
    public class Mobile : UOEntity
    {
        public string Name { get; set; } = "Unknown";
        public bool IsHidden { get; set; }

        // Stats base
        public ushort Hits { get; set; }
        public ushort HitsMax { get; set; }
        public ushort Mana { get; set; }
        public ushort ManaMax { get; set; }
        public ushort Stam { get; set; }
        public ushort StamMax { get; set; }

        public ushort Str { get; set; }
        public ushort Dex { get; set; }
        public ushort Int { get; set; }

        public bool IsPoisoned { get; set; }
        public bool IsYellowHits { get; set; }
        public byte Notoriety { get; set; }
        public byte Direction { get; set; }

        // Combat
        public bool WarMode { get; set; }
        public uint AttackTarget { get; set; }

        // Stats estese (0x11 type >= 3, AOS+)
        public ushort Gold { get; set; }
        public ushort Armor { get; set; }
        public ushort Weight { get; set; }
        public ushort MaxWeight { get; set; }
        public ushort StatCap { get; set; }
        public byte Followers { get; set; }
        public byte FollowersMax { get; set; }

        // Resistenze (AOS+, via 0xBF sub 0x19)
        public short FireResist { get; set; }
        public short ColdResist { get; set; }
        public short PoisonResist { get; set; }
        public short EnergyResist { get; set; }

        // Misc estese (AOS+)
        public int Luck { get; set; }
        public ushort MinDamage { get; set; }
        public ushort MaxDamage { get; set; }
        public int Tithe { get; set; }

        // Fame e Karma (aggiornabili da messaggi server o handshake)
        public short Fame { get; set; }
        public short Karma { get; set; }

        // Posizione mappa (cambia con 0x76/0xBF sub)
        public int MapId { get; set; }

        // Stato ambiente (cambia con 0xBC/0xC8)
        public byte Season { get; set; }
        public byte VisRange { get; set; } = 18;

        // Feature flags del server (0xB9)
        public ushort Features { get; set; }

        /// <summary>Item che rappresenta il backpack del giocatore (null se non ancora rilevato).</summary>
        public Item? Backpack { get; set; }

        public Mobile(uint serial) : base(serial) { }
    }

    /// <summary>
    /// Rappresenta un oggetto (nel mondo o nei contenitori).
    /// </summary>
    public class Item : UOEntity
    {
        public string Name { get; set; } = "Unknown Item";
        public ushort Amount { get; set; }
        public uint Container { get; set; }
        /// <summary>Alias per Container — usato dalle API di scripting.</summary>
        public uint ContainerSerial { get => Container; set => Container = value; }
        public byte Layer { get; set; }
        public uint RootContainer { get; set; }

        public Item(uint serial) : base(serial) { }
    }
}

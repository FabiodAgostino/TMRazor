using System;

namespace TMRazorImproved.Shared.Models
{
    /// <summary>
    /// Classe base per ogni entità in gioco (Mobile o Item).
    /// </summary>
    public abstract class UOEntity
    {
        public virtual object SyncRoot { get; } = new();
        public virtual uint Serial { get; }
        public virtual string Name { get; set; } = "Unknown";
        public virtual ushort Graphic { get; set; }
        public virtual ushort Hue { get; set; }
        /// <summary>Alias per Hue — usato dalle API di scripting.</summary>
        public virtual int Color { get => Hue; set => Hue = (ushort)value; }
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual int Z { get; set; }
        public virtual byte Flags { get; set; }
        public virtual UOPropertyList? OPL { get; set; }
        public virtual System.Collections.Generic.List<string> Properties => OPL?.Properties.Select(p => p.Arguments).ToList() ?? new();

        protected UOEntity(uint serial)
        {
            Serial = serial;
        }

        public virtual int DistanceTo(UOEntity other)
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
        public virtual bool IsHidden { get; set; }
        public virtual bool IsHuman => Graphic == 0x0190 || Graphic == 0x0191 || Graphic == 0x025D || Graphic == 0x025E || Graphic == 0x025F || Graphic == 0x0260 || Graphic == 0x02E8 || Graphic == 0x02E9;

        // Stats base
        public virtual ushort Hits { get; set; }
        public virtual ushort HitsMax { get; set; }
        public virtual ushort Mana { get; set; }
        public virtual ushort ManaMax { get; set; }
        public virtual ushort Stam { get; set; }
        public virtual ushort StamMax { get; set; }

        public virtual ushort Str { get; set; }
        public virtual ushort Dex { get; set; }
        public virtual ushort Int { get; set; }

        public virtual bool IsPoisoned { get; set; }
        /// <summary>Alias per IsPoisoned — usato dalle API di scripting.</summary>
        public virtual bool Poisoned { get => IsPoisoned; set => IsPoisoned = value; }

        public virtual bool IsYellowHits { get; set; }
        /// <summary>Alias per IsYellowHits — usato dalle API di scripting.</summary>
        public virtual bool YellowHits { get => IsYellowHits; set => IsYellowHits = value; }

        public virtual byte Notoriety { get; set; }
        public virtual byte Direction { get; set; }

        // Combat
        public virtual bool WarMode { get; set; }
        public virtual uint AttackTarget { get; set; }
        public virtual uint LastObject { get; set; }
        public virtual System.Collections.Generic.List<uint> Pets { get; } = new();

        /// <summary>ID abilità speciale primaria corrente (0 = nessuna)</summary>
        public int PrimaryAbilityId { get; set; }
        /// <summary>ID abilità speciale secondaria corrente (0 = nessuna)</summary>
        public int SecondaryAbilityId { get; set; }
        /// <summary>True se la primaria è attiva</summary>
        public bool PrimaryAbilityActive { get; set; }
        /// <summary>True se la secondaria è attiva</summary>
        public bool SecondaryAbilityActive { get; set; }

        // Stats estese (0x11 type >= 3, AOS+)
        public virtual ushort Gold { get; set; }
        public virtual ushort Armor { get; set; }
        public virtual ushort Weight { get; set; }
        public virtual ushort MaxWeight { get; set; }
        public virtual ushort StatCap { get; set; }
        public virtual byte Followers { get; set; }
        public virtual byte FollowersMax { get; set; }

        // Resistenze (AOS+, via 0xBF sub 0x19)
        public virtual short FireResist { get; set; }
        public virtual short ColdResist { get; set; }
        public virtual short PoisonResist { get; set; }
        public virtual short EnergyResist { get; set; }

        // Misc estese (AOS+)
        public virtual int Luck { get; set; }
        public virtual ushort MinDamage { get; set; }
        public virtual ushort MaxDamage { get; set; }
        public virtual int Tithe { get; set; }

        // Fame e Karma (aggiornabili da messaggi server o handshake)
        public virtual short Fame { get; set; }
        public virtual short Karma { get; set; }
        public virtual string KarmaTitle { get; set; } = string.Empty;

        // Posizione mappa (cambia con 0x76/0xBF sub)
        public virtual int MapId { get; set; }
        /// <summary>Alias per MapId — usato dalle API di scripting.</summary>
        public virtual int Map { get => MapId; set => MapId = value; }

        // Stato ambiente (cambia con 0xBC/0xC8)
        public virtual byte Season { get; set; }
        public virtual byte VisRange { get; set; } = 18;

        // Feature flags del server (0xB9)
        public virtual ushort Features { get; set; }

        // Flags fisici aggiuntivi
        public virtual bool Female { get; set; }
        public virtual bool Paralyzed { get; set; }
        /// <summary>Alias per Paralyzed (legacy typo) — usato dalle API di scripting.</summary>
        public virtual bool Paralized { get => Paralyzed; set => Paralyzed = value; }
        public virtual bool Flying { get; set; }
        public virtual bool IsGhost { get; set; }

        // Stat lock (0=Up, 1=Down, 2=Locked)
        public virtual byte StrLock { get; set; }
        public virtual byte DexLock { get; set; }
        public virtual byte IntLock { get; set; }

        // Extended stats AOS+ (da 0x11 type>=5 / type>=6)
        public virtual int AR { get; set; }
        public virtual int HitChanceIncrease { get; set; }
        public virtual int SwingSpeedIncrease { get; set; }
        public virtual int DamageChanceIncrease { get; set; }
        public virtual int LowerReagentCost { get; set; }
        public virtual int HitPointsRegeneration { get; set; }
        public virtual int StaminaRegeneration { get; set; }
        public virtual int ManaRegeneration { get; set; }
        public virtual int ReflectPhysicalDamage { get; set; }
        public virtual int EnhancePotions { get; set; }
        public virtual int DefenseChanceIncrease { get; set; }
        public virtual int SpellDamageIncrease { get; set; }
        public virtual int FasterCastRecovery { get; set; }
        public virtual int FasterCasting { get; set; }
        public virtual int LowerManaCost { get; set; }
        public virtual int StrengthIncrease { get; set; }
        public virtual int DexterityIncrease { get; set; }
        public virtual int IntelligenceIncrease { get; set; }
        public virtual int HitPointsIncrease { get; set; }
        public virtual int StaminaIncrease { get; set; }
        public virtual int ManaIncrease { get; set; }
        public virtual int MaximumHitPointsIncrease { get; set; }
        public virtual int MaximumStaminaIncrease { get; set; }

        // Buff attivi (nome → secondi rimanenti; -1 = senza durata definita)
        public virtual System.Collections.Generic.Dictionary<string, int> ActiveBuffs { get; }
            = new System.Collections.Generic.Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);

        // Corporei (corpses raccolti alla morte)
        public virtual System.Collections.Generic.HashSet<uint> CorpseSerials { get; }
            = new System.Collections.Generic.HashSet<uint>();

        /// <summary>Item che rappresenta il backpack del giocatore (null se non ancora rilevato).</summary>
        public virtual Item? Backpack { get; set; }

        public Mobile(uint serial) : base(serial) { }
    }

    /// <summary>
    /// Rappresenta un oggetto (nel mondo o nei contenitori).
    /// </summary>
    public class Item : UOEntity
    {
        public virtual ushort Amount { get; set; }
        public virtual uint Container { get; set; }
        /// <summary>Alias per Container — usato dalle API di scripting.</summary>
        public virtual uint ContainerSerial { get => Container; set => Container = value; }
        public virtual byte Layer { get; set; }
        public virtual uint RootContainer { get; set; }

        // Proprietà aggiuntive per compatibilità RazorEnhanced
        public virtual ushort Graphics { get => Graphic; set => Graphic = value; }
        public virtual ushort ItemID { get => Graphic; set => Graphic = value; }
        public virtual string Direction { get; set; } = "0";
        public virtual bool Visible { get; set; } = true;
        public virtual bool Movable { get; set; } = true;
        public virtual byte Light { get; set; }
        public virtual byte GridNum { get; set; }
        public virtual bool OnGround => Container == 0;
        public virtual bool IsContainer { get; set; }
        public virtual bool IsBagOfSending { get; set; }
        public virtual bool IsInBank { get; set; }
        public virtual bool IsSearchable { get; set; }
        public virtual bool IsCorpse { get; set; }
        public virtual bool ContainerOpened { get; set; }
        public virtual int CorpseNumberItems { get; set; } = -1;
        public virtual bool IsDoor { get; set; }
        public virtual bool IsLootable { get; set; } = true;
        public virtual bool IsResource { get; set; }
        public virtual bool IsPotion { get; set; }
        public virtual bool IsVirtueShield { get; set; }
        public virtual bool IsTwoHanded { get; set; }
        public virtual int Price { get; set; }
        public virtual string BuyDesc { get; set; } = string.Empty;
        public virtual uint Owner { get; set; }
        public virtual bool PropsUpdated { get; set; }
        public virtual bool Updated { get; set; }

        public virtual int Weight { get; set; }
        public virtual int Durability { get; set; }
        public virtual int MaxDurability { get; set; }

        public Item(uint serial) : base(serial) { }
    }
}

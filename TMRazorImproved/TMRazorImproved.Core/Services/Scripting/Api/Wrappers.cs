using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class ScriptItem : Item
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ITargetingService _targeting;
        internal readonly Item _inner;

        public ScriptItem(Item inner, IWorldService world, IPacketService packet, ITargetingService targeting) : base(inner.Serial)
        {
            _inner = inner;
            _world = world;
            _packet = packet;
            _targeting = targeting;
        }

        public override string Name { get => _inner.Name; set => _inner.Name = value; }
        public override ushort Amount { get => _inner.Amount; set => _inner.Amount = value; }
        public override uint Container { get => _inner.Container; set => _inner.Container = value; }
        public override byte Layer { get => _inner.Layer; set => _inner.Layer = value; }
        public override uint RootContainer { get => _inner.RootContainer; set => _inner.RootContainer = value; }
        public override ushort Graphic { get => _inner.Graphic; set => _inner.Graphic = value; }
        public override ushort Hue { get => _inner.Hue; set => _inner.Hue = value; }
        public override int X { get => _inner.X; set => _inner.X = value; }
        public override int Y { get => _inner.Y; set => _inner.Y = value; }
        public override int Z { get => _inner.Z; set => _inner.Z = value; }
        public override UOPropertyList? OPL { get => _inner.OPL; set => _inner.OPL = value; }

        public new uint Serial => _inner.Serial;
        public new ushort Color => Hue;
        public new ushort Graphics => Graphic;
        public new int ItemID => Graphic;
        public new bool OnGround => Container == 0;

        public virtual List<ScriptItem> Contains => _world.Items.Where(i => i.Container == _inner.Serial).Select(i => new ScriptItem(i, _world, _packet, _targeting)).ToList();
        public new virtual List<string> Properties => _inner.OPL?.Properties.Select(p => p.Arguments).ToList() ?? new List<string>();

        public override bool Visible { get => _inner.Visible; set => _inner.Visible = value; }
        public override bool Movable { get => _inner.Movable; set => _inner.Movable = value; }
        public override byte Light { get => _inner.Light; set => _inner.Light = value; }
        public override byte GridNum { get => _inner.GridNum; set => _inner.GridNum = value; }
        public override bool IsContainer { get => _inner.IsContainer; set => _inner.IsContainer = value; }
        public override bool IsBagOfSending { get => _inner.IsBagOfSending; set => _inner.IsBagOfSending = value; }
        public override bool IsInBank { get => _inner.IsInBank; set => _inner.IsInBank = value; }
        public override bool IsSearchable { get => _inner.IsSearchable; set => _inner.IsSearchable = value; }
        public override bool IsCorpse { get => _inner.IsCorpse; set => _inner.IsCorpse = value; }
        public override bool ContainerOpened { get => _inner.ContainerOpened; set => _inner.ContainerOpened = value; }
        public override int CorpseNumberItems { get => _inner.CorpseNumberItems; set => _inner.CorpseNumberItems = value; }
        public override bool IsDoor { get => _inner.IsDoor; set => _inner.IsDoor = value; }
        public override bool IsLootable { get => _inner.IsLootable; set => _inner.IsLootable = value; }
        public override bool IsResource { get => _inner.IsResource; set => _inner.IsResource = value; }
        public override bool IsPotion { get => _inner.IsPotion; set => _inner.IsPotion = value; }
        public override bool IsVirtueShield { get => _inner.IsVirtueShield; set => _inner.IsVirtueShield = value; }
        public override bool IsTwoHanded { get => _inner.IsTwoHanded; set => _inner.IsTwoHanded = value; }
        public override int Price { get => _inner.Price; set => _inner.Price = value; }
        public override string BuyDesc { get => _inner.BuyDesc; set => _inner.BuyDesc = value; }
        public override bool PropsUpdated { get => _inner.PropsUpdated; set => _inner.PropsUpdated = value; }
        public override bool Updated { get => _inner.Updated; set => _inner.Updated = value; }
        public override int Weight { get => _inner.Weight; set => _inner.Weight = value; }
        public override int Durability { get => _inner.Durability; set => _inner.Durability = value; }
        public override int MaxDurability { get => _inner.MaxDurability; set => _inner.MaxDurability = value; }

        public virtual void Message(int hue, string message) { /* Implementation */ }
        public virtual void Move(uint targetContainer, int amount = -1) { ushort amt = (amount == -1) ? Amount : (ushort)amount; _packet.SendToServer(PacketBuilder.LiftItem(Serial, amt)); _packet.SendToServer(PacketBuilder.DropToContainer(Serial, targetContainer)); }
        public virtual void MoveOnGround(ushort x, ushort y, short z, int amount = -1) { ushort amt = (amount == -1) ? Amount : (ushort)amount; _packet.SendToServer(PacketBuilder.LiftItem(Serial, amt)); _packet.SendToServer(PacketBuilder.DropToWorld(Serial, x, y, z)); }
        public virtual void UseItem() => _packet.SendToServer(PacketBuilder.DoubleClick(Serial));
        public virtual void SingleClick() => _packet.SendToServer(PacketBuilder.SingleClick(Serial));
        public virtual void Select() => _targeting.SendTarget(Serial);
        public virtual int DistanceTo(ScriptMobile m) => (int)Math.Max(Math.Abs(X - m.X), Math.Abs(Y - m.Y));
        public virtual int DistanceTo(ScriptItem i) => (int)Math.Max(Math.Abs(X - i.X), Math.Abs(Y - i.Y));
        public virtual bool IsChildOf(ScriptItem container) => container != null && (Container == container.Serial || RootContainer == container.Serial);
        public virtual bool IsChildOf(ScriptMobile container) => container != null && (Container == container.Serial || RootContainer == container.Serial);
        public virtual void WaitForProps(int timeout = 1000) { }
        public virtual void WaitForContents(int timeout = 1000) { }
        public override string ToString() => $"Item: {Serial:X8} ({Graphic:X4})";

        // Compatibility methods for ItemsApi mapping
        public virtual void ApplyFilter() { }
        public virtual int BackpackCount(int id, int hue = -1) => 0;
        public virtual void ChangeDyeingTubColor(int color) { }
        public virtual void Close() { _packet.SendToServer(PacketBuilder.CloseContainer(Serial)); }
        public virtual int ContainerCount() => Contains.Count;
        public virtual bool ContextExist(string name) => false;
        public virtual void DropFromHand() { }
        public virtual void DropItemGroundSelf(int amount = 1) { }
        public virtual List<ScriptItem> FindAllByID(int id) => new List<ScriptItem>();
        public virtual ScriptItem? FindByID(int id) => null;
        public virtual ScriptItem? FindByName(string name) => null;
        public virtual ScriptItem? FindBySerial(uint serial) => null;
        public virtual List<string> GetProperties() => Properties;
        public virtual string GetPropStringByIndex(int index) => (index >= 0 && index < Properties.Count) ? Properties[index] : "";
        public virtual List<string> GetPropStringList() => Properties;
        public virtual int GetPropValue(string name) => 0;
        public virtual string GetPropValueString(string name) => "";
        public virtual Point3D GetWorldPosition() => new Point3D(X, Y, Z);
        public virtual void Hide() { _packet.SendToClient(PacketBuilder.RemoveObject(Serial)); }
        public virtual void IgnoreTypes(List<int> ids) { }
        public virtual void Lift(int amount = 1) { _packet.SendToServer(PacketBuilder.LiftItem(Serial, (ushort)amount)); }
        public virtual void OpenAt(int x, int y) { UseItem(); }
        public virtual void OpenContainerAt(int x, int y) { UseItem(); }
        public virtual void SetColor(int color) { Hue = (ushort)color; }
        public virtual void UseItemByID(int id) { }
    }

    public class ScriptMobile : Mobile
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ITargetingService _targeting;
        internal readonly Mobile _inner;

        public ScriptMobile(Mobile inner, IWorldService world, IPacketService packet, ITargetingService targeting) : base(inner.Serial)
        {
            _inner = inner;
            _world = world;
            _packet = packet;
            _targeting = targeting;
        }

        public override string Name { get => _inner.Name; set => _inner.Name = value; }
        public override ushort Graphic { get => _inner.Graphic; set => _inner.Graphic = value; }
        public override ushort Hue { get => _inner.Hue; set => _inner.Hue = value; }
        public override int X { get => _inner.X; set => _inner.X = value; }
        public override int Y { get => _inner.Y; set => _inner.Y = value; }
        public override int Z { get => _inner.Z; set => _inner.Z = value; }
        public override UOPropertyList? OPL { get => _inner.OPL; set => _inner.OPL = value; }

        public new uint Serial => _inner.Serial;
        public new ushort Color => Hue;
        public new ushort Graphics => Graphic;
        public int MobileID => Graphic;
        public int ItemID => Graphic;
        public int Body => Graphic;
        public bool InParty => _world.IsPartyMember(_inner.Serial);
        public new virtual List<string> Properties => _inner.OPL?.Properties.Select(p => p.Arguments).ToList() ?? new List<string>();

        public override ushort Hits { get => _inner.Hits; set => _inner.Hits = value; }
        public override ushort HitsMax { get => _inner.HitsMax; set => _inner.HitsMax = value; }
        public override ushort Mana { get => _inner.Mana; set => _inner.Mana = value; }
        public override ushort ManaMax { get => _inner.ManaMax; set => _inner.ManaMax = value; }
        public virtual void OverheadMsg(string message, int color = 945)
        {
            if (string.IsNullOrEmpty(message)) return;
            _packet.SendToClient(PacketBuilder.OverheadUnicodeSpeech(message, _inner.Serial, _inner.Graphic, 0xAE, (ushort)color));
        }

        public virtual void Message(int hue, string message) => OverheadMsg(message, hue);
        public override ushort StamMax { get => _inner.StamMax; set => _inner.StamMax = value; }
        public override bool IsPoisoned { get => _inner.IsPoisoned; set => _inner.IsPoisoned = value; }
        public override bool IsYellowHits { get => _inner.IsYellowHits; set => _inner.IsYellowHits = value; }
        public new bool Poisoned => IsPoisoned;
        public new bool YellowHits => IsYellowHits;
        public bool CanRename => false;
        public override byte Notoriety { get => _inner.Notoriety; set => _inner.Notoriety = value; }
        public override byte Direction { get => _inner.Direction; set => _inner.Direction = value; }
        public override bool WarMode { get => _inner.WarMode; set => _inner.WarMode = value; }
        public override int MapId { get => _inner.MapId; set => _inner.MapId = value; }
        public new int Map => MapId;
        public override bool Paralyzed { get => _inner.Paralyzed; set => _inner.Paralyzed = value; }
        public new bool Paralized => Paralyzed;

        public override Item? Backpack { get { var bp = _world.Items.FirstOrDefault(i => i.Container == _inner.Serial && i.Layer == 0x15); return bp != null ? new ScriptItem(bp, _world, _packet, _targeting) : null; } }
        public override bool IsHidden { get => _inner.IsHidden; set => _inner.IsHidden = value; }
        public bool Visible => !IsHidden;
        public override ushort Str { get => _inner.Str; set => _inner.Str = value; }
        public override ushort Dex { get => _inner.Dex; set => _inner.Dex = value; }
        public override ushort Int { get => _inner.Int; set => _inner.Int = value; }
        public override uint AttackTarget { get => _inner.AttackTarget; set => _inner.AttackTarget = value; }
        public override ushort Gold { get => _inner.Gold; set => _inner.Gold = value; }
        public override ushort Armor { get => _inner.Armor; set => _inner.Armor = value; }
        public override ushort Weight { get => _inner.Weight; set => _inner.Weight = value; }
        public override ushort MaxWeight { get => _inner.MaxWeight; set => _inner.MaxWeight = value; }
        public override ushort StatCap { get => _inner.StatCap; set => _inner.StatCap = value; }
        public override byte Followers { get => _inner.Followers; set => _inner.Followers = value; }
        public override byte FollowersMax { get => _inner.FollowersMax; set => _inner.FollowersMax = value; }
        public override short FireResist { get => _inner.FireResist; set => _inner.FireResist = value; }
        public override short ColdResist { get => _inner.ColdResist; set => _inner.ColdResist = value; }
        public override short PoisonResist { get => _inner.PoisonResist; set => _inner.PoisonResist = value; }
        public override short EnergyResist { get => _inner.EnergyResist; set => _inner.EnergyResist = value; }
        public override int Luck { get => _inner.Luck; set => _inner.Luck = value; }
        public override ushort MinDamage { get => _inner.MinDamage; set => _inner.MinDamage = value; }
        public override ushort MaxDamage { get => _inner.MaxDamage; set => _inner.MaxDamage = value; }
        public override int Tithe { get => _inner.Tithe; set => _inner.Tithe = value; }
        public override short Fame { get => _inner.Fame; set => _inner.Fame = value; }
        public override short Karma { get => _inner.Karma; set => _inner.Karma = value; }
        public override string KarmaTitle { get => _inner.KarmaTitle; set => _inner.KarmaTitle = value; }
        public override byte Season { get => _inner.Season; set => _inner.Season = value; }
        public override byte VisRange { get => _inner.VisRange; set => _inner.VisRange = value; }
        public override ushort Features { get => _inner.Features; set => _inner.Features = value; }
        public override bool Female { get => _inner.Female; set => _inner.Female = value; }
        public override bool Flying { get => _inner.Flying; set => _inner.Flying = value; }
        public override bool IsGhost { get => _inner.IsGhost; set => _inner.IsGhost = value; }
        public override bool IsHuman => _inner.IsHuman;
        public override byte StrLock { get => _inner.StrLock; set => _inner.StrLock = value; }
        public override byte DexLock { get => _inner.DexLock; set => _inner.DexLock = value; }
        public override byte IntLock { get => _inner.IntLock; set => _inner.IntLock = value; }
        public override int AR { get => _inner.AR; set => _inner.AR = value; }
        public override int HitChanceIncrease { get => _inner.HitChanceIncrease; set => _inner.HitChanceIncrease = value; }
        public override int SwingSpeedIncrease { get => _inner.SwingSpeedIncrease; set => _inner.SwingSpeedIncrease = value; }
        public override int DamageChanceIncrease { get => _inner.DamageChanceIncrease; set => _inner.DamageChanceIncrease = value; }
        public override int LowerReagentCost { get => _inner.LowerReagentCost; set => _inner.LowerReagentCost = value; }
        public override int HitPointsRegeneration { get => _inner.HitPointsRegeneration; set => _inner.HitPointsRegeneration = value; }
        public override int StaminaRegeneration { get => _inner.StaminaRegeneration; set => _inner.StaminaRegeneration = value; }
        public override int ManaRegeneration { get => _inner.ManaRegeneration; set => _inner.ManaRegeneration = value; }
        public override int ReflectPhysicalDamage { get => _inner.ReflectPhysicalDamage; set => _inner.ReflectPhysicalDamage = value; }
        public override int EnhancePotions { get => _inner.EnhancePotions; set => _inner.EnhancePotions = value; }
        public override int DefenseChanceIncrease { get => _inner.DefenseChanceIncrease; set => _inner.DefenseChanceIncrease = value; }
        public override int SpellDamageIncrease { get => _inner.SpellDamageIncrease; set => _inner.SpellDamageIncrease = value; }
        public override int FasterCastRecovery { get => _inner.FasterCastRecovery; set => _inner.FasterCastRecovery = value; }
        public override int FasterCasting { get => _inner.FasterCasting; set => _inner.FasterCasting = value; }
        public override int LowerManaCost { get => _inner.LowerManaCost; set => _inner.LowerManaCost = value; }
        public override int StrengthIncrease { get => _inner.StrengthIncrease; set => _inner.StrengthIncrease = value; }
        public override int DexterityIncrease { get => _inner.DexterityIncrease; set => _inner.DexterityIncrease = value; }
        public override int IntelligenceIncrease { get => _inner.IntelligenceIncrease; set => _inner.IntelligenceIncrease = value; }
        public override int HitPointsIncrease { get => _inner.HitPointsIncrease; set => _inner.HitPointsIncrease = value; }
        public override int StaminaIncrease { get => _inner.StaminaIncrease; set => _inner.StaminaIncrease = value; }
        public override int ManaIncrease { get => _inner.ManaIncrease; set => _inner.ManaIncrease = value; }
        public override int MaximumHitPointsIncrease { get => _inner.MaximumHitPointsIncrease; set => _inner.MaximumHitPointsIncrease = value; }
        public override int MaximumStaminaIncrease { get => _inner.MaximumStaminaIncrease; set => _inner.MaximumStaminaIncrease = value; }

        public virtual Item? Mount => WrapItem(_world.Items.FirstOrDefault(i => i.Container == _inner.Serial && i.Layer == 0x19));
        public virtual Item? Quiver => WrapItem(_world.Items.FirstOrDefault(i => i.Container == _inner.Serial && i.Layer == 0x16));
        public virtual List<Item> Contains => _world.Items.Where(i => i.Container == _inner.Serial).Select(i => (Item)new ScriptItem(i, _world, _packet, _targeting)).ToList();
        public virtual void UseMobile() => _packet.SendToServer(PacketBuilder.DoubleClick(Serial));
        public virtual void SingleClick() => _packet.SendToServer(PacketBuilder.SingleClick(Serial));
        public virtual void Select() => _targeting.SendTarget(Serial);
        public virtual int DistanceTo(ScriptMobile m) => (int)Math.Max(Math.Abs(X - m.X), Math.Abs(Y - m.Y));
        public virtual int DistanceTo(ScriptItem i) => (int)Math.Max(Math.Abs(X - i.X), Math.Abs(Y - i.Y));
        public virtual bool UpdateKarma() { _packet.SendToServer(PacketBuilder.RequestProfile(Serial)); return true; }
        public virtual void WaitForProps(int timeout = 1000) { }
        public virtual void WaitForStats(int timeout = 1000) { }
        public override string ToString() => $"Mobile: {Serial:X8} ({Graphic:X4}) '{Name}'";

        // Compatibility methods
        public virtual void ApplyFilter() { }
        public virtual bool ContextExist(string name) => false;
        public virtual ScriptMobile? FindBySerial(uint serial) => null;
        public virtual ScriptMobile? FindMobile(uint serial) => null;
        public virtual ScriptItem? GetItemOnLayer(string layer) => null;
        public virtual string GetPropStringByIndex(int index) => (index >= 0 && index < Properties.Count) ? Properties[index] : "";
        public virtual List<string> GetPropStringList() => Properties;
        public virtual int GetPropValue(string name) => 0;
        public virtual void GetTargetingFilter() { }
        public virtual void GetTrackingInfo() { }
        public bool PropsUpdated => true;

        private ScriptItem? WrapItem(Item? i) => i == null ? null : new ScriptItem(i, _world, _packet, _targeting);
    }
}

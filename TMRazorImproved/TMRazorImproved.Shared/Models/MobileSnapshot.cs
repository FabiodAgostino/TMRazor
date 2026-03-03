using System;

namespace TMRazorImproved.Shared.Models
{
    /// <summary>
    /// Snapshot immutabile e thread-safe delle proprietà di un Mobile.
    /// Usare quando si necessita di un insieme coerente di valori
    /// (es. posizione X+Y+Z, oppure Hits+HitsMax per calcoli percentuali).
    /// </summary>
    public readonly record struct MobileSnapshot(
        uint Serial, 
        string Name,
        int X, 
        int Y, 
        int Z,
        ushort Graphic, 
        ushort Hue,
        ushort Hits, 
        ushort HitsMax,
        ushort Mana, 
        ushort ManaMax,
        ushort Stam, 
        ushort StamMax,
        byte Notoriety, 
        byte Direction);

    /// <summary>
    /// Snapshot immutabile e thread-safe delle proprietà di un Item.
    /// </summary>
    public readonly record struct ItemSnapshot(
        uint Serial,
        ushort Graphic,
        ushort Hue,
        int X,
        int Y,
        int Z,
        ushort Amount,
        uint Container,
        byte Layer);

    public static class EntityExtensions
    {
        /// <summary>
        /// Cattura uno snapshot thread-safe delle proprietà correnti del Mobile.
        /// Le letture di proprietà singole sono atomiche su x86;
        /// il record garantisce che l'insieme sia coerente.
        /// </summary>
        public static MobileSnapshot Snapshot(this Mobile m)
        {
            lock (m.SyncRoot)
            {
                return new MobileSnapshot(
                    m.Serial,
                    m.Name,
                    m.X,
                    m.Y,
                    m.Z,
                    m.Graphic,
                    m.Hue,
                    m.Hits,
                    m.HitsMax,
                    m.Mana,
                    m.ManaMax,
                    m.Stam,
                    m.StamMax,
                    m.Notoriety,
                    m.Direction);
            }
        }

        /// <summary>
        /// Cattura uno snapshot thread-safe delle proprietà correnti dell'Item.
        /// </summary>
        public static ItemSnapshot Snapshot(this Item i)
        {
            lock (i.SyncRoot)
            {
                return new ItemSnapshot(
                    i.Serial,
                    i.Graphic,
                    i.Hue,
                    i.X,
                    i.Y,
                    i.Z,
                    i.Amount,
                    i.Container,
                    i.Layer);
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API esposta agli script Python come variabile <c>Items</c>.
    /// Compatibile con la firma usata dagli script RazorEnhanced.
    ///
    /// Tutte le proprietà e i metodi sono <c>public virtual</c> per garantire
    /// che il binder IronPython possa accedervi correttamente su .NET 8/10.
    /// (I membri non-virtual sealed possono causare AttributeError nel binder DLR.)
    /// </summary>
    public class ItemsApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        public ItemsApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel)
        {
            _world = world;
            _packet = packet;
            _cancel = cancel;
        }

        // ------------------------------------------------------------------
        // Ricerca per identificatore
        // ------------------------------------------------------------------

        /// <summary>Cerca un item per serial numerico. Ritorna None se non trovato.</summary>
        public virtual Item? FindBySerial(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindItem(serial);
        }

        /// <summary>Cerca il primo item con un determinato graphic ID nella lista globale.</summary>
        public virtual Item? FindByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return _world.Items.FirstOrDefault(i => i.Graphic == graphic);
        }

        /// <summary>Ritorna tutti gli item con un determinato graphic ID.</summary>
        public virtual IEnumerable<Item> FindAllByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return _world.Items.Where(i => i.Graphic == graphic).ToList();
        }

        /// <summary>Ritorna tutti gli item nel backpack del giocatore.</summary>
        public virtual IEnumerable<Item> GetBackpackItems()
        {
            _cancel.ThrowIfCancelled();
            var bp = _world.Player?.Backpack;
            if (bp == null) return Enumerable.Empty<Item>();
            return _world.Items.Where(i => i.ContainerSerial == bp.Serial).ToList();
        }

        /// <summary>Controlla se un item esiste nel mondo corrente.</summary>
        public virtual bool Exists(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindItem(serial) != null;
        }

        // ------------------------------------------------------------------
        // Proprietà di un item
        // ------------------------------------------------------------------

        /// <summary>Ritorna il nome (dalla OPL) di un item, oppure stringa vuota.</summary>
        public virtual string GetName(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var item = _world.FindItem(serial);
            return item?.Properties?.GetNameOrEmpty() ?? string.Empty;
        }

        /// <summary>Ritorna la quantità (Amount) di un item.</summary>
        public virtual int GetAmount(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindItem(serial)?.Amount ?? 0;
        }

        // ------------------------------------------------------------------
        // Azioni
        // ------------------------------------------------------------------

        public virtual void UseItem(uint serial)
        {
            _cancel.ThrowIfCancelled();
            byte[] packet = new byte[5];
            packet[0] = 0x06; // Double Click
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(1), serial);
            _packet.SendToServer(packet);
        }

        public virtual void Click(uint serial)
        {
            _cancel.ThrowIfCancelled();
            byte[] packet = new byte[5];
            packet[0] = 0x09; // Single Click
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(1), serial);
            _packet.SendToServer(packet);
        }

        public virtual void Move(uint serial, uint targetContainer, int amount = 1)
        {
            _cancel.ThrowIfCancelled();
            byte[] lift = new byte[7];
            lift[0] = 0x07;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(lift.AsSpan(1), serial);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(lift.AsSpan(5), (ushort)amount);
            _packet.SendToServer(lift);

            byte[] drop = new byte[15];
            drop[0] = 0x08;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(1), serial);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(5), 0xFFFF);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(7), 0xFFFF);
            drop[9] = 0;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(11), targetContainer);
            _packet.SendToServer(drop);
        }
    }
}

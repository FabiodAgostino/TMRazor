using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Core.Utilities;
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
        private readonly ILogger<ItemsApi>? _logger;

        public ItemsApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel, ILogger<ItemsApi>? logger = null)
        {
            _world = world;
            _packet = packet;
            _cancel = cancel;
            _logger = logger;
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

        public virtual Item? FindByID(int graphic, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return searchSpace.FirstOrDefault(i => i.Graphic == graphic && (hue == -1 || i.Hue == hue));
        }

        public virtual List<Item> FindAllByID(int graphic, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return searchSpace.Where(i => i.Graphic == graphic && (hue == -1 || i.Hue == hue)).ToList();
        }

        private IEnumerable<Item> GetItemsInContainer(uint containerSerial, bool recurse)
        {
            var children = _world.Items.Where(i => i.ContainerSerial == containerSerial).ToList();
            foreach (var child in children)
            {
                yield return child;
                if (recurse)
                {
                    foreach (var grandchild in GetItemsInContainer(child.Serial, true))
                        yield return grandchild;
                }
            }
        }

        public virtual void Select(uint serial)
        {
            _cancel.ThrowIfCancelled();
            // Spesso usato per forzare il target o l'ispezione
        }

        // FIX BUG-P2-05: implementate tramite UOPropertyList (OPL) già presente sull'entità
        public virtual string GetPropString(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            var entity = _world.FindItem(serial) as TMRazorImproved.Shared.Models.UOEntity
                      ?? _world.FindMobile(serial) as TMRazorImproved.Shared.Models.UOEntity;
            if (entity?.Properties == null) return string.Empty;

            // Cerca la property che contiene il nome cercato negli Arguments (case-insensitive)
            var entry = entity.Properties.Properties.FirstOrDefault(p =>
                p.Arguments.Contains(name, StringComparison.OrdinalIgnoreCase));
            return entry?.Arguments ?? string.Empty;
        }

        public virtual int GetPropValue(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            string text = GetPropString(serial, name);
            if (string.IsNullOrEmpty(text)) return 0;
            // Estrae il primo numero intero dal testo della proprietà
            var match = Regex.Match(text, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
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
            _logger?.LogDebug("UseItem: serial=0x{Serial:X}", serial);
            _packet.SendToServer(PacketBuilder.DoubleClick(serial));
        }

        public virtual void Click(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.SingleClick(serial));
        }

        public virtual void Move(uint serial, uint targetContainer, int amount = 1)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogDebug("Move: serial=0x{Serial:X} amount={Amount} → container=0x{Container:X}", serial, amount, targetContainer);
            _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount));
            _packet.SendToServer(PacketBuilder.DropToContainer(serial, targetContainer));
        }
    }
}

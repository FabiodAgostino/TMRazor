using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;

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
        // FIX P1-01: messenger iniettato invece di WeakReferenceMessenger.Default
        // per correttezza nei test unitari e per rispettare la DI chain.
        private readonly IMessenger _messenger;

        public ItemsApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel,
            ILogger<ItemsApi>? logger = null, IMessenger? messenger = null)
        {
            _world = world;
            _packet = packet;
            _cancel = cancel;
            _logger = logger;
            _messenger = messenger ?? WeakReferenceMessenger.Default;
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

        /// <summary>Cerca il primo item con uno dei graphic ID specificati.</summary>
        public virtual Item? FindByID(System.Collections.IEnumerable graphics, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            var ids = ConvertToList(graphics);
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return searchSpace.FirstOrDefault(i => ids.Contains(i.Graphic) && (hue == -1 || i.Hue == hue));
        }

        /// <summary>Ritorna tutti gli item con uno dei graphic ID specificati.</summary>
        public virtual List<Item> FindAllByID(System.Collections.IEnumerable graphics, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            var ids = ConvertToList(graphics);
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return searchSpace.Where(i => ids.Contains(i.Graphic) && (hue == -1 || i.Hue == hue)).ToList();
        }

        private List<int> ConvertToList(System.Collections.IEnumerable input)
        {
            var list = new List<int>();
            foreach (var item in input)
            {
                if (item is int i) list.Add(i);
                else if (item is IConvertible c) list.Add(Convert.ToInt32(c));
            }
            return list;
        }

        /// <summary>Attende che il server invii il contenuto del contenitore (pacchetto 0x3C).</summary>
        public virtual bool WaitForContents(uint serial, int timeout = 5000)
        {
            _cancel.ThrowIfCancelled();
            var deadline = Environment.TickCount64 + timeout;
            
            // In RazorEnhanced, WaitForContents aspetta che arrivi il pacchetto 0x3C.
            // Possiamo usare il Messenger per ricevere la notifica del ContainerContentMessage.
            // FIX P1-01: usa il messenger iniettato anziché WeakReferenceMessenger.Default
            bool received = false;
            _messenger.Register<ItemsApi, ContainerContentMessage>(this, (r, m) =>
            {
                if (m.Value.ContainerSerial == serial) received = true;
            });

            try
            {
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (received) return true;
                    System.Threading.Thread.Sleep(50);
                }
                return false;
            }
            finally
            {
                _messenger.Unregister<ContainerContentMessage>(this);
            }
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
            
            // Regex migliorata per gestire "45 / 50" (estrae 45), "+15%" (estrae 15), "10" (estrae 10)
            // Cerca il primo gruppo di cifre, potenzialmente preceduto da + o seguito da /
            var match = Regex.Match(text, @"([-+]?\d+)(?:\s*/\s*\d+)?");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int val))
                    return val;
            }
            return 0;
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

        /// <summary>Ritorna tutti gli item entro il range dal giocatore (solo item a terra).</summary>
        public virtual IEnumerable<Item> FindAllInRange(int range)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return Enumerable.Empty<Item>();

            return _world.Items
                .Where(i => i.ContainerSerial == 0 && i.DistanceTo(player) <= range)
                .ToList();
        }

        public virtual IEnumerable<Item> FilterByGraphic(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return _world.Items.Where(i => i.Graphic == (ushort)graphic).ToList();
        }

        public virtual IEnumerable<Item> FilterByHue(int hue)
        {
            _cancel.ThrowIfCancelled();
            return _world.Items.Where(i => i.Hue == (ushort)hue).ToList();
        }

        /// <summary>Cerca il primo item con il graphic specificato nel backpack e lo usa.</summary>
        public virtual void UseType(int graphic, int hue = -1)
        {
            _cancel.ThrowIfCancelled();
            var bp = _world.Player?.Backpack;
            if (bp == null) return;

            var item = _world.Items.FirstOrDefault(i => 
                i.ContainerSerial == bp.Serial && 
                i.Graphic == (ushort)graphic && 
                (hue == -1 || i.Hue == (ushort)hue));

            if (item != null)
                UseItem(item.Serial);
        }

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

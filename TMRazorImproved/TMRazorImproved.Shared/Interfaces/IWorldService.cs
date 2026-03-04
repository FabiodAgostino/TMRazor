using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    /// <summary>
    /// Servizio che mantiene lo stato in tempo reale del mondo di gioco.
    /// </summary>
    public interface IWorldService
    {
        Mobile? Player { get; }
        UOGump? CurrentGump { get; }
        System.Collections.Concurrent.ConcurrentDictionary<uint, UOGump> OpenGumps { get; }
        bool IsCasting { get; set; }

        /// <summary>Serial dell'ultimo contenitore aperto (aggiornato da 0x3C).</summary>
        uint LastOpenedContainer { get; }

        IEnumerable<Mobile> Mobiles { get; }
        IEnumerable<Item> Items { get; }
        HashSet<uint> PartyMembers { get; }

        Mobile? FindMobile(uint serial);
        Item? FindItem(uint serial);
        UOEntity? FindEntity(uint serial);
        IEnumerable<Item> GetItemsInContainer(uint containerSerial);

        void AddMobile(Mobile mobile);
        void AddItem(Item item);
        
        void AddPartyMember(uint serial);
        void RemovePartyMember(uint serial);
        void ClearParty();

        void RemoveMobile(uint serial);
        void RemoveItem(uint serial);

        void SetPlayer(Mobile player);
        void SetCurrentGump(UOGump? gump);
        void RemoveGump(uint gumpId);
        void RemoveGump(); // Rimuove il CurrentGump (per compatibilità)
        void SetLastOpenedContainer(uint serial);

        void Clear();
    }
}

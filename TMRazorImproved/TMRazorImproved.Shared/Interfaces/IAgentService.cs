using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IAgentService
    {
        bool IsRunning { get; }
        void Start();
        Task StopAsync();
    }
    
    public interface IAutoLootService : IAgentService
    {
        void ChangeList(string listName);
        /// <summary>Runs a one-shot loot pass using the specified list (FR-030).</summary>
        void RunOnce(string listName, int msDelay);
        /// <summary>Toggles NoOpenCorpse temporarily; returns the previous value (FR-031).</summary>
        bool SetNoOpenCorpse(bool noOpen);
        /// <summary>Returns the item list for the named AutoLoot config (FR-032).</summary>
        System.Collections.Generic.List<TMRazorImproved.Shared.Models.LootItem> GetList(string listName);
        /// <summary>Returns the serial of the active loot bag (FR-032).</summary>
        uint GetLootBag();
        /// <summary>Clears the processed-serials set and drains the pending loot queue (FR-032).</summary>
        void ResetIgnore();
    }

    public interface IScavengerService : IAgentService
    {
        void ChangeList(string listName);
        /// <summary>One-shot scavenge pass from the pending ground-item queue (FR-045).</summary>
        void RunOnce();
        /// <summary>Returns the serial of the active scavenge bag (FR-045).</summary>
        uint GetScavengerBag();
        /// <summary>Clears processed-serials and pending queue (FR-045).</summary>
        void ResetIgnore();
    }

    public interface IOrganizerService : IAgentService
    {
        void ChangeList(string listName);
        event Action OnComplete;
        /// <summary>One-shot organizer pass with explicit source/dest/delay (FR-042).</summary>
        void RunOnce(string listName, uint sourceSerial, uint destSerial, int delayMs);
    }

    public interface IBandageHealService : IAgentService
    {
    }

    public interface IDressService : IAgentService
    {
        void ChangeList(string listName);
        void Dress(string listName);
        void Undress(string listName);
        void DressUp();
        void Undress();
        /// <summary>Saves the player's current equipment to the active dress list (FR-039).</summary>
        void ReadPlayerDress();
        /// <summary>Returns true while a dress operation is in progress (FR-041).</summary>
        bool DressStatus();
        /// <summary>Returns true while an undress operation is in progress (FR-041).</summary>
        bool UnDressStatus();
    }

    public interface IVendorService : IAgentService
    {
        void ExecuteBuy(uint vendorSerial, System.Collections.Generic.List<(uint Serial, ushort Amount)> items);
        void ExecuteSell(uint vendorSerial, System.Collections.Generic.List<(uint Serial, ushort Amount)> items);
        void SetBuyList(string listName);
        void SetSellList(string listName);
        void ClearBuyList();
        void ClearSellList();
        /// <summary>Script-driven buy by graphic ID (FR-047).</summary>
        void Buy(uint vendorSerial, int itemID, int amount, int maxPrice = 0);
        /// <summary>Script-driven buy by item name (FR-047).</summary>
        void Buy(uint vendorSerial, string itemName, int amount, int maxPrice = 0);
        /// <summary>Returns the items in the last-opened vendor container (FR-047).</summary>
        System.Collections.Generic.List<(string Name, int Graphic, uint Price)> BuyList(uint vendorSerial);
    }

    public interface IRestockService : IAgentService
    {
        void ChangeList(string listName);
        event Action OnComplete;
        /// <summary>One-shot restock pass with explicit source/dest/delay (FR-043).</summary>
        void RunOnce(string listName, uint sourceSerial, uint destSerial, int delayMs);
    }
}

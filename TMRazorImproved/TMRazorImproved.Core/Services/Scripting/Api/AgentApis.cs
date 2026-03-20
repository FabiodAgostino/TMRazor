using System;
using System.Collections.Generic;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>Base class for all agent API wrappers.</summary>
    public abstract class AgentApiBase
    {
        protected readonly ScriptCancellationController _cancel;
        public AgentApiBase(ScriptCancellationController cancel) { _cancel = cancel; }
    }

    /// <summary>Controls the AutoLoot agent, which automatically picks up items from corpses.</summary>
    public class AutoLootApi : AgentApiBase
    {
        private readonly IAutoLootService _service;
        public AutoLootApi(IAutoLootService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        /// <summary>Starts the AutoLoot agent.</summary>
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        /// <summary>Stops the AutoLoot agent.</summary>
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        /// <summary>Returns <c>true</c> if the AutoLoot agent is currently running.</summary>
        public virtual bool Status() => _service.IsRunning;
        /// <summary>Switches the active loot list to the list with the given name.</summary>
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
        /// <summary>Runs a one-shot loot pass using the given list. Does not start the continuous agent loop.</summary>
        public virtual void RunOnce(string listName, int msDelay = 600) { _cancel.ThrowIfCancelled(); _service.RunOnce(listName, msDelay); }
        /// <summary>Toggles the NoOpenCorpse option and returns the previous value.</summary>
        public virtual bool SetNoOpenCorpse(bool noOpen) { _cancel.ThrowIfCancelled(); return _service.SetNoOpenCorpse(noOpen); }
        /// <summary>Returns the item list for the named AutoLoot config.</summary>
        public virtual List<LootItem> GetList(string listName) { _cancel.ThrowIfCancelled(); return _service.GetList(listName); }
        /// <summary>Returns the serial of the active loot bag (configured container or backpack).</summary>
        public virtual uint GetLootBag() { _cancel.ThrowIfCancelled(); return _service.GetLootBag(); }
        /// <summary>Clears the processed-serials ignore set and the pending loot queue.</summary>
        public virtual void ResetIgnore() { _cancel.ThrowIfCancelled(); _service.ResetIgnore(); }
    }

    /// <summary>Controls the Dress agent, which equips or removes items according to a dress list.</summary>
    public class DressApi : AgentApiBase
    {
        private readonly IDressService _service;
        public DressApi(IDressService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        /// <summary>Starts the Dress agent.</summary>
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        /// <summary>Stops the Dress agent.</summary>
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        /// <summary>Returns <c>true</c> if the Dress agent is currently running.</summary>
        public virtual bool Status() => _service.IsRunning;
        /// <summary>Switches the active dress list to the list with the given name.</summary>
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
        /// <summary>Equips all items in the active dress list.</summary>
        public virtual void DressUp() { _cancel.ThrowIfCancelled(); _service.DressUp(); }
        /// <summary>Removes all items in the active dress list and places them in the backpack.</summary>
        public virtual void Undress() { _cancel.ThrowIfCancelled(); _service.Undress(); }
        /// <summary>Captures current player equipment and saves it to the active dress list.</summary>
        public virtual void ReadPlayerDress() { _cancel.ThrowIfCancelled(); _service.ReadPlayerDress(); }
        /// <summary>Returns true while a dress operation is in progress.</summary>
        public virtual bool DressStatus() { _cancel.ThrowIfCancelled(); return _service.DressStatus(); }
        /// <summary>Returns true while an undress operation is in progress.</summary>
        public virtual bool UnDressStatus() { _cancel.ThrowIfCancelled(); return _service.UnDressStatus(); }
    }

    /// <summary>Controls the Scavenger agent, which automatically picks up items that appear on the ground.</summary>
    public class ScavengerApi : AgentApiBase
    {
        private readonly IScavengerService _service;
        public ScavengerApi(IScavengerService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        /// <summary>Starts the Scavenger agent.</summary>
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        /// <summary>Stops the Scavenger agent.</summary>
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        /// <summary>Returns <c>true</c> if the Scavenger agent is currently running.</summary>
        public virtual bool Status() => _service.IsRunning;
        /// <summary>Switches the active scavenger list to the list with the given name.</summary>
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
        /// <summary>One-shot scavenge pass from the pending queue (FR-045).</summary>
        public virtual void RunOnce() { _cancel.ThrowIfCancelled(); _service.RunOnce(); }
        /// <summary>Returns the serial of the active scavenger bag (FR-045).</summary>
        public virtual uint GetScavengerBag() { _cancel.ThrowIfCancelled(); return _service.GetScavengerBag(); }
        /// <summary>Clears the ignore list and pending queue (FR-045).</summary>
        public virtual void ResetIgnore() { _cancel.ThrowIfCancelled(); _service.ResetIgnore(); }
    }

    /// <summary>Controls the Restock agent, which refills items from a vendor or container.</summary>
    public class RestockApi : AgentApiBase
    {
        private readonly IRestockService _service;
        public RestockApi(IRestockService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        /// <summary>Starts the Restock agent.</summary>
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        /// <summary>Stops the Restock agent.</summary>
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        /// <summary>Returns <c>true</c> if the Restock agent is currently running.</summary>
        public virtual bool Status() => _service.IsRunning;
        /// <summary>Switches the active restock list to the list with the given name.</summary>
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
        /// <summary>One-shot restock pass with explicit source/dest/delay (FR-043).</summary>
        public virtual void RunOnce(string listName, uint sourceSerial = 0, uint destSerial = 0, int delayMs = 0) { _cancel.ThrowIfCancelled(); _service.RunOnce(listName, sourceSerial, destSerial, delayMs); }
    }

    /// <summary>Controls the Organizer agent, which moves items between containers according to a rule list.</summary>
    public class OrganizerApi : AgentApiBase
    {
        private readonly IOrganizerService _service;
        public OrganizerApi(IOrganizerService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        /// <summary>Starts the Organizer agent.</summary>
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        /// <summary>Stops the Organizer agent.</summary>
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        /// <summary>Returns <c>true</c> if the Organizer agent is currently running.</summary>
        public virtual bool Status() => _service.IsRunning;
        /// <summary>Switches the active organizer list to the list with the given name.</summary>
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
        /// <summary>One-shot organize pass with explicit source/dest/delay (FR-042).</summary>
        public virtual void RunOnce(string listName, uint sourceSerial = 0, uint destSerial = 0, int delayMs = 0) { _cancel.ThrowIfCancelled(); _service.RunOnce(listName, sourceSerial, destSerial, delayMs); }
    }

    /// <summary>Controls the BandageHeal agent, which automatically applies bandages to heal the player or a target.</summary>
    public class BandageHealApi : AgentApiBase
    {
        private readonly IBandageHealService _service;
        public BandageHealApi(IBandageHealService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        /// <summary>Starts the BandageHeal agent.</summary>
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        /// <summary>Stops the BandageHeal agent.</summary>
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        /// <summary>Returns <c>true</c> if the BandageHeal agent is currently running.</summary>
        public virtual bool Status() => _service.IsRunning;
    }

    /// <summary>Controls vendor buy/sell operations for the VendorBuy and VendorSell agents.</summary>
    public class VendorApi : AgentApiBase
    {
        private readonly IVendorService _service;
        public VendorApi(IVendorService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        /// <summary>Starts the Vendor agent.</summary>
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        /// <summary>Stops the Vendor agent.</summary>
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        /// <summary>Returns <c>true</c> if the Vendor agent is currently running.</summary>
        public virtual bool Status() => _service.IsRunning;
        /// <summary>Sets the active buy list by name.</summary>
        public virtual void SetBuyList(string name) { _cancel.ThrowIfCancelled(); _service.SetBuyList(name); }
        /// <summary>Sets the active sell list by name.</summary>
        public virtual void SetSellList(string name) { _cancel.ThrowIfCancelled(); _service.SetSellList(name); }
        /// <summary>Clears the current buy list.</summary>
        public virtual void ClearBuyList() { _cancel.ThrowIfCancelled(); _service.ClearBuyList(); }
        /// <summary>Clears the current sell list.</summary>
        public virtual void ClearSellList() { _cancel.ThrowIfCancelled(); _service.ClearSellList(); }
        /// <summary>Script-driven buy by graphic ID (FR-047).</summary>
        public virtual void Buy(uint vendorSerial, int itemID, int amount, int maxPrice = 0) { _cancel.ThrowIfCancelled(); _service.Buy(vendorSerial, itemID, amount, maxPrice); }
        /// <summary>Script-driven buy by item name (FR-047).</summary>
        public virtual void Buy(uint vendorSerial, string itemName, int amount, int maxPrice = 0) { _cancel.ThrowIfCancelled(); _service.Buy(vendorSerial, itemName, amount, maxPrice); }
        /// <summary>Returns the items in the last-opened vendor container with prices (FR-047).</summary>
        public virtual List<(string Name, int Graphic, uint Price)> BuyList(uint vendorSerial) { _cancel.ThrowIfCancelled(); return _service.BuyList(vendorSerial); }
    }
}

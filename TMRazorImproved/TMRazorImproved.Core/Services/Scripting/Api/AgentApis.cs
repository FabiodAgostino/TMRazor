using System;
using System.Collections.Generic;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;

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
    }
}

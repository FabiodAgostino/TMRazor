using System;
using System.Collections.Generic;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public abstract class AgentApiBase
    {
        protected readonly ScriptCancellationController _cancel;
        public AgentApiBase(ScriptCancellationController cancel) { _cancel = cancel; }
    }

    public class AutoLootApi : AgentApiBase
    {
        private readonly IAutoLootService _service;
        public AutoLootApi(IAutoLootService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        public virtual bool Status() => _service.IsRunning;
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
    }

    public class DressApi : AgentApiBase
    {
        private readonly IDressService _service;
        public DressApi(IDressService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        public virtual bool Status() => _service.IsRunning;
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
        public virtual void DressUp() { _cancel.ThrowIfCancelled(); _service.DressUp(); }
        public virtual void Undress() { _cancel.ThrowIfCancelled(); _service.Undress(); }
    }

    public class ScavengerApi : AgentApiBase
    {
        private readonly IScavengerService _service;
        public ScavengerApi(IScavengerService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        public virtual bool Status() => _service.IsRunning;
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
    }

    public class RestockApi : AgentApiBase
    {
        private readonly IRestockService _service;
        public RestockApi(IRestockService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        public virtual bool Status() => _service.IsRunning;
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
    }

    public class OrganizerApi : AgentApiBase
    {
        private readonly IOrganizerService _service;
        public OrganizerApi(IOrganizerService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        public virtual bool Status() => _service.IsRunning;
        public virtual void ChangeList(string name) { _cancel.ThrowIfCancelled(); _service.ChangeList(name); }
    }

    public class BandageHealApi : AgentApiBase
    {
        private readonly IBandageHealService _service;
        public BandageHealApi(IBandageHealService service, ScriptCancellationController cancel) : base(cancel) { _service = service; }
        public virtual void Start() { _cancel.ThrowIfCancelled(); _service.Start(); }
        public virtual void Stop() { _cancel.ThrowIfCancelled(); _ = _service.StopAsync(); }
        public virtual bool Status() => _service.IsRunning;
    }
}

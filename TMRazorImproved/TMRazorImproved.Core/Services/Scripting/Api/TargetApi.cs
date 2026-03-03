using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class TargetApi
    {
        private readonly ITargetingService _targeting;
        private readonly ScriptCancellationController _cancel;

        public TargetApi(ITargetingService targeting, ScriptCancellationController cancel)
        {
            _targeting = targeting;
            _cancel = cancel;
        }

        public virtual void Self() => _targeting.TargetSelf();
        public virtual void Last() => _targeting.SendTarget(_targeting.LastTarget);
        public virtual void Cancel() => _targeting.CancelTarget();
        public virtual void WaitForTarget(int timeout = 5000)
        {
            // Attesa bloccante (gestita in MiscApi)
        }

        public virtual uint GetLast() => _targeting.LastTarget;
        public virtual bool HasTarget() => false; // TODO: Implementare flag in targeting service
        
        public virtual void TargetExecute(uint serial) => _targeting.SendTarget(serial);
        public virtual void TargetExecute(int x, int y, int z, int graphic) 
            => _targeting.SendTarget(0, (ushort)x, (ushort)y, (sbyte)z, (ushort)graphic);
    }
}

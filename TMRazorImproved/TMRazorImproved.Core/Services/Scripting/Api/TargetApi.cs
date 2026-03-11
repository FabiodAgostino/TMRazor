using System;
using System.Threading;
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

        public virtual void Self()
        {
            _cancel.ThrowIfCancelled();
            _targeting.TargetSelf();
        }

        public virtual void Last()
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(_targeting.LastTarget);
        }

        public virtual void Cancel()
        {
            _cancel.ThrowIfCancelled();
            _targeting.CancelTarget();
        }

        /// <summary>
        /// Attende che il server invii un target cursor (0x6C S2C) fino a <paramref name="timeout"/> ms.
        /// Ritorna true se il cursor è arrivato, false se scaduto il timeout.
        /// </summary>
        public virtual bool WaitForTarget(int timeout = 5000)
        {
            _cancel.ThrowIfCancelled();

            // Il cursore è già pendente — ritorna subito
            if (_targeting.HasTargetCursor) return true;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action<uint> handler = _ => tcs.TrySetResult(true);

            _targeting.TargetCursorRequested += handler;
            try
            {
                var deadline = Environment.TickCount64 + timeout;
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (_targeting.HasTargetCursor || tcs.Task.IsCompleted) return true;
                    Thread.Sleep(50);
                }
                return _targeting.HasTargetCursor;
            }
            finally
            {
                _targeting.TargetCursorRequested -= handler;
            }
        }

        /// <summary>Ritorna true se c'è un target cursor S2C attivo inviato dal server e non ancora consumato.</summary>
        public virtual bool HasTarget() => _targeting.HasTargetCursor;

        public virtual uint GetLast() => _targeting.LastTarget;
        public virtual void SetLastTarget(uint serial) => _targeting.LastTarget = serial;

        public virtual void TargetExecute(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(serial);
        }

        public virtual void TargetExecute(int x, int y, int z, int graphic)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(0, (ushort)x, (ushort)y, (sbyte)z, (ushort)graphic);
        }

        public virtual bool HasPrompt() => _targeting.HasPrompt;

        public virtual bool WaitForPrompt(int timeout = 5000)
        {
            if (_targeting.HasPrompt) return true;

            var tcs = new TaskCompletionSource<bool>();
            Action<bool> handler = (hasPrompt) => { if (hasPrompt) tcs.TrySetResult(true); };

            _targeting.PromptChanged += handler;
            try
            {
                var deadline = Environment.TickCount64 + timeout;
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (_targeting.HasPrompt) return true;
                    Thread.Sleep(50);
                }
                return _targeting.HasPrompt;
            }
            finally
            {
                _targeting.PromptChanged -= handler;
            }
        }

        public virtual void SendPrompt(string text)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendPrompt(text);
        }

        /// <summary>Invia un target di tipo land/ground alle coordinate specificate (z=0 se non noto).</summary>
        public virtual void TargetXYZ(int x, int y, int z = 0, int graphic = 0)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(0, (ushort)x, (ushort)y, (sbyte)z, (ushort)graphic);
        }

        /// <summary>
        /// Attende un target cursor poi lo invia alla tile ground specificata.
        /// </summary>
        public virtual bool TargetGround(int x, int y, int z = 0, int timeoutMs = 5000)
        {
            if (!WaitForTarget(timeoutMs)) return false;
            TargetXYZ(x, y, z);
            return true;
        }

        /// <summary>
        /// Avanza al prossimo target nella coda. Ritorna il cursore inviato (0 se nessun target in coda).
        /// </summary>
        public virtual void TargetNext()
        {
            _cancel.ThrowIfCancelled();
            _targeting.TargetNext();
        }

        // --- Migrated Missing APIs ---

        public virtual void AttackTargetFromList(string target_name) { throw new NotImplementedException("Target GUI filters not fully implemented yet."); }
        
        public virtual void ClearLast()
        {
            _cancel.ThrowIfCancelled();
            _targeting.LastTarget = 0;
        }

        public virtual void ClearLastandQueue()
        {
            _cancel.ThrowIfCancelled();
            _targeting.LastTarget = 0;
            _targeting.Clear();
        }

        public virtual void ClearLastAttack() { /* Need LastAttack property in ITargetingService */ throw new NotImplementedException(); }

        public virtual void ClearQueue()
        {
            _cancel.ThrowIfCancelled();
            _targeting.Clear();
        }

        public virtual int GetLastAttack() { throw new NotImplementedException(); }

        public virtual object GetTargetFromList(string target_name) { throw new NotImplementedException("Target GUI filters not fully implemented yet."); }

        public virtual void LastQueued()
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(_targeting.LastTarget); // Queuing system to be fully implemented
        }

        public virtual int LastUsedObject() { throw new NotImplementedException(); }

        public virtual void PerformTargetFromList(string target_name) { throw new NotImplementedException("Target GUI filters not fully implemented yet."); }

        public virtual object PromptGroundTarget(string message = "Select Ground Position", int color = 945)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendPrompt(message);
            throw new NotImplementedException("Full ground target prompting requires UI/Point3D integration.");
        }

        public virtual int PromptTarget(string message = "Select Item or Mobile", int color = 945)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendPrompt(message);
            throw new NotImplementedException("Full target prompting requires UI integration.");
        }

        public virtual void SelfQueued()
        {
            _cancel.ThrowIfCancelled();
            _targeting.TargetSelf(); // Queuing system to be fully implemented
        }

        public virtual void SetLast(int serial, bool wait = true)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SetLastTarget((uint)serial);
        }

        public virtual void SetLast(object mob) { throw new NotImplementedException("Requires Mobile wrapper resolution."); }

        public virtual void SetLastTargetFromList(string target_name) { throw new NotImplementedException("Target GUI filters not fully implemented yet."); }

        public virtual void TargetExecuteRelative(int serial, int offset) { throw new NotImplementedException("Requires Mobile wrapper and Position resolution."); }

        public virtual void TargetExecuteRelative(object mobile, int offset) { throw new NotImplementedException("Requires Mobile wrapper and Position resolution."); }

        public virtual void TargetResource(int item_serial, int resource_number) { throw new NotImplementedException(); }

        public virtual void TargetResource(int item_serial, string resource_name) { throw new NotImplementedException(); }

        public virtual void TargetResource(object item, string resource_name) { throw new NotImplementedException(); }

        public virtual void TargetResource(object item, int resource_number) { throw new NotImplementedException(); }

        public virtual bool TargetType(int graphic, int color = -1, int range = 20, string selector = "Nearest", System.Collections.Generic.List<byte> notoriety = null)
        {
            throw new NotImplementedException("Requires Item/Mobile search and filtering implementation.");
        }

        public virtual bool WaitForTargetOrFizzle(int delay = 5000, bool noshow = false)
        {
            _cancel.ThrowIfCancelled();
            // Basic fallback to WaitForTarget
            return WaitForTarget(delay);
        }
    }
}

using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class DPSMeterApi
    {
        private readonly IDPSMeterService _dps;
        private readonly ScriptCancellationController _cancel;

        public DPSMeterApi(IDPSMeterService dps, ScriptCancellationController cancel)
        {
            _dps = dps;
            _cancel = cancel;
        }

        /// <summary>DPS calcolato sulla finestra mobile degli ultimi 10 secondi.</summary>
        public virtual double GetCurrentDPS()
        {
            _cancel.ThrowIfCancelled();
            return _dps.CurrentDPS;
        }

        /// <summary>DPS massimo registrato dall'ultimo Reset().</summary>
        public virtual double GetMaxDPS()
        {
            _cancel.ThrowIfCancelled();
            return _dps.MaxDPS;
        }

        /// <summary>Danno totale inflitto dall'ultimo Reset().</summary>
        public virtual long GetTotalDamage()
        {
            _cancel.ThrowIfCancelled();
            return _dps.TotalDamage;
        }

        /// <summary>Secondi trascorsi in combattimento dall'ultimo Reset().</summary>
        public virtual double GetCombatTime()
        {
            _cancel.ThrowIfCancelled();
            return _dps.CombatTime.TotalSeconds;
        }

        /// <summary>True se il DPS meter è attivo (combattimento in corso).</summary>
        public virtual bool IsActive()
        {
            _cancel.ThrowIfCancelled();
            return _dps.IsActive;
        }

        /// <summary>Avvia il DPS meter manualmente.</summary>
        public virtual void Start()
        {
            _cancel.ThrowIfCancelled();
            _dps.Start();
        }

        /// <summary>Ferma il DPS meter.</summary>
        public virtual void Stop()
        {
            _cancel.ThrowIfCancelled();
            _dps.Stop();
        }

        /// <summary>Azzera tutte le statistiche e ferma il meter.</summary>
        public virtual void Reset()
        {
            _cancel.ThrowIfCancelled();
            _dps.Reset();
        }
    }
}

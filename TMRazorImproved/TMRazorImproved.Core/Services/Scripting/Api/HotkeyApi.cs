using System;
using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Core.Services.Scripting;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API per la gestione degli Hotkey da script.
    /// </summary>
    public class HotkeyApi
    {
        private readonly IHotkeyService _hotkeyService;
        private readonly IConfigService _config;
        private readonly ScriptCancellationController _cancel;

        public HotkeyApi(IHotkeyService hotkeyService, IConfigService config, ScriptCancellationController cancel)
        {
            _hotkeyService = hotkeyService;
            _config = config;
            _cancel = cancel;
        }

        /// <summary>
        /// Restituisce la lista di tutti gli hotkey (nomi delle azioni) configurati.
        /// </summary>
        public virtual List<string> Get()
        {
            _cancel.ThrowIfCancelled();
            return _config.CurrentProfile?.Hotkeys.Select(h => h.Action).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Restituisce lo stato di abilitazione di un hotkey specifico per nome azione.
        /// Se il nome non viene trovato, restituisce lo stato globale del servizio hotkey.
        /// </summary>
        public virtual bool GetStatus(string actionName)
        {
            _cancel.ThrowIfCancelled();
            var hk = _config.CurrentProfile?.Hotkeys.FirstOrDefault(h => string.Equals(h.Action, actionName, StringComparison.OrdinalIgnoreCase));
            if (hk != null)
                return hk.Enabled && _hotkeyService.IsEnabled;
            
            return _hotkeyService.IsEnabled;
        }

        /// <summary>
        /// Abilita o disabilita un hotkey specifico per nome azione.
        /// Se actionName è "Master" o vuoto, agisce sullo stato globale del servizio.
        /// </summary>
        public virtual void SetStatus(string actionName, bool enabled)
        {
            _cancel.ThrowIfCancelled();
            if (string.IsNullOrEmpty(actionName) || string.Equals(actionName, "Master", StringComparison.OrdinalIgnoreCase))
            {
                _hotkeyService.IsEnabled = enabled;
                return;
            }

            var hk = _config.CurrentProfile?.Hotkeys.FirstOrDefault(h => string.Equals(h.Action, actionName, StringComparison.OrdinalIgnoreCase));
            if (hk != null)
            {
                hk.Enabled = enabled;
            }
        }

        /// <summary>
        /// Restituisce il tasto (KeyCode) associato a un'azione.
        /// </summary>
        public virtual int GetKey(string actionName)
        {
            _cancel.ThrowIfCancelled();
            var hk = _config.CurrentProfile?.Hotkeys.FirstOrDefault(h => string.Equals(h.Action, actionName, StringComparison.OrdinalIgnoreCase));
            return hk?.KeyCode ?? 0;
        }

        /// <summary>
        /// Restituisce una rappresentazione testuale del tasto associato a un'azione.
        /// </summary>
        public virtual string KeyString(string actionName)
        {
            _cancel.ThrowIfCancelled();
            var hk = _config.CurrentProfile?.Hotkeys.FirstOrDefault(h => string.Equals(h.Action, actionName, StringComparison.OrdinalIgnoreCase));
            if (hk == null || hk.KeyCode == 0) return "None";

            List<string> mods = new List<string>();
            if (hk.Ctrl) mods.Add("Ctrl");
            if (hk.Alt) mods.Add("Alt");
            if (hk.Shift) mods.Add("Shift");
            
            string keyName = ((System.Windows.Forms.Keys)hk.KeyCode).ToString();
            if (mods.Count > 0)
                return $"{string.Join("+", mods)}+{keyName}";
            
            return keyName;
        }
    }
}

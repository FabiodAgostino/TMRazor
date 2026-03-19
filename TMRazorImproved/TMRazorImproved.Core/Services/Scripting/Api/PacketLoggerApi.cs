using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class PacketLoggerApi
    {
        private readonly IPacketLoggerService _logger;
        private readonly ScriptCancellationController _cancel;

        public PacketLoggerApi(IPacketLoggerService logger, ScriptCancellationController cancel)
        {
            _logger = logger;
            _cancel = cancel;
        }

        /// <summary>True se la registrazione pacchetti è attiva.</summary>
        public virtual bool IsRecording()
        {
            _cancel.ThrowIfCancelled();
            return _logger.IsRecording;
        }

        /// <summary>Percorso del file di log corrente.</summary>
        public virtual string GetOutputPath()
        {
            _cancel.ThrowIfCancelled();
            return _logger.OutputPath;
        }

        /// <summary>Imposta il percorso del file di log.</summary>
        public virtual void SetOutputPath(string path)
        {
            _cancel.ThrowIfCancelled();
            _logger.OutputPath = path;
        }

        /// <summary>Avvia la registrazione. Se append è true aggiunge al file esistente.</summary>
        public virtual void Start(bool append = false)
        {
            _cancel.ThrowIfCancelled();
            _logger.StartRecording(append);
        }

        /// <summary>Ferma la registrazione.</summary>
        public virtual void Stop()
        {
            _cancel.ThrowIfCancelled();
            _logger.StopRecording();
        }

        /// <summary>Aggiunge un ID pacchetto alla blacklist (non viene loggato).</summary>
        public virtual void AddBlacklist(int packetId)
        {
            _cancel.ThrowIfCancelled();
            _logger.AddBlacklist(packetId);
        }

        /// <summary>Rimuove un ID pacchetto dalla blacklist.</summary>
        public virtual void RemoveBlacklist(int packetId)
        {
            _cancel.ThrowIfCancelled();
            _logger.RemoveBlacklist(packetId);
        }

        /// <summary>Svuota la blacklist.</summary>
        public virtual void ClearBlacklist()
        {
            _cancel.ThrowIfCancelled();
            _logger.ClearBlacklist();
        }

        /// <summary>Aggiunge un ID pacchetto alla whitelist (solo questi vengono loggati).</summary>
        public virtual void AddWhitelist(int packetId)
        {
            _cancel.ThrowIfCancelled();
            _logger.AddWhitelist(packetId);
        }

        /// <summary>Rimuove un ID pacchetto dalla whitelist.</summary>
        public virtual void RemoveWhitelist(int packetId)
        {
            _cancel.ThrowIfCancelled();
            _logger.RemoveWhitelist(packetId);
        }

        /// <summary>Svuota la whitelist (torna a loggare tutto, meno la blacklist).</summary>
        public virtual void ClearWhitelist()
        {
            _cancel.ThrowIfCancelled();
            _logger.ClearWhitelist();
        }

        /// <summary>
        /// Abilita o disabilita il logging per una direzione pacchetti.
        /// path: 0 = ClientToServer, 1 = ServerToClient
        /// </summary>
        public virtual void ListenPath(int path, bool active)
        {
            _cancel.ThrowIfCancelled();
            _logger.ListenPacketPath((PacketPath)path, active);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// FR-074: Servizio di memorizzazione password per auto-login.
    /// Le password vengono cifrate con DPAPI (Windows ProtectedData, ambito CurrentUser)
    /// e persistite nella GlobalSettings tramite IConfigService.
    /// </summary>
    public class PasswordService
    {
        private readonly IConfigService _config;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(IConfigService config, ILogger<PasswordService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>Salva le credenziali per un dato host:port/username. Sovrascrive se già presente.</summary>
        public void Save(string host, int port, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username))
                return;

            var encrypted = EncryptPassword(password);
            var creds = _config.Global.SavedCredentials;

            var existing = creds.FirstOrDefault(c =>
                string.Equals(c.Host, host, StringComparison.OrdinalIgnoreCase) &&
                c.Port == port &&
                string.Equals(c.Username, username, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.EncryptedPassword = encrypted;
            }
            else
            {
                creds.Add(new SavedCredential { Host = host, Port = port, Username = username, EncryptedPassword = encrypted });
            }

            _config.Save();
        }

        /// <summary>Recupera la password in chiaro per host:port/username. Null se non trovata o errore decifratura.</summary>
        public string? Get(string host, int port, string username)
        {
            var entry = _config.Global.SavedCredentials.FirstOrDefault(c =>
                string.Equals(c.Host, host, StringComparison.OrdinalIgnoreCase) &&
                c.Port == port &&
                string.Equals(c.Username, username, StringComparison.OrdinalIgnoreCase));

            if (entry == null || string.IsNullOrEmpty(entry.EncryptedPassword))
                return null;

            return DecryptPassword(entry.EncryptedPassword);
        }

        /// <summary>Rimuove le credenziali per host:port/username.</summary>
        public void Remove(string host, int port, string username)
        {
            var creds = _config.Global.SavedCredentials;
            creds.RemoveAll(c =>
                string.Equals(c.Host, host, StringComparison.OrdinalIgnoreCase) &&
                c.Port == port &&
                string.Equals(c.Username, username, StringComparison.OrdinalIgnoreCase));
            _config.Save();
        }

        /// <summary>Restituisce tutti gli username salvati per un dato host:port.</summary>
        public IReadOnlyList<string> GetUsernames(string host, int port)
        {
            return _config.Global.SavedCredentials
                .Where(c => string.Equals(c.Host, host, StringComparison.OrdinalIgnoreCase) && c.Port == port)
                .Select(c => c.Username)
                .ToList();
        }

        // ── Encryption helpers ──────────────────────────────────────────────────

        private string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            try
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PasswordService] Encrypt failed, storing empty.");
                return string.Empty;
            }
        }

        private string? DecryptPassword(string encryptedBase64)
        {
            if (string.IsNullOrEmpty(encryptedBase64)) return null;
            try
            {
                var encrypted = Convert.FromBase64String(encryptedBase64);
                var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PasswordService] Decrypt failed (key may have changed).");
                return null;
            }
        }
    }
}

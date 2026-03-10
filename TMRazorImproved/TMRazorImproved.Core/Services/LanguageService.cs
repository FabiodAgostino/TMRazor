using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Threading;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly ResourceManager _resourceManager;
        private Ultima.StringList? _cliloc;
        private CultureInfo _currentCulture = new CultureInfo("en-US");

        public string CurrentLanguage { get; private set; } = "en";

        public LanguageService()
        {
            // Inizializza il ResourceManager puntando ai file .resx nel progetto Shared
            _resourceManager = new ResourceManager("TMRazorImproved.Shared.Resources.Strings", typeof(TMRazorImproved.Shared.Models.Config.GlobalSettings).Assembly);
        }

        public string GetString(LocString key) => GetString(key.ToString());

        public string GetString(int key) => GetString(key.ToString());

        public string GetString(string key)
        {
            try
            {
                string? val = _resourceManager.GetString(key, _currentCulture);
                return val ?? $"[{key}]";
            }
            catch
            {
                return $"[{key}]";
            }
        }

        public string GetCliloc(int clilocId)
        {
            return _cliloc?.GetString(clilocId) ?? $"[Cliloc:{clilocId}]";
        }

        public string ClilocFormat(int clilocId, string args)
        {
            var entry = _cliloc?.GetEntry(clilocId);
            return entry?.SplitFormat(args) ?? $"[Cliloc:{clilocId}]";
        }

        public void Load(string langCode)
        {
            CurrentLanguage = langCode;
            
            // Imposta la cultura del thread per il ResourceManager
            _currentCulture = langCode switch
            {
                "it" => new CultureInfo("it-IT"),
                "es" => new CultureInfo("es-ES"),
                _ => new CultureInfo("en-US")
            };
            Thread.CurrentThread.CurrentUICulture = _currentCulture;
            Thread.CurrentThread.CurrentCulture = _currentCulture;

            // Caricamento Cliloc tramite UltimaSDK
            try
            {
                _cliloc = new Ultima.StringList(langCode, IsCompressedCli(langCode));
            }
            catch { _cliloc = null; }
        }

        private bool IsCompressedCli(string lang)
        {
            string filePath = Ultima.Files.GetFilePath($"cliloc.{lang}");
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return false;

            try
            {
                using var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                if (stream.Length < 4) return false;
                stream.Position = 3;
                int value = stream.ReadByte();
                return value == 0x8E;
            }
            catch { return false; }
        }

        public IEnumerable<string> GetAvailableLanguages()
        {
            yield return "en";
            yield return "it";
            yield return "es";
        }
    }
}

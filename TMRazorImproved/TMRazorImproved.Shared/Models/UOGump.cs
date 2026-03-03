using System;
using System.Collections.Generic;

namespace TMRazorImproved.Shared.Models
{
    /// <summary>
    /// Rappresenta un Gump (finestra di gioco) aperto.
    /// Dopo Freeze() è thread-safe (read-only).
    /// </summary>
    public class UOGump
    {
        public uint Serial { get; }
        public uint GumpId { get; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Layout { get; set; } = string.Empty;

        // Inizialmente una lista mutabile interna per la fase di costruzione.
        // Dopo Freeze(), diventa immutabile (ReadOnlyList).
        private List<string> _strings = new();
        private IReadOnlyList<string>? _frozenStrings;

        /// <summary>
        /// Stringhe del gump. Dopo Freeze() è thread-safe (read-only).
        /// </summary>
        public IReadOnlyList<string> Strings => _frozenStrings ?? _strings;

        public UOGump(uint serial, uint gumpId)
        {
            Serial = serial;
            GumpId = gumpId;
        }

        /// <summary>Aggiunge una stringa durante la costruzione (pre-Freeze).</summary>
        public void AddString(string s) => _strings.Add(s);

        /// <summary>Congela l'oggetto: le stringhe diventano read-only e thread-safe.</summary>
        public void Freeze() => _frozenStrings = _strings.AsReadOnly();
    }

    /// <summary>
    /// Rappresenta una singola proprietà di un oggetto (Cliloc).
    /// </summary>
    public record UOPropertyEntry(int Number, string Arguments);

    /// <summary>
    /// Lista delle proprietà di un oggetto (Object Property List - OPL).
    /// Dopo Freeze() è thread-safe (read-only).
    /// </summary>
    public class UOPropertyList
    {
        public uint Serial { get; }
        public int Hash { get; set; }

        private List<UOPropertyEntry> _properties = new();
        private IReadOnlyList<UOPropertyEntry>? _frozenProperties;

        public IReadOnlyList<UOPropertyEntry> Properties => _frozenProperties ?? _properties;

        public UOPropertyList(uint serial)
        {
            Serial = serial;
        }

        /// <summary>Aggiunge una proprietà durante la costruzione (pre-Freeze).</summary>
        public void AddProperty(UOPropertyEntry entry) => _properties.Add(entry);

        /// <summary>Congela l'oggetto: le proprietà diventano read-only e thread-safe.</summary>
        public void Freeze() => _frozenProperties = _properties.AsReadOnly();

        /// <summary>
        /// Ritorna il nome dell'oggetto (primo campo Arguments della prima property, cliloc 1050045/1042971)
        /// oppure stringa vuota se non disponibile.
        /// </summary>
        public string GetNameOrEmpty()
            => Properties.Count > 0 ? Properties[0].Arguments : string.Empty;
    }
}

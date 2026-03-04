using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        private List<GumpControl> _controls = new();
        private IReadOnlyList<GumpControl>? _frozenControls;

        /// <summary>
        /// Stringhe del gump. Dopo Freeze() è thread-safe (read-only).
        /// </summary>
        public IReadOnlyList<string> Strings => _frozenStrings ?? _strings;

        /// <summary>
        /// Controlli del gump (pulsanti, testo, ecc.). Dopo Freeze() è thread-safe.
        /// </summary>
        public IReadOnlyList<GumpControl> Controls => _frozenControls ?? _controls;

        /// <summary>
        /// Lista dei ButtonId presenti nel gump.
        /// </summary>
        public List<int> Buttons 
        { 
            get 
            {
                var list = new List<int>();
                foreach (var c in Controls)
                {
                    if (c is GumpButton b) list.Add(b.ButtonId);
                }
                return list;
            }
        }

        /// <summary>
        /// Alias per Strings, usato per parità con GumpData di Razor Enhanced.
        /// </summary>
        public IReadOnlyList<string> Texts => Strings;

        public UOGump(uint serial, uint gumpId)
        {
            Serial = serial;
            GumpId = gumpId;
        }

        /// <summary>Aggiunge una stringa durante la costruzione (pre-Freeze).</summary>
        public void AddString(string s) => _strings.Add(s);

        /// <summary>Congela l'oggetto: le stringhe diventano read-only e thread-safe.</summary>
        public void Freeze() 
        {
            ParseControls();
            _frozenStrings = _strings.AsReadOnly();
            _frozenControls = _controls.AsReadOnly();
        }

        private void ParseControls()
        {
            if (string.IsNullOrEmpty(Layout)) return;

            // Regex per i comandi principali del layout UO
            // Formato tipico: { command arg1 arg2 ... }
            var matches = Regex.Matches(Layout, @"\{?\s*(?<cmd>\w+)\s+(?<args>[^}]+)\}?");

            foreach (Match match in matches)
            {
                string cmd = match.Groups["cmd"].Value.ToLower();
                string[] args = match.Groups["args"].Value.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    switch (cmd)
                    {
                        case "button": // { button x y released pressed quit page id }
                            if (args.Length >= 7)
                            {
                                _controls.Add(new GumpButton
                                {
                                    Type = "Button",
                                    X = int.Parse(args[0]),
                                    Y = int.Parse(args[1]),
                                    ReleasedId = int.Parse(args[2]),
                                    PressedId = int.Parse(args[3]),
                                    Quit = int.Parse(args[4]),
                                    Page = int.Parse(args[5]),
                                    ButtonId = int.Parse(args[6])
                                });
                            }
                            break;

                        case "text": // { text x y color id }
                            if (args.Length >= 4)
                            {
                                int stringIndex = int.Parse(args[3]);
                                string content = (stringIndex >= 0 && stringIndex < _strings.Count) ? _strings[stringIndex] : "";
                                _controls.Add(new GumpText
                                {
                                    Type = "Text",
                                    X = int.Parse(args[0]),
                                    Y = int.Parse(args[1]),
                                    Color = int.Parse(args[2]),
                                    StringId = stringIndex,
                                    Text = content
                                });
                            }
                            break;

                        case "gumppic": // { gumppic x y id }
                        case "gumppictiled":
                            if (args.Length >= 3)
                            {
                                _controls.Add(new GumpImage
                                {
                                    Type = cmd == "gumppic" ? "Image" : "Tiled Image",
                                    X = int.Parse(args[0]),
                                    Y = int.Parse(args[1]),
                                    GumpId = int.Parse(args[2])
                                });
                            }
                            break;

                        case "resizepic": // { resizepic x y id w h }
                            if (args.Length >= 5)
                            {
                                _controls.Add(new GumpBackground
                                {
                                    Type = "Background",
                                    X = int.Parse(args[0]),
                                    Y = int.Parse(args[1]),
                                    GumpId = int.Parse(args[2]),
                                    Width = int.Parse(args[3]),
                                    Height = int.Parse(args[4])
                                });
                            }
                            break;

                        case "xmfhtmlgump": // { xmfhtmlgump x y w h cliloc background scroll }
                        case "xmfhtmlgumpcolor":
                            if (args.Length >= 5)
                            {
                                _controls.Add(new GumpLabel
                                {
                                    Type = "HTML Label",
                                    X = int.Parse(args[0]),
                                    Y = int.Parse(args[1]),
                                    ClilocId = int.Parse(args[4])
                                });
                            }
                            break;
                    }
                }
                catch { /* Ignora errori di parsing su singoli comandi malformati */ }
            }
        }
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

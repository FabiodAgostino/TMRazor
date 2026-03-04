using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMRazorImproved.Core.Services.Scripting.Api;

namespace TMRazorImproved.Core.Services.Scripting.Engines
{
    public class UOSteamInterpreter
    {
        private readonly MiscApi _misc;
        private readonly PlayerApi _player;
        private readonly ItemsApi _items;
        private readonly MobilesApi _mobiles;
        private readonly JournalApi _journal;
        private readonly ScriptCancellationController _cancel;
        private readonly Action<string> _output;

        private readonly Dictionary<string, uint> _aliases = new();
        private readonly Dictionary<string, List<uint>> _lists = new();
        private readonly Dictionary<string, long> _timers = new();
        private readonly Dictionary<string, string> _variables = new();
        private readonly Stack<int> _loopStack = new();
        private int _currentLineIndex;
        private string[] _lines = Array.Empty<string>();

        public UOSteamInterpreter(
            MiscApi misc, 
            PlayerApi player, 
            ItemsApi items, 
            MobilesApi mobiles,
            JournalApi journal,
            ScriptCancellationController cancel,
            Action<string> output)
        {
            _misc = misc;
            _player = player;
            _items = items;
            _mobiles = mobiles;
            _journal = journal;
            _cancel = cancel;
            _output = output;

            _aliases["self"] = 0;
            _aliases["backpack"] = 0;
            _aliases["lasttarget"] = 0;
        }

        public void Execute(string code)
        {
            _lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            _currentLineIndex = 0;
            _loopStack.Clear();

            while (_currentLineIndex < _lines.Length)
            {
                _cancel.ThrowIfCancelled();
                string line = _lines[_currentLineIndex].Trim();
                
                if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                {
                    _currentLineIndex++;
                    continue;
                }

                ExecuteLine(line);
                _currentLineIndex++;
            }
        }

        private void ExecuteLine(string line)
        {
            // Sostituzione variabili prima di eseguire la riga
            foreach (var kvp in _variables)
            {
                if (line.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    line = Regex.Replace(line, $@"\b{Regex.Escape(kvp.Key)}\b", kvp.Value, RegexOptions.IgnoreCase);
                }
            }

            // Regex migliorata per gestire 'stringhe' e "stringhe"
            var matches = Regex.Matches(line, @"(?<match>'[^']*')|(?<match>""[^""]*"")|(?<match>[^ ]+)");
            if (matches.Count == 0) return;

            string command = matches[0].Groups["match"].Value.ToLower();
            string[] args = new string[matches.Count - 1];
            for (int i = 1; i < matches.Count; i++)
            {
                args[i - 1] = matches[i].Groups["match"].Value.Trim('\'', '\"');
            }

            try
            {
                switch (command)
                {
                    case "if":
                        HandleIf(args);
                        break;
                    case "elseif":
                        SkipToBlockEnd("if", "endif");
                        break;
                    case "else":
                        SkipToBlockEnd("if", "endif");
                        break;
                    case "endif":
                        break;
                    case "while":
                        HandleWhile(args);
                        break;
                    case "endwhile":
                        if (_loopStack.Count > 0)
                        {
                            _currentLineIndex = _loopStack.Pop() - 1; 
                        }
                        break;
                    case "setvar":
                        if (args.Length >= 2) _variables[args[0].ToLower()] = args[1];
                        break;
                    case "clearvar":
                        if (args.Length > 0) _variables.Remove(args[0].ToLower());
                        break;
                    case "setalias":
                        if (args.Length >= 2) _aliases[args[0].ToLower()] = ParseSerial(args[1]);
                        break;
                    case "unsetalias":
                        if (args.Length > 0) _aliases.Remove(args[0].ToLower());
                        break;
                    case "pushlist":
                        if (args.Length >= 2)
                        {
                            var listName = args[0].ToLower();
                            if (!_lists.ContainsKey(listName)) _lists[listName] = new List<uint>();
                            _lists[listName].Add(ParseSerial(args[1]));
                        }
                        break;
                    case "poplist":
                        if (args.Length >= 2)
                        {
                            var listName = args[0].ToLower();
                            var destAlias = args[1].ToLower();
                            if (_lists.ContainsKey(listName) && _lists[listName].Count > 0)
                            {
                                _aliases[destAlias] = _lists[listName][0];
                                _lists[listName].RemoveAt(0);
                            }
                        }
                        break;
                    case "removelist":
                        if (args.Length > 0) _lists.Remove(args[0].ToLower());
                        break;
                    case "clearlist":
                        if (args.Length > 0 && _lists.ContainsKey(args[0].ToLower())) _lists[args[0].ToLower()].Clear();
                        break;
                    case "createtimer":
                        if (args.Length > 0) _timers[args[0].ToLower()] = Environment.TickCount64;
                        break;
                    case "settimer":
                        if (args.Length >= 2) _timers[args[0].ToLower()] = Environment.TickCount64 - ParseInt(args[1]);
                        break;
                    case "clearjournal":
                    case "clearsysmsg":
                        _journal.Clear();
                        break;
                    case "setability":
                        if (args.Length > 0) _player.SetAbility(args[0]);
                        break;
                    case "interrupt":
                        _player.Cast("clumsy"); // Un hack tipico per resettare il casting o target
                        break;
                    case "setoption":
                        if (args.Length >= 2) _variables[args[0].ToLower()] = args[1];
                        break;
                    case "overhead":
                        if (args.Length >= 1)
                        {
                            string msg = args[0];
                            int color = args.Length > 1 ? ParseInt(args[1]) : 945;
                            uint serial = args.Length > 2 ? ParseSerial(args[2]) : _player.Serial;
                            if (serial == _player.Serial) _player.HeadMsg(msg, color);
                        }
                        break;
                    case "msg":
                    case "say":
                        if (args.Length > 0) _player.ChatSay(args[0]);
                        break;
                    case "headmsg":
                        if (args.Length > 0) _player.HeadMsg(args[0], args.Length > 1 ? ParseInt(args[1]) : 945);
                        break;
                    case "pause":
                    case "wait":
                        if (args.Length > 0) _misc.Pause(ParseInt(args[0]));
                        break;
                    case "cast":
                        if (args.Length > 0) _player.Cast(args[0]);
                        break;
                    case "useobject":
                        if (args.Length > 0) _items.UseItem(ParseSerial(args[0]));
                        break;
                    case "targetself":
                        _player.TargetSelf();
                        break;
                    case "waitfortarget":
                        _misc.WaitForTarget(args.Length > 0 ? ParseInt(args[0]) : 5000);
                        break;
                    case "replay":
                        _currentLineIndex = -1; // Riavvia lo script
                        _loopStack.Clear();
                        break;
                    case "stop":
                        _currentLineIndex = _lines.Length; // Termina
                        break;
                    default:
                        EvaluateExpression(line);
                        break;
                }
            }
            catch (Exception ex)
            {
                _output?.Invoke($"[UOSteam Errore riga {_currentLineIndex + 1}] {ex.Message}");
            }
        }

        private void HandleIf(string[] args)
        {
            if (!EvaluateExpression(string.Join(" ", args)))
            {
                // Cerca elseif, else o endif allo stesso livello
                int depth = 0;
                while (_currentLineIndex < _lines.Length - 1)
                {
                    _currentLineIndex++;
                    string line = _lines[_currentLineIndex].Trim().ToLower();
                    if (line.StartsWith("if ") || line == "if") depth++;
                    else if (line == "endif")
                    {
                        if (depth == 0) break;
                        depth--;
                    }
                    else if (depth == 0 && (line.StartsWith("elseif ") || line == "else"))
                    {
                        _currentLineIndex--; // Torna indietro per processare elseif/else nel loop principale
                        break;
                    }
                }
            }
        }

        private void HandleWhile(string[] args)
        {
            if (EvaluateExpression(string.Join(" ", args)))
            {
                _loopStack.Push(_currentLineIndex);
            }
            else
            {
                SkipToBlockEnd("while", "endwhile");
            }
        }

        private void SkipToBlockEnd(string startCmd, string endCmd)
        {
            int depth = 0;
            while (_currentLineIndex < _lines.Length - 1)
            {
                _currentLineIndex++;
                string line = _lines[_currentLineIndex].Trim().ToLower();
                if (line.StartsWith(startCmd + " ") || line == startCmd) depth++;
                else if (line == endCmd)
                {
                    if (depth == 0) break;
                    depth--;
                }
            }
        }

        private bool EvaluateExpression(string expr)
        {
            expr = expr.ToLower().Trim();
            if (string.IsNullOrEmpty(expr)) return false;

            // Supporto OR / AND (semplificato: valuta da sinistra a destra)
            if (expr.Contains(" or "))
            {
                var parts = expr.Split(new[] { " or " }, StringSplitOptions.RemoveEmptyEntries);
                return parts.Any(EvaluateExpression);
            }
            if (expr.Contains(" and "))
            {
                var parts = expr.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
                return parts.All(EvaluateExpression);
            }

            // Operatori di confronto
            if (expr.Contains("==")) return Compare(expr, "==");
            if (expr.Contains("!=")) return Compare(expr, "!=");
            if (expr.Contains(">=")) return Compare(expr, ">=");
            if (expr.Contains("<=")) return Compare(expr, "<=");
            if (expr.Contains(">"))  return Compare(expr, ">");
            if (expr.Contains("<"))  return Compare(expr, "<");

            // Comandi di ricerca
            if (expr.StartsWith("findtype ")) return HandleFindType(expr);
            if (expr.StartsWith("counttype ")) return _items.GetBackpackItems().Count(i => i.Graphic == ParseInt(expr.Replace("counttype ", ""))) > 0;

            // Proprietà booleane
            switch (expr)
            {
                case "poisoned": return _player.IsPoisoned;
                case "dead": return _player.Hits <= 0;
                case "hidden": return false;
                case "mounted": return false;
                case "warmode": return false;
            }

            return false;
        }

        private bool HandleFindType(string expr)
        {
            // findtype graphic [hue] [container] [range]
            var parts = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return false;
            
            int graphic = ParseInt(parts[1]);
            int hue = parts.Length > 2 ? ParseInt(parts[2]) : -1;
            
            var item = _items.FindByID(graphic, hue);
            if (item != null)
            {
                _aliases["found"] = item.Serial;
                return true;
            }
            return false;
        }

        private bool Compare(string expr, string op)
        {
            string[] parts = expr.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            
            long left = GetValue(parts[0].Trim());
            long right = GetValue(parts[1].Trim());

            return op switch
            {
                "==" => left == right,
                "!=" => left != right,
                ">"  => left > right,
                "<"  => left < right,
                ">=" => left >= right,
                "<=" => left <= right,
                _ => false
            };
        }

        private long GetValue(string term)
        {
            if (long.TryParse(term, out long val)) return val;
            if (term.StartsWith("0x") && long.TryParse(term.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out long hexVal)) return hexVal;

            return term switch
            {
                "hits" => _player.Hits,
                "maxhits" => _player.HitsMax,
                "mana" => _player.Mana,
                "maxmana" => _player.ManaMax,
                "stam" => _player.Stam,
                "maxstam" => _player.StamMax,
                "serial" => _player.Serial,
                "weight" => 0,
                _ => ParseSerial(term) // Prova se è un alias
            };
        }

        private uint ParseSerial(string s)
        {
            s = s.ToLower();
            if (s == "self") return _player.Serial;
            if (s == "backpack" && _player.Backpack != null) return _player.Backpack.Serial;
            if (_aliases.TryGetValue(s, out uint serial)) return serial;
            
            if (s.StartsWith("0x") && uint.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out uint hex)) return hex;
            uint.TryParse(s, out uint dec);
            return dec;
        }

        private int ParseInt(string s)
        {
            if (s.StartsWith("0x") && int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int hex)) return hex;
            int.TryParse(s, out int dec);
            return dec;
        }
    }
}

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
        private readonly TargetApi _targetApi;
        private readonly SkillsApi _skillsApi;
        private readonly GumpsApi _gumpsApi;
        private readonly AutoLootApi _autoLootApi;
        private readonly DressApi _dressApi;
        private readonly ScavengerApi _scavengerApi;
        private readonly RestockApi _restockApi;
        private readonly OrganizerApi _organizerApi;
        private readonly BandageHealApi _bandageHealApi;
        private readonly HotkeyApi _hotkeyApi;
        private readonly ScriptCancellationController _cancel;
        private readonly Action<string> _output;

        private readonly Dictionary<string, uint> _aliases = new();
        private readonly Dictionary<string, List<uint>> _lists = new();
        private readonly Dictionary<string, long> _timers = new();
        private readonly Dictionary<string, string> _variables = new();
        private readonly Stack<int> _loopStack = new();
        private readonly Stack<(int lineIndex, int counter, int limit, string varName)> _forStack = new();
        private readonly Stack<bool> _ifSucceededStack = new(); // Traccia se un ramo if/elseif è già stato eseguito
        private static readonly Random _rng = new();
        private int _currentLineIndex;
        private string[] _lines = Array.Empty<string>();

        public UOSteamInterpreter(
            MiscApi misc, 
            PlayerApi player, 
            ItemsApi items, 
            MobilesApi mobiles,
            JournalApi journal,
            TargetApi targetApi,
            SkillsApi skillsApi,
            GumpsApi gumpsApi,
            AutoLootApi autoLootApi,
            DressApi dressApi,
            ScavengerApi scavengerApi,
            RestockApi restockApi,
            OrganizerApi organizerApi,
            BandageHealApi bandageHealApi,
            HotkeyApi hotkeyApi,
            ScriptCancellationController cancel,
            Action<string> output)
        {
            _misc = misc;
            _player = player;
            _items = items;
            _mobiles = mobiles;
            _journal = journal;
            _targetApi = targetApi;
            _skillsApi = skillsApi;
            _gumpsApi = gumpsApi;
            _autoLootApi = autoLootApi;
            _dressApi = dressApi;
            _scavengerApi = scavengerApi;
            _restockApi = restockApi;
            _organizerApi = organizerApi;
            _bandageHealApi = bandageHealApi;
            _hotkeyApi = hotkeyApi;
            _cancel = cancel;
            _output = output;

            _aliases["self"] = 0;
            _aliases["backpack"] = 0;
            _aliases["lasttarget"] = 0;
            _aliases["found"] = 0;
        }

        public void Execute(string code)
        {
            _lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            _currentLineIndex = 0;
            _loopStack.Clear();
            _forStack.Clear();
            _ifSucceededStack.Clear();

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
                        HandleElseIf(args);
                        break;
                    case "else":
                        HandleElse();
                        break;
                    case "endif":
                        if (_ifSucceededStack.Count > 0) _ifSucceededStack.Pop();
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
                    case "for":
                        HandleFor(args);
                        break;
                    case "endfor":
                        HandleEndFor();
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
                    case "removetimer":
                        if (args.Length > 0) _timers.Remove(args[0].ToLower());
                        break;
                    case "clearjournal":
                    case "clearsysmsg":
                        _journal.Clear();
                        break;
                    case "waitforjournal":
                        if (args.Length > 0)
                        {
                            int timeout = args.Length > 1 ? ParseInt(args[1]) : 5000;
                            _journal.WaitJournal(args[0], timeout);
                        }
                        break;
                    case "setability":
                        if (args.Length > 0) _player.SetAbility(args[0]);
                        break;
                    case "interrupt":
                        _player.Resync();
                        break;
                    case "resync":
                        _player.Resync();
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
                    case "sysmsg":
                        if (args.Length > 0) _output?.Invoke(args[0]);
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
                    case "waitforgump":
                        if (args.Length > 0) _misc.WaitGump(ParseSerial(args[0]), args.Length > 1 ? ParseInt(args[1]) : 5000);
                        break;
                    case "waitforcontents":
                    case "waitforcontainer":
                        if (args.Length > 0) _items.WaitForContents(ParseSerial(args[0]), args.Length > 1 ? ParseInt(args[1]) : 5000);
                        break;
                    case "replygump":
                    case "gumpresponse":
                        if (args.Length >= 2) _gumpsApi.SendAction(ParseSerial(args[0]), ParseInt(args[1]));
                        break;
                    case "closegump":
                        if (args.Length > 0) _gumpsApi.Close(ParseSerial(args[0]));
                        break;
                    case "random":
                        if (args.Length >= 2)
                            _variables[args[0].ToLower()] = _rng.Next(0, ParseInt(args[1]) + 1).ToString();
                        else if (args.Length == 1)
                            _variables["_random"] = _rng.Next(0, ParseInt(args[0]) + 1).ToString();
                        break;
                    case "target":
                        if (args.Length > 0)
                        {
                            _misc.WaitForTarget(500);
                            _targetApi.Target(ParseSerial(args[0]));
                        }
                        break;
                    case "attack":
                        if (args.Length > 0) _player.Attack(ParseSerial(args[0]));
                        break;
                    case "warmode":
                        if (args.Length > 0) _player.SetWarMode(args[0].Equals("on", StringComparison.OrdinalIgnoreCase));
                        break;
                    case "useskill":
                    case "skill":
                        if (args.Length > 0) _player.UseSkill(args[0]);
                        break;
                    case "dclick":
                    case "doubleclick":
                        if (args.Length > 0) _items.UseItem(ParseSerial(args[0]));
                        break;
                    case "click":
                    case "singleclick":
                        if (args.Length > 0) _items.Click(ParseSerial(args[0]));
                        break;
                    case "move":
                    case "moveitem":
                        if (args.Length >= 2) _items.Move(ParseSerial(args[0]), ParseSerial(args[1]), args.Length > 2 ? ParseInt(args[2]) : 1);
                        break;
                    case "lift":
                        if (args.Length > 0) _items.Lift(ParseSerial(args[0]), args.Length > 1 ? ParseInt(args[1]) : 1);
                        break;
                    case "drop":
                        if (args.Length >= 2) _items.Drop(ParseSerial(args[0]), ParseSerial(args[1]), args.Length > 2 ? ParseInt(args[2]) : 1);
                        break;
                    case "walk":
                        if (args.Length > 0) _player.Walk(args[0]);
                        break;
                    case "turn":
                        if (args.Length > 0) _player.Turn(args[0]);
                        break;
                    case "autoloot":
                        if (args.Length > 0 && args[0] == "off") _autoLootApi.Stop(); else _autoLootApi.Start();
                        break;
                    case "dress":
                        if (args.Length > 0) _dressApi.ChangeList(args[0]);
                        _dressApi.DressUp();
                        break;
                    case "undress":
                        if (args.Length > 0) _dressApi.ChangeList(args[0]);
                        _dressApi.Undress();
                        break;
                    case "organizer":
                        if (args.Length > 0) _organizerApi.ChangeList(args[0]);
                        _organizerApi.Start();
                        break;
                    case "restock":
                        if (args.Length > 0) _restockApi.ChangeList(args[0]);
                        _restockApi.Start();
                        break;
                    case "scavenger":
                        if (args.Length > 0 && args[0] == "off") _scavengerApi.Stop(); else _scavengerApi.Start();
                        break;
                    case "bandageheal":
                        if (args.Length > 0 && args[0] == "off") _bandageHealApi.Stop(); else _bandageHealApi.Start();
                        break;
                    case "playsound":
                        if (args.Length > 0) _misc.PlaySound(ParseInt(args[0]));
                        break;
                    case "playmusic":
                        if (args.Length > 0) _misc.PlayMusic(ParseInt(args[0]));
                        break;
                    case "stopmusic":
                    case "stopsound":
                        _misc.StopMusic();
                        break;
                    case "getlabel":
                        if (args.Length >= 2)
                        {
                            var item = _items.FindBySerial(ParseSerial(args[0]));
                            if (item != null) _variables[args[1].ToLower()] = string.Join(" ", item.Properties);
                        }
                        break;
                    case "replay":
                        _currentLineIndex = -1;
                        _loopStack.Clear();
                        _forStack.Clear();
                        _ifSucceededStack.Clear();
                        break;
                    case "stop":
                        _currentLineIndex = _lines.Length;
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
            bool success = EvaluateExpression(string.Join(" ", args));
            _ifSucceededStack.Push(success);
            if (!success)
            {
                SkipToNextConditionalBranch();
            }
        }

        private void HandleElseIf(string[] args)
        {
            if (_ifSucceededStack.Count == 0) return; // Errore: elseif senza if

            if (_ifSucceededStack.Peek())
            {
                // Un ramo precedente ha già avuto successo, salta tutto fino a endif
                SkipToBlockEnd("if", "endif");
            }
            else
            {
                // Prova questo ramo
                bool success = EvaluateExpression(string.Join(" ", args));
                if (success)
                {
                    _ifSucceededStack.Pop();
                    _ifSucceededStack.Push(true);
                }
                else
                {
                    SkipToNextConditionalBranch();
                }
            }
        }

        private void HandleElse()
        {
            if (_ifSucceededStack.Count == 0) return;

            if (_ifSucceededStack.Peek())
            {
                SkipToBlockEnd("if", "endif");
            }
            else
            {
                _ifSucceededStack.Pop();
                _ifSucceededStack.Push(true);
            }
        }

        private void SkipToNextConditionalBranch()
        {
            int depth = 0;
            while (_currentLineIndex < _lines.Length - 1)
            {
                _currentLineIndex++;
                string line = _lines[_currentLineIndex].Trim().ToLower();
                if (line.StartsWith("if ") || line == "if") depth++;
                else if (line == "endif")
                {
                    if (depth == 0)
                    {
                        _currentLineIndex--; // Torna indietro per far processare endif nel loop principale
                        break;
                    }
                    depth--;
                }
                else if (depth == 0 && (line.StartsWith("elseif ") || line == "else"))
                {
                    _currentLineIndex--; // Torna indietro per far processare elseif/else nel loop principale
                    break;
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

            // Supporto NOT (!)
            if (expr.StartsWith("not ") || expr.StartsWith("! "))
            {
                return !EvaluateExpression(expr.Substring(expr.StartsWith("!") ? 1 : 4).Trim());
            }

            // Supporto OR / AND (valutazione semplice da sinistra a destra)
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

            // Comandi di ricerca/query
            if (expr.StartsWith("findtype ")) return HandleFindType(expr);
            if (expr.StartsWith("counttype ")) return HandleCountType(expr);
            if (expr.StartsWith("injournal ")) return _journal.InJournal(expr.Substring(10).Trim('\'', '"'));
            if (expr.StartsWith("injournalline ")) return _journal.InJournal(expr.Substring(14).Trim('\'', '"'));
            if (expr.StartsWith("gumpexists "))
            {
                uint gumpId = ParseSerial(expr.Substring(11).Trim());
                return _misc.WaitGump(gumpId, 50);
            }
            if (expr.StartsWith("ingump ")) return _misc.WaitForGumpAny(50);
            if (expr.StartsWith("timer ") || expr.StartsWith("timername "))
            {
                var parts2 = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts2.Length >= 2)
                {
                    string tname = parts2[1].ToLower();
                    if (_timers.TryGetValue(tname, out long t))
                    {
                        long elapsed = (long)Environment.TickCount64 - t;
                        if (parts2.Length >= 4)
                        {
                            long val = ParseInt(parts2[3]);
                            string op = parts2[2];
                            return op switch { ">" => elapsed > val, ">=" => elapsed >= val, "<" => elapsed < val, "<=" => elapsed <= val, "==" => elapsed == val, _ => false };
                        }
                        return true;
                    }
                }
                return false;
            }
            if (expr.StartsWith("listexists ")) return _lists.ContainsKey(expr.Substring(11).Trim().ToLower());
            if (expr.StartsWith("listcount "))
            {
                string lname = expr.Substring(10).Trim().ToLower();
                return _lists.TryGetValue(lname, out var l) && l.Count > 0;
            }
            if (expr.StartsWith("inrange "))
            {
                var parts = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3) return _misc.Distance(_player.X, _player.Y, (int)ParseSerial(parts[1]), (int)ParseSerial(parts[2])) <= ParseInt(parts[2]);
                if (parts.Length == 2) return _misc.Distance(_player.X, _player.Y, (int)ParseSerial(parts[1]), (int)ParseSerial(parts[1])) <= 2;
            }

            // Proprietà booleane
            switch (expr)
            {
                case "poisoned":  return _player.IsPoisoned;
                case "dead":      return _player.Hits <= 0;
                case "hidden":    return _player.IsHidden;
                case "mounted":   return _player.IsOnMount;
                case "warmode":   return _player.WarMode;
                case "paralyzed": return false; 
                case "blessed":   return false;
                case "yellowhits": return _player.IsYellowHits;
                case "hasgump":   return _misc.WaitForGumpAny(50);
                case "waitingfortarget": return false;
                case "inparty":   return _player.InParty;
            }

            return false;
        }

        private bool HandleFindType(string expr)
        {
            var parts = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return false;
            
            int graphic = ParseInt(parts[1]);
            int hue = parts.Length > 2 ? ParseInt(parts[2]) : -1;
            uint container = parts.Length > 3 ? ParseSerial(parts[3]) : 0;
            int range = parts.Length > 4 ? ParseInt(parts[4]) : -1;
            
            var item = _items.FindByID(graphic, hue, container, range);
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
            if (_variables.TryGetValue(term, out string? varVal) && long.TryParse(varVal, out long varNum)) return varNum;

            if (long.TryParse(term, out long val)) return val;
            if (term.StartsWith("0x") && long.TryParse(term.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out long hexVal)) return hexVal;

            return term switch
            {
                "hits"         => _player.Hits,
                "maxhits"      => _player.HitsMax,
                "diffhits"     => _player.HitsMax - _player.Hits,
                "mana"         => _player.Mana,
                "maxmana"      => _player.ManaMax,
                "diffmana"     => _player.ManaMax - _player.Mana,
                "stam"         => _player.Stam,
                "maxstam"      => _player.StamMax,
                "diffstam"     => _player.StamMax - _player.Stam,
                "str"          => _player.Str,
                "dex"          => _player.Dex,
                "int"          => _player.Int,
                "serial"       => _player.Serial,
                "weight"       => _player.Weight,
                "maxweight"    => _player.MaxWeight,
                "diffweight"   => _player.MaxWeight - _player.Weight,
                "followers"    => _player.Followers,
                "maxfollowers" => _player.FollowersMax,
                "gold"         => _player.Gold,
                "armor"        => _player.Armor,
                "fame"         => _player.Fame,
                "karma"        => _player.Karma,
                "luck"         => _player.Luck,
                "x"            => _player.X,
                "y"            => _player.Y,
                "z"            => _player.Z,
                "random"       => _rng.Next(0, 100),
                _ => ParseSerial(term)
            };
        }

        private bool HandleCountType(string expr)
        {
            var parts = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return false;
            int graphic = ParseInt(parts[1]);
            int hue = parts.Length > 2 ? ParseInt(parts[2]) : -1;
            uint container = parts.Length > 3 ? ParseSerial(parts[3]) : 0;
            return _items.FindAllByID(graphic, hue, container).Count > 0;
        }

        private void HandleFor(string[] args)
        {
            int limit = args.Length > 0 ? ParseInt(args[0]) : 0;
            if (limit <= 0)
            {
                SkipToBlockEnd("for", "endfor");
                return;
            }
            _forStack.Push((_currentLineIndex, 0, limit, string.Empty));
        }

        private void HandleEndFor()
        {
            if (_forStack.Count == 0) return;
            var (lineIndex, counter, limit, varName) = _forStack.Pop();
            int next = counter + 1;
            if (next < limit)
            {
                _forStack.Push((lineIndex, next, limit, varName));
                _currentLineIndex = lineIndex;
            }
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

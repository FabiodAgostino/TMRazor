using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMRazorImproved.Core.Services.Scripting.Api;
using TMRazorImproved.Shared.Interfaces;

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
        private readonly VendorApi _vendorApi;
        private readonly IFriendsService _friends;
        private readonly IMacrosService _macros;
        private readonly IWorldService _world;
        private readonly ScriptCancellationController _cancel;
        private readonly Action<string> _output;

        private uint _lastMount;
        private uint _toggleLeftSave;
        private uint _toggleRightSave;

        private readonly Dictionary<string, uint> _aliases = new();
        private readonly Dictionary<string, List<uint>> _lists = new();
        private readonly Dictionary<string, long> _timers = new();
        private readonly Dictionary<string, string> _variables = new();
        private readonly List<uint> _useOnceIgnoreList = new();
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
            VendorApi vendorApi,
            IFriendsService friends,
            IMacrosService macros,
            IWorldService world,
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
            _vendorApi = vendorApi;
            _friends = friends;
            _macros = macros;
            _world = world;
            _cancel = cancel;
            _output = output;

            _aliases["self"] = 0;
            _aliases["backpack"] = 0;
            _aliases["lasttarget"] = 0;
            _aliases["found"] = 0;
            _useOnceIgnoreList.Clear();
        }

        public void Execute(string code)
        {
            _lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            _currentLineIndex = 0;
            _loopStack.Clear();
            _forStack.Clear();
            _ifSucceededStack.Clear();
            _useOnceIgnoreList.Clear();

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
                    case "run":
                        if (args.Length > 0) _player.Run(args[0]);
                        break;
                    case "turn":
                        if (args.Length > 0) _player.Turn(args[0]);
                        break;
                    case "fly":
                        _player.Fly(true);
                        break;
                    case "land":
                        _player.Fly(false);
                        break;
                    case "pathfindto":
                        if (args.Length >= 3) _player.PathFindTo(ParseInt(args[0]), ParseInt(args[1]), ParseInt(args[2]));
                        break;
                    case "miniheal":
                        {
                            uint mhTarget = args.Length > 0 ? ParseSerial(args[0]) : _player.Serial;
                            if (_skillsApi.GetValue("Chivalry") >= 30)
                            {
                                _player.Cast("Close Wounds");
                                _misc.WaitForTarget(2500);
                                if (_targetApi.HasTarget()) { if (mhTarget == _player.Serial) _player.TargetSelf(); else _targetApi.Target(mhTarget); }
                            }
                            else if (_skillsApi.GetValue("Magery") >= 10)
                            {
                                _player.Cast("Heal");
                                _misc.WaitForTarget(2500);
                                if (_targetApi.HasTarget()) { if (mhTarget == _player.Serial) _player.TargetSelf(); else _targetApi.Target(mhTarget); }
                            }
                            else
                            {
                                var b = _items.FindByID(0x0E21, -1, _player.Backpack?.Serial ?? 0, -1);
                                if (b != null) { _items.UseItem(b.Serial); _misc.WaitForTarget(1000); _player.TargetSelf(); }
                            }
                        }
                        break;
                    case "bigheal":
                        {
                            _player.Cast("Greater Heal");
                            _misc.WaitForTarget(2500);
                            if (_targetApi.HasTarget())
                            {
                                if (args.Length > 0) _targetApi.Target(ParseSerial(args[0]));
                                else _player.TargetSelf();
                            }
                        }
                        break;
                    case "chivalryheal":
                        _player.Cast("Close Wounds");
                        break;
                    case "bandageself":
                        {
                            var b = _items.FindByID(0x0E21, -1, _player.Backpack?.Serial ?? 0, -1);
                            if (b != null) { _items.UseItem(b.Serial); _misc.WaitForTarget(1000); _player.TargetSelf(); }
                        }
                        break;
                    case "targettype":
                        if (args.Length >= 1)
                        {
                            int ttGraphic = ParseInt(args[0]);
                            int ttColor = args.Length > 1 ? ParseInt(args[1]) : -1;
                            if (args.Length >= 3)
                            {
                                // Third arg: large value = container serial, small value = range (legacy behavior)
                                uint ttThird = ParseSerial(args[2]);
                                if (ttThird > 0x7FFF)
                                {
                                    var ttFound = _world.Items.FirstOrDefault(i =>
                                        i.Graphic == (ushort)ttGraphic &&
                                        (ttColor == -1 || i.Hue == (ushort)ttColor) &&
                                        (i.Container == ttThird || i.ContainerSerial == ttThird));
                                    if (ttFound != null) _targetApi.Target(ttFound.Serial);
                                }
                                else
                                {
                                    _targetApi.TargetType(ttGraphic, ttColor, (int)ttThird);
                                }
                            }
                            else
                            {
                                _targetApi.TargetType(ttGraphic, ttColor);
                            }
                        }
                        break;
                    case "targetground":
                        if (args.Length >= 1)
                        {
                            var g = ParseInt(args[0]);
                            var c = args.Length > 1 ? ParseInt(args[1]) : -1;
                            var range = args.Length > 2 ? ParseInt(args[2]) : 20;
                            // TargetApi.TargetType checks backpack then ground items. For UOSteam's targetground we'll reuse it for now.
                            _targetApi.TargetType(g, c, range, "Nearest");
                        }
                        break;
                    case "targettile":
                        if (args.Length >= 3) _targetApi.TargetXYZ(ParseInt(args[0]), ParseInt(args[1]), ParseInt(args[2]));
                        break;
                    case "targettileoffset":
                        if (args.Length >= 3)
                        {
                            _targetApi.TargetXYZ(_player.X + ParseInt(args[0]), _player.Y + ParseInt(args[1]), _player.Z + ParseInt(args[2]));
                        }
                        break;
                    case "targettilerelative":
                        if (args.Length >= 3)
                        {
                            _targetApi.TargetExecuteRelative(ParseSerial(args[0]), ParseInt(args[1]));
                        }
                        break;
                    case "targetresource":
                        // UOSteam: targetresource tool_serial resource_name (ore/sand/wood/graves/red mushrooms)
                        if (args.Length >= 2)
                            _targetApi.TargetResource(ParseSerial(args[0]), args[1]);
                        break;
                    case "cleartargetqueue":
                        _targetApi.ClearQueue();
                        break;
                    case "autotargetobject":
                        if (args.Length >= 1) _targetApi.SetLastTarget(ParseSerial(args[0]));
                        break;
                    case "cancelautotarget":
                        _targetApi.ClearLast();
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
                    case "buy":
                        if (args.Length > 0) _vendorApi.SetBuyList(args[0]);
                        _vendorApi.Start(); // UOSteam buy can also just enable
                        break;
                    case "sell":
                        if (args.Length > 0) _vendorApi.SetSellList(args[0]);
                        _vendorApi.Start(); // UOSteam sell enables sell agent
                        break;
                    case "clearbuy":
                        _vendorApi.ClearBuyList();
                        break;
                    case "clearsell":
                        _vendorApi.ClearSellList();
                        break;
                    case "contextmenu":
                        // Legacy: contextmenu serial optionString — uses string name matching, 1s timeout
                        if (args.Length >= 2)
                        {
                            uint serial = ParseSerial(args[0]);
                            string option = args[1];
                            _misc.UseContextMenu(serial, option, 1000);
                        }
                        break;
                    case "waitforcontext":
                        // Legacy: waitforcontext serial intIndex [timeout] — uses integer index, longer timeout
                        if (args.Length >= 2)
                        {
                            uint serial = ParseSerial(args[0]);
                            int index = ParseInt(args[1]);
                            int timeout = args.Length > 2 ? ParseInt(args[2]) : 5000;
                            _misc.ContextMenu(serial);
                            _misc.WaitForContext(serial, timeout);
                            _misc.ContextReply(serial, index);
                        }
                        else if (args.Length == 1)
                        {
                            uint serial = ParseSerial(args[0]);
                            _misc.ContextMenu(serial);
                            _misc.WaitForContext(serial, 5000);
                        }
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
                    case "useonce":
                        if (args.Length >= 1)
                        {
                            int graphic = ParseInt(args[0]);
                            int hue = args.Length > 1 ? ParseInt(args[1]) : -1;
                            var backpackSerial = _player.Backpack?.Serial ?? 0;
                            var item = _items.FindAllByID(graphic, hue, backpackSerial, true)
                                .FirstOrDefault(i => !_useOnceIgnoreList.Contains(i.Serial));
                            if (item != null)
                            {
                                _useOnceIgnoreList.Add(item.Serial);
                                _items.UseItem(item.Serial);
                            }
                        }
                        break;
                    case "clearusequeue":
                        _useOnceIgnoreList.Clear();
                        break;
                    case "moveitemoffset":
                        if (args.Length >= 2)
                        {
                            uint serial = ParseSerial(args[0]);
                            string target = args[1].ToLower();
                            int amount = 1;
                            if (args.Length == 3) amount = ParseInt(args[2]);
                            else if (args.Length >= 6) amount = ParseInt(args[5]);

                            if (target == "ground")
                            {
                                if (args.Length >= 5)
                                    _items.MoveOnGround(serial, _player.X + ParseInt(args[2]), _player.Y + ParseInt(args[3]), _player.Z + ParseInt(args[4]), amount);
                                else
                                    _items.DropItemGroundSelf(serial, amount);
                            }
                            else
                            {
                                uint contSerial = ParseSerial(args[1]);
                                if (args.Length >= 5)
                                    _items.Move(serial, contSerial, amount, ParseInt(args[2]), ParseInt(args[3]));
                                else
                                    _items.Move(serial, contSerial, amount);
                            }
                        }
                        break;
                    case "movetypeoffset":
                        if (args.Length >= 2)
                        {
                            int graphic = ParseInt(args[0]);
                            uint srcCont = ParseSerial(args[1]);
                            int amount = -1;
                            int hue = -1;
                            var item = _items.FindByID(graphic, hue, srcCont, true);
                            if (item != null)
                            {
                                if (args.Length >= 8) amount = ParseInt(args[7]);
                                if (args.Length >= 7) hue = ParseInt(args[6]);
                                
                                // Source logic already handled by FindByID
                                if (args.Length >= 6)
                                    _items.MoveOnGround(item.Serial, _player.X + ParseInt(args[3]), _player.Y + ParseInt(args[4]), _player.Z + ParseInt(args[5]), amount == -1 ? item.Amount : amount);
                                else
                                    _items.DropItemGroundSelf(item.Serial, amount == -1 ? item.Amount : amount);
                            }
                        }
                        break;
                    case "waitforproperties":
                        if (args.Length >= 1)
                        {
                            uint serial = ParseSerial(args[0]);
                            int timeout = args.Length > 1 ? ParseInt(args[1]) : 5000;
                            _items.WaitForProps(serial, timeout);
                        }
                        break;
                    case "getenemy":
                        if (args.Length > 0)
                        {
                            var filter = _mobiles.Filter();
                            filter.OnlyAlive = true;
                            bool nearest = false;
                            foreach (var arg in args)
                            {
                                string a = arg.ToLower();
                                if (a == "nearest" || a == "closest") nearest = true;
                                else if (a == "friend") filter.Notorieties.Add(1);
                                else if (a == "innocent") filter.Notorieties.Add(2);
                                else if (a == "criminal") filter.Notorieties.Add(4);
                                else if (a == "gray") { filter.Notorieties.Add(3); filter.Notorieties.Add(4); }
                                else if (a == "murderer") filter.Notorieties.Add(6);
                                else if (a == "enemy") { filter.Notorieties.Add(6); filter.Notorieties.Add(5); filter.Notorieties.Add(4); }
                                else if (a == "humanoid") filter.IsHuman = 1;
                            }
                            var list = _mobiles.ApplyFilter(filter);
                            if (list.Count > 0)
                            {
                                var enemy = nearest ? list.OrderBy(m => m._inner.DistanceTo(_world.Player)).First() : list[0];
                                _aliases["enemy"] = enemy.Serial;
                                _targetApi.SetLastTarget(enemy.Serial);
                                if (!line.Contains("quiet", StringComparison.OrdinalIgnoreCase))
                                    _player.HeadMsg($"[Enemy] {enemy.Name}", 138);
                            }
                            else _aliases.Remove("enemy");
                        }
                        break;
                    case "getfriend":
                        if (args.Length > 0)
                        {
                            var filter = _mobiles.Filter();
                            filter.OnlyAlive = true;
                            bool nearest = false;
                            foreach (var arg in args)
                            {
                                string a = arg.ToLower();
                                if (a == "nearest" || a == "closest") nearest = true;
                                else if (a == "friend") filter.Notorieties.Add(1);
                                else if (a == "innocent") filter.Notorieties.Add(2);
                                else if (a == "criminal" || a == "gray") { filter.Notorieties.Add(3); filter.Notorieties.Add(4); }
                                else if (a == "murderer") filter.Notorieties.Add(6);
                                else if (a == "enemy") { filter.Notorieties.Add(6); filter.Notorieties.Add(5); filter.Notorieties.Add(4); }
                                else if (a == "invulnerable") filter.Notorieties.Add(7);
                                else if (a == "humanoid") filter.IsHuman = 1;
                            }
                            var list = _mobiles.ApplyFilter(filter);
                            if (list.Count > 0)
                            {
                                var friend = nearest ? list.OrderBy(m => m._inner.DistanceTo(_world.Player)).First() : list[0];
                                _aliases["friend"] = friend.Serial;
                                _targetApi.SetLastTarget(friend.Serial);
                                if (!line.Contains("quiet", StringComparison.OrdinalIgnoreCase))
                                    _player.HeadMsg($"[Friend] {friend.Name}", 168);
                            }
                            else _aliases.Remove("friend");
                        }
                        break;
                    case "ignoreobject":
                        if (args.Length > 0) _misc.IgnoreObject(ParseSerial(args[0]));
                        break;
                    case "clearignorelist":
                        _misc.ClearIgnore();
                        break;
                    case "partymsg":
                        if (args.Length > 0) _player.ChatParty(args[0]);
                        break;
                    case "guildmsg":
                        if (args.Length > 0) _player.ChatGuild(args[0]);
                        break;
                    case "allymsg":
                        if (args.Length > 0) _player.ChatAlliance(args[0]);
                        break;
                    case "whispermsg":
                        if (args.Length >= 2) _player.ChatWhisper(args[0], ParseInt(args[1]));
                        else if (args.Length == 1) _player.ChatWhisper(args[0]);
                        break;
                    case "yellmsg":
                        if (args.Length >= 2) _player.ChatYell(args[0], ParseInt(args[1]));
                        else if (args.Length == 1) _player.ChatYell(args[0]);
                        break;
                    case "emotemsg":
                        if (args.Length >= 2) _player.ChatEmote(args[0], ParseInt(args[1]));
                        else if (args.Length == 1) _player.ChatEmote(args[0]);
                        break;
                    case "equipitem":
                        if (args.Length >= 1) _player.EquipItem(ParseSerial(args[0]));
                        break;
                    case "equipwand":
                    {
                        // Cerca nel backpack una bacchetta il cui OPL/nome contiene il tipo richiesto
                        string wandType = args.Length > 0 ? args[0].Trim('\'', '"').ToLowerInvariant() : string.Empty;
                        var backpack = _player.Backpack;
                        if (backpack != null)
                        {
                            var allItems = _items.ApplyFilter(container: backpack.Serial);
                            foreach (var wand in allItems)
                            {
                                string wname = (wand.Name ?? string.Empty).ToLowerInvariant();
                                if (wname.Contains("wand") && (string.IsNullOrEmpty(wandType) || wname.Contains(wandType)))
                                {
                                    _player.EquipItem(wand.Serial);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    case "togglemounted":
                        if (_player.IsOnMount)
                        {
                            var mount = _player.Mount;
                            if (mount != null)
                            {
                                _lastMount = mount.Serial;
                                _aliases["mount"] = _lastMount;
                            }
                            _items.UseItem(_player.Serial); // Dismount
                        }
                        else
                        {
                            if (_lastMount == 0 && _aliases.TryGetValue("mount", out uint m)) _lastMount = m;
                            if (_lastMount != 0) _items.UseItem(_lastMount);
                        }
                        break;
                    case "togglehands":
                        if (args.Length > 0)
                        {
                            string hand = args[0].ToLower();
                            if (hand == "left")
                            {
                                var item = _player.FindLayer(0x02); // LeftHand
                                if (item != null) { _toggleLeftSave = item.Serial; _player.UnEquipItemByLayer("LeftHand"); }
                                else if (_toggleLeftSave != 0) _player.EquipItem(_toggleLeftSave);
                            }
                            else if (hand == "right")
                            {
                                var item = _player.FindLayer(0x01); // RightHand
                                if (item != null) { _toggleRightSave = item.Serial; _player.UnEquipItemByLayer("RightHand"); }
                                else if (_toggleRightSave != 0) _player.EquipItem(_toggleRightSave);
                            }
                        }
                        break;
                    case "clearhands":
                        if (args.Length > 0)
                        {
                            string what = args[0].ToLower();
                            if (what == "both" || what == "left") _player.UnEquipItemByLayer("LeftHand");
                            if (what == "both" || what == "right") _player.UnEquipItemByLayer("RightHand");
                        }
                        else
                        {
                            _player.UnEquipItemByLayer("LeftHand");
                            _player.UnEquipItemByLayer("RightHand");
                        }
                        break;
                    case "addfriend":
                        if (args.Length > 0) _friends.AddFriend(ParseSerial(args[0]), "Added via script");
                        break;
                    case "removefriend":
                        if (args.Length > 0) _friends.RemoveFriend(ParseSerial(args[0]));
                        break;
                    case "toggleautoloot":
                        if (_autoLootApi.Status()) _autoLootApi.Stop(); else _autoLootApi.Start();
                        break;
                    case "togglescavenger":
                        if (_scavengerApi.Status()) _scavengerApi.Stop(); else _scavengerApi.Start();
                        break;
                    case "promptalias":
                        if (args.Length > 0)
                        {
                            string aliasName = args[0].ToLower();
                            _player.HeadMsg($"Click target for alias '{args[0]}' (5s)");
                            // Poll LastTarget for up to 5 seconds to detect when user clicks a target
                            uint prevTarget = (uint)_targetApi.GetLast();
                            long paDeadline = Environment.TickCount64 + 5000;
                            while (Environment.TickCount64 < paDeadline)
                            {
                                _cancel.ThrowIfCancelled();
                                uint cur = (uint)_targetApi.GetLast();
                                if (cur != 0 && cur != prevTarget) { _aliases[aliasName] = cur; break; }
                                System.Threading.Thread.Sleep(50);
                            }
                        }
                        break;
                    case "playmacro":
                        if (args.Length > 0) _macros.Play(args[0]);
                        break;
                    case "virtue":
                        if (args.Length > 0) _player.InvokeVirtue(args[0]);
                        break;
                    case "rename":
                        if (args.Length >= 2) _misc.PetRename(ParseSerial(args[0]), args[1]);
                        break;
                    case "feed":
                        if (args.Length >= 2)
                        {
                            uint target = ParseSerial(args[0]);
                            int graphic = ParseInt(args[1]);
                            int color = args.Length > 2 ? ParseInt(args[2]) : -1;
                            int amount = args.Length > 3 ? ParseInt(args[3]) : 1;
                            var food = _items.FindByID(graphic, color, _player.Backpack?.Serial ?? 0, -1);
                            if (food != null)
                            {
                                if (target == _player.Serial) _items.UseItem(food.Serial);
                                else _items.Move(food.Serial, target, amount);
                            }
                        }
                        break;
                    case "clickobject":
                        if (args.Length > 0) _items.Click(ParseSerial(args[0]));
                        break;
                    case "dressconfig":
                        if (args.Length > 0) _dressApi.ChangeList(args[0]);
                        break;
                    case "paperdoll":
                        _player.OpenPaperDoll();
                        break;
                    case "helpbutton":
                        _player.QuestButton(); // As fallback
                        break;
                    case "guildbutton":
                        _player.GuildButton();
                        break;
                    case "ping":
                        _player.HeadMsg("Ping sent to server...");
                        // For now just resync or something similar
                        break;
                    case "where":
                        _player.HeadMsg($"X: {_player.X}, Y: {_player.Y}, Z: {_player.Z}, Map: {_player.MapId}");
                        break;
                    case "snapshot":
                        _misc.CaptureNow();
                        break;
                    case "messagebox":
                        if (args.Length >= 2) _player.HeadMsg($"[{args[0]}] {args[1]}");
                        else if (args.Length == 1) _player.HeadMsg(args[0]);
                        break;
                    case "shownames":
                    {
                        // Invia SingleClick (0x09) per ogni mobile/corpse visibile per richiederne il nome al server
                        // Sintassi: shownames ['mobiles'/'corpses']
                        bool doMobiles = args.Length == 0 || args[0].Trim('\'', '"').Equals("mobiles", StringComparison.OrdinalIgnoreCase);
                        bool doCorpses = args.Length == 0 || args[0].Trim('\'', '"').Equals("corpses", StringComparison.OrdinalIgnoreCase);
                        const int SHOW_RANGE = 18;
                        if (doMobiles)
                        {
                            foreach (var mob in _world.Mobiles)
                            {
                                if (_player.DistanceTo(mob.Serial) <= SHOW_RANGE)
                                    _items.Click(mob.Serial);
                            }
                        }
                        if (doCorpses)
                        {
                            // I cadaveri in UO hanno graphic 0x2006
                            foreach (var corpse in _world.Items.Where(i => i.Graphic == 0x2006))
                            {
                                if (_player.DistanceTo(corpse.Serial) <= SHOW_RANGE)
                                    _items.Click(corpse.Serial);
                            }
                        }
                        break;
                    }
                    case "hotkeys":
                        if (args.Length > 0)
                        {
                            if (args[0] == "on") _hotkeyApi.SetStatus("Master", true); else _hotkeyApi.SetStatus("Master", false);
                        }
                        break;
                    case "counter":
                        // Stub
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

                    // ------------------------------------------------------------------
                    // Comandi aggiunti da TASK-FR-003
                    // ------------------------------------------------------------------

                    case "uniquejournal":
                        // Crea un journal isolato per questo script (non condiviso con altri)
                        _journal.Clear();
                        break;

                    case "info":
                        // Apre l'inspector per il prossimo oggetto cliccato
                        _misc.Inspect();
                        break;

                    case "clickscreen":
                        // clickscreen (x) (y) ['single'/'double'] ['left'/'right']
                        if (args.Length >= 2 &&
                            int.TryParse(args[0], out int csX) &&
                            int.TryParse(args[1], out int csY))
                        {
                            string csSingleDouble = args.Length > 2 ? args[2].ToLower() : "single";
                            string csLeftRight    = args.Length > 3 ? args[3].ToLower() : "left";
                            if (csLeftRight == "left")
                            {
                                _misc.LeftMouseClick(csX, csY);
                                if (csSingleDouble == "double")
                                {
                                    System.Threading.Thread.Sleep(50);
                                    _misc.LeftMouseClick(csX, csY);
                                }
                            }
                            else
                            {
                                _misc.RightMouseClick(csX, csY);
                                if (csSingleDouble == "double")
                                {
                                    System.Threading.Thread.Sleep(50);
                                    _misc.RightMouseClick(csX, csY);
                                }
                            }
                        }
                        break;

                    case "mapuo":
                        // NOT IMPLEMENTED — funzionalità mappa esterna non disponibile
                        _output?.Invoke("[UOSteam] mapuo: not implemented");
                        break;

                    case "questsbutton":
                        _player.QuestButton();
                        break;

                    case "logoutbutton":
                        _misc.Disconnect();
                        break;

                    case "chatmsg":
                        // chatmsg (text) [color]
                        if (args.Length >= 1)
                        {
                            int chatColor = args.Length >= 2 && int.TryParse(args[1], out int cc) ? cc : 70;
                            _player.ChatSay(args[0], chatColor);
                        }
                        break;

                    case "promptmsg":
                        // promptmsg (text)
                        if (args.Length >= 1)
                            _misc.ResponsePrompt(args[0]);
                        break;

                    case "timermsg":
                        // timermsg (delay) (text) [color]
                        if (args.Length >= 2 && int.TryParse(args[0], out int tmDelay))
                        {
                            string tmText  = args[1];
                            int    tmColor = args.Length >= 3 && int.TryParse(args[2], out int tc) ? tc : 20;
                            System.Threading.Tasks.Task.Delay(tmDelay)
                                .ContinueWith(_ => _misc.SendMessage(tmText, tmColor));
                        }
                        break;

                    case "waitforprompt":
                        // waitforprompt (timeout)
                        if (args.Length >= 1 && int.TryParse(args[0], out int wfpTimeout))
                            _misc.WaitForPrompt(wfpTimeout);
                        break;

                    case "cancelprompt":
                        _misc.CancelPrompt();
                        break;

                    case "setskill":
                        // setskill ('skill name') ('locked'/'up'/'down')
                        if (args.Length >= 2)
                        {
                            int setSkillStatus = args[1].ToLower() switch
                            {
                                "up"     => 0,
                                "down"   => 1,
                                "locked" => 2,
                                _        => 0
                            };
                            _player.SetSkillStatus(args[0], setSkillStatus);
                        }
                        break;

                    case "autocolorpick":
                        // autocolorpick (color) (dyesSerial) (dyeTubSerial)
                        if (args.Length >= 3)
                        {
                            int acpColor = ParseInt(args[0]);
                            uint acpDyesSerial = ParseSerial(args[1]);
                            _items.ChangeDyeingTubColor(acpDyesSerial, acpColor);
                        }
                        break;

                    case "canceltarget":
                        // Esegui target su serial 0 (cancel) — equivalente a Target.Cancel()
                        _targetApi.Cancel();
                        break;

                    case "namespace":
                        // namespace — gestione namespace variabili cross-script
                        // Implementazione minimale: log del comando non supportato pienamente
                        if (args.Length >= 1)
                        {
                            switch (args[0].ToLower())
                            {
                                case "list":
                                    _output?.Invoke("[UOSteam] namespace list: single namespace in use (not isolated)");
                                    break;
                                case "isolation":
                                    // namespace isolation (true|false) — accettato senza effetto nella implementazione corrente
                                    break;
                                default:
                                    // create / activate / delete / move / get / set / print — stub
                                    _output?.Invoke($"[UOSteam] namespace {args[0]}: partial support");
                                    break;
                            }
                        }
                        break;

                    case "script":
                        // script ('run'|'stop'|'suspend'|'resume'|'isrunning'|'issuspended') [script_name] [output_alias]
                        if (args.Length >= 1)
                        {
                            string scriptOp   = args[0].ToLower();
                            string scriptName = args.Length >= 2 ? args[1] : _misc.ScriptCurrent(true);
                            string outputAlias = args.Length >= 3 ? args[2] : scriptName + (scriptOp == "isrunning" ? "_running" : "_suspended");
                            switch (scriptOp)
                            {
                                case "run":      _misc.ScriptRun(scriptName); break;
                                case "stop":     _misc.ScriptStop(scriptName); break;
                                case "suspend":  _misc.ScriptSuspend(scriptName); break;
                                case "resume":   _misc.ScriptResume(scriptName); break;
                                case "isrunning":
                                    bool running = _misc.ScriptStatus(scriptName);
                                    _aliases[outputAlias] = (uint)(running ? 1 : 0);
                                    break;
                                case "issuspended":
                                    bool suspended = _misc.ScriptIsSuspended(scriptName);
                                    _aliases[outputAlias] = (uint)(suspended ? 1 : 0);
                                    break;
                            }
                        }
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

            if (expr == "true") return true;
            if (expr == "false") return false;

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

            // findalias: controlla se un alias è impostato
            if (expr.StartsWith("findalias "))
            {
                string aname = expr.Substring(10).Trim().Trim('\'', '"');
                return _aliases.ContainsKey(aname);
            }

            // buffexists: controlla se il player ha un buff attivo con quel nome
            if (expr.StartsWith("buffexists "))
            {
                string bname = expr.Substring(11).Trim().Trim('\'', '"');
                return _player.BuffsExist(bname);
            }

            // findwand: stub (non implementato nel legacy)
            if (expr.StartsWith("findwand ")) return false;

            // name comparison: name 'serial' = 'text'
            {
                var nm = Regex.Match(expr, @"^name\s+'?(.+?)'?\s*(?:==|=)\s*'?([^']+?)'?$");
                if (nm.Success)
                {
                    uint ns = ParseSerial(nm.Groups[1].Value.Trim());
                    string expected = nm.Groups[2].Value.Trim();
                    var mob = _mobiles.FindBySerial(ns);
                    string actual = mob != null ? mob.Name : (_items.FindBySerial(ns)?.Name ?? string.Empty);
                    return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Comandi di ricerca/query
            if (expr.StartsWith("findtype ")) return HandleFindType(expr);
            if (expr.StartsWith("findobject "))
            {
                uint fo = ParseSerial(expr.Substring(11).Trim());
                if (_world.FindEntity(fo) != null) { _aliases["found"] = fo; return true; }
                return false;
            }
            if (expr.StartsWith("findlayer "))
            {
                var parts = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    uint container = parts.Length > 2 ? ParseSerial(parts[2]) : _player.Serial;
                    var item = _world.Items.FirstOrDefault(i => i.Container == container && i.Layer == (byte)ParseInt(parts[1]));
                    if (item != null) { _aliases["found"] = item.Serial; return true; }
                }
                return false;
            }
            if (expr.StartsWith("counttype ")) return HandleCountType(expr);
            if (expr.StartsWith("counttypeground "))
            {
                var parts = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return false;
                int g = ParseInt(parts[1]);
                int c = parts.Length > 2 ? ParseInt(parts[2]) : -1;
                int r = parts.Length > 3 ? ParseInt(parts[3]) : 18;
                return _world.Items.Count(i => i.ContainerSerial == 0 && i.Graphic == g && (c == -1 || i.Hue == c) && i.DistanceTo(_world.Player) <= r) > 0;
            }
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
                if (parts.Length >= 3) return _player.DistanceTo(ParseSerial(parts[1])) <= ParseInt(parts[2]);
                if (parts.Length == 2) return _player.DistanceTo(ParseSerial(parts[1])) <= 2;
            }
            if (expr.StartsWith("property "))
            {
                var parts = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    uint s = ParseSerial(parts[1]);
                    string p = string.Join(" ", parts.Skip(2)).Trim('\'', '"');
                    var item = _world.FindEntity(s);
                    return item?.OPL?.Properties.Any(prop => prop.Arguments.Contains(p, StringComparison.OrdinalIgnoreCase)) ?? false;
                }
                return false;
            }
            if (expr.StartsWith("infriendlist ")) return _friends.IsFriend(ParseSerial(expr.Substring(13).Trim()));
            if (expr.StartsWith("inregion ")) return _player.Area().Contains(expr.Substring(9).Trim('\'', '"'), StringComparison.OrdinalIgnoreCase);

            // Notorietà
            if (expr.StartsWith("criminal ")) return _mobiles.FindBySerial(ParseSerial(expr.Substring(9).Trim()))?.Notoriety == 4;
            if (expr.StartsWith("enemy ")) return _mobiles.FindBySerial(ParseSerial(expr.Substring(6).Trim()))?.Notoriety == 5;
            if (expr.StartsWith("friend ")) return _mobiles.FindBySerial(ParseSerial(expr.Substring(7).Trim()))?.Notoriety == 1;
            if (expr.StartsWith("gray ")) { var n = _mobiles.FindBySerial(ParseSerial(expr.Substring(5).Trim()))?.Notoriety; return n == 3 || n == 4; }
            if (expr.StartsWith("innocent ")) return _mobiles.FindBySerial(ParseSerial(expr.Substring(9).Trim()))?.Notoriety == 1;
            if (expr.StartsWith("murderer ")) return _mobiles.FindBySerial(ParseSerial(expr.Substring(9).Trim()))?.Notoriety == 6;

            // Proprietà booleane
            switch (expr)
            {
                case "poisoned":  return _player.IsPoisoned;
                case "dead":      return _player.Hits <= 0;
                case "hidden":    return _player.IsHidden;
                case "mounted":   return _player.IsOnMount;
                case "warmode":   return _player.WarMode;
                case "paralyzed": return _player.Paralized; 
                case "yellowhits": return _player.IsYellowHits;
                case "hasgump":    return _misc.WaitForGumpAny(50);
                case "waitingfortarget": return _targetApi.HasTarget();
                case "inparty":    return _player.InParty;
                case "flying":     return _world.Player?.Flying ?? false;
                case "organizing": return _organizerApi.Status();
                case "restocking": return _restockApi.Status();
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

            if (term.StartsWith("skill ") || term.StartsWith("skillvalue "))
            {
                string sname = term.Substring(term.IndexOf(' ') + 1).Trim('\'', '"');
                return (long)(_skillsApi.GetValue(sname) * 10);
            }
            if (term.StartsWith("skillbase "))
            {
                string sname = term.Substring(10).Trim('\'', '"');
                return (long)(_skillsApi.GetBase(sname) * 10);
            }
            if (term.StartsWith("skillstate "))
            {
                string sname = term.Substring(11).Trim('\'', '"');
                return _skillsApi.GetLock(sname).ToLowerInvariant() switch
                {
                    "up"     => 1,
                    "down"   => 0,
                    "locked" => 2,
                    _        => -1
                };
            }
            if (term.StartsWith("contents ")) return _items.ContainerCount(ParseSerial(term.Substring(9).Trim()));
            if (term.StartsWith("distance ")) return _player.DistanceTo(ParseSerial(term.Substring(9).Trim()));
            if (term.StartsWith("name ")) return 0; // string expression — handled in EvaluateExpression
            if (term.StartsWith("amount ")) return _items.FindBySerial(ParseSerial(term.Substring(7).Trim()))?.Amount ?? 0;
            if (term.StartsWith("graphic ")) return _items.FindBySerial(ParseSerial(term.Substring(8).Trim()))?.Graphic ?? 0;
            if (term.StartsWith("color ")) return _items.FindBySerial(ParseSerial(term.Substring(6).Trim()))?.Hue ?? 0;
            if (term.StartsWith("durability "))
            {
                var item = _items.FindBySerial(ParseSerial(term.Substring(11).Trim()));
                return item?.GetPropValue("Durability") ?? 0;
            }

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
                "physical"     => _player.AR,
                "fire"         => _player.FireResist,
                "cold"         => _player.ColdResist,
                "poison"       => _player.PoisonResist,
                "energy"       => _player.EnergyResist,
                "bandage"      => _items.BackpackCount(0x0E21),
                "x"            => _player.X,
                "y"            => _player.Y,
                "z"            => _player.Z,
                "direction"    => _player.DirectionNum,
                "directionname" => _player.DirectionNum & 0x07,
                // Direction string literals — for comparisons like: directionname == north
                "north"        => 0,
                "northeast"    => 1,
                "east"         => 2,
                "southeast"    => 3,
                "south"        => 4,
                "southwest"    => 5,
                "west"         => 6,
                "northwest"    => 7,
                // skillstate string literals — for comparisons like: skillstate Magery == up
                "up"           => 1,
                "down"         => 0,
                "locked"       => 2,
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

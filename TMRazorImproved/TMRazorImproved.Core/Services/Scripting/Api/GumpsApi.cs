using System;
using System.Buffers.Binary;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>Provides script access to open game windows (gumps): detect, read text/strings, respond to buttons, and close gumps.</summary>
    public class GumpsApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;
        private readonly IMessenger _messenger;

        public GumpsApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel, IMessenger messenger)
        {
            _world = world;
            _packet = packet;
            _cancel = cancel;
            _messenger = messenger;
        }

        public enum GumpButtonType
        {
            Page = 0,
            Reply = 1
        }

        public class GumpData
        {
            public uint gumpId;
            public uint serial;
            public uint x;
            public uint y;
            public string gumpDefinition = "";
            public List<string> gumpStrings = new();
            public bool hasResponse;
            public int buttonid = -1;
            public List<int> switches = new();
            public List<string> text = new();
            public List<int> textID = new();
            public string gumpLayout = "";
            public List<string> gumpText = new();
            public List<string> gumpData = new();
            public List<string> layoutPieces = new();
            public List<string> stringList = new();
        }

        /// <summary>Returns <c>true</c> if any gump is currently open.</summary>
        public virtual bool HasGump() => _world.CurrentGump != null;

        /// <summary>Returns <c>true</c> if a gump with the given ID is currently open.</summary>
        public virtual bool HasGump(uint gumpId)
        {
            _cancel.ThrowIfCancelled();
            if (_world.CurrentGump?.GumpId == gumpId) return true;
            return _world.OpenGumps.Values.Any(g => g.GumpId == gumpId);
        }

        /// <summary>Returns the gump ID of the currently focused gump, or 0 if none is open.</summary>
        public virtual uint CurrentGump() => _world.CurrentGump?.GumpId ?? 0;

        /// <summary>Alias for <see cref="CurrentGump()"/>.</summary>
        public virtual uint CurrentID() => CurrentGump();

        /// <summary>Sends a button click response to the current gump.</summary>
        public virtual void SendAction(int buttonId, int[]? switches = null)
        {
            _cancel.ThrowIfCancelled();
            var gump = _world.CurrentGump;
            if (gump == null) return;
            ReplyGump(gump.Serial, gump.GumpId, buttonId, switches, null);
        }

        public virtual void SendAction(uint gumpid, int buttonid, List<int>? sw = null, List<int>? t_id = null, List<string>? t_str = null)
        {
            _cancel.ThrowIfCancelled();
            var gump = GetGumpById(gumpid) ?? _world.CurrentGump;
            if (gump == null) return;

            (int, string)[]? entries = null;
            if (t_str != null && t_str.Count > 0)
            {
                entries = new (int, string)[t_str.Count];
                for (int i = 0; i < t_str.Count; i++)
                {
                    int id = (t_id != null && t_id.Count > i) ? t_id[i] : i;
                    entries[i] = (id, t_str[i]);
                }
            }

            _packet.SendToServer(
                TMRazorImproved.Core.Utilities.PacketBuilder.RespondGump(
                    gump.Serial, gump.GumpId, buttonid, sw?.ToArray(), entries));
            _world.RemoveGump(gump.Serial);
        }

        /// <summary>Closes the current gump by sending button 0 (cancel).</summary>
        public virtual void Close() => SendAction(0);

        /// <summary>Closes the gump with the specified ID.</summary>
        public virtual void Close(uint gumpId)
        {
            _cancel.ThrowIfCancelled();
            var gump = GetGumpById(gumpId);
            if (gump != null)
            {
                _packet.SendToServer(TMRazorImproved.Core.Utilities.PacketBuilder.RespondGump(gump.Serial, gump.GumpId, 0, null, null));
                _world.RemoveGump(gump.Serial);
            }
        }

        /// <summary>Returns the raw data for the gump with the given ID, or <c>null</c> if not found.</summary>
        public virtual GumpData? GetGumpData(uint gumpId)
        {
            _cancel.ThrowIfCancelled();
            var g = _world.CurrentGump;
            if (g != null && g.GumpId == gumpId)
            {
                var gd = new GumpData();
                gd.gumpId = g.GumpId;
                gd.serial = g.Serial;
                gd.gumpLayout = g.Layout;
                gd.gumpStrings = new List<string>(g.Strings);
                return gd;
            }
            return null;
        }

        /// <summary>Blocks until a gump with the given ID appears, or the timeout (ms) expires. Returns <c>true</c> if the gump appeared.</summary>
        public virtual bool WaitForGump(uint gumpId, int timeoutMs = 5000)
        {
            if (gumpId == 0)
            {
                if (_world.CurrentGump != null) return true;
            }
            else
            {
                if (_world.CurrentGump?.GumpId == gumpId) return true;
                if (_world.OpenGumps.Values.Any(g => g.GumpId == gumpId)) return true;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            
            var recipient = new object();
            _messenger.Register<GumpMessage>(recipient, (r, msg) =>
            {
                if (gumpId == 0 || msg.Value.GumpId == gumpId)
                    tcs.TrySetResult(true);
            });

            try
            {
                var task = tcs.Task;
                var deadline = Environment.TickCount64 + timeoutMs;
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (task.IsCompleted) return task.Result;
                    Thread.Sleep(50);
                }
                return false;
            }
            finally
            {
                _messenger.UnregisterAll(recipient);
            }
        }

        public virtual bool WaitForGump(System.Collections.IEnumerable gumpIds, int timeoutMs = 5000)
        {
            var ids = new List<uint>();
            foreach (var id in gumpIds)
            {
                if (id is uint u) ids.Add(u);
                else if (id is int i) ids.Add((uint)i);
                else if (id is long l) ids.Add((uint)l);
            }
            return WaitForGump(ids, timeoutMs);
        }

        public virtual bool WaitForGump(List<uint> gumpIds, int timeoutMs = 5000)
        {
            foreach (var gumpId in gumpIds)
            {
                if (_world.CurrentGump?.GumpId == gumpId) return true;
                if (_world.OpenGumps.Values.Any(g => g.GumpId == gumpId)) return true;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            
            var recipient = new object();
            _messenger.Register<GumpMessage>(recipient, (r, msg) =>
            {
                if (gumpIds.Contains(msg.Value.GumpId))
                    tcs.TrySetResult(true);
            });

            try
            {
                var task = tcs.Task;
                var deadline = Environment.TickCount64 + timeoutMs;
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (task.IsCompleted) return task.Result;
                    Thread.Sleep(50);
                }
                return false;
            }
            finally
            {
                _messenger.UnregisterAll(recipient);
            }
        }

        /// <summary>Returns the number of text strings in the current gump.</summary>
        public virtual int GetLineCount()
        {
            _cancel.ThrowIfCancelled();
            return _world.CurrentGump?.Strings.Count ?? 0;
        }

        public virtual string GetGumpText(int index) => GetStringLine(index);

        public virtual List<string> GetGumpText(uint gumpid)
        {
            _cancel.ThrowIfCancelled();
            var g = GetGumpById(gumpid);
            if (g != null)
                return new List<string>(g.Strings);
            return new List<string>();
        }

        public virtual uint LastGumpId() => CurrentGump();

        public virtual string GetStringLine(int index)
        {
            _cancel.ThrowIfCancelled();
            var gump = _world.CurrentGump;
            if (gump == null || index < 0 || index >= gump.Strings.Count) return string.Empty;
            return gump.Strings[index];
        }

        public virtual bool IsGumpVisible(uint gumpId)
        {
            _cancel.ThrowIfCancelled();
            if (_world.CurrentGump?.GumpId == gumpId) return true;
            return _world.OpenGumps.Values.Any(g => g.GumpId == gumpId);
        }

        public virtual string GetTextEntry(int index)
        {
            _cancel.ThrowIfCancelled();
            return GetStringLine(index);
        }

        public virtual List<int> GetSwitches()
        {
            _cancel.ThrowIfCancelled();
            var gump = _world.CurrentGump;
            if (gump == null) return new List<int>();
            var result = new List<int>();
            var matches = System.Text.RegularExpressions.Regex.Matches(
                gump.Layout, @"\{\s*(?:checkmark|radio)\s+\d+\s+\d+\s+\d+\s+\d+\s+(\d+)\s*\}",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                if (int.TryParse(m.Groups[1].Value, out int id))
                    result.Add(id);
            }
            return result;
        }

        public virtual void ReplyGump(uint gumpSerial, uint gumpTypeId, int buttonId,
            int[]? switches = null, string[]? textEntries = null)
        {
            _cancel.ThrowIfCancelled();
            (int, string)[]? entries = null;
            if (textEntries != null)
            {
                entries = new (int, string)[textEntries.Length];
                for (int i = 0; i < textEntries.Length; i++)
                    entries[i] = (i, textEntries[i]);
            }
            _packet.SendToServer(
                TMRazorImproved.Core.Utilities.PacketBuilder.RespondGump(
                    gumpSerial, gumpTypeId, buttonId, switches, entries));
            _world.RemoveGump(gumpSerial);
        }

        public virtual UOGump? GetGumpById(uint gumpId)
        {
            _cancel.ThrowIfCancelled();
            if (_world.CurrentGump?.GumpId == gumpId) return _world.CurrentGump;
            return _world.OpenGumps.Values.FirstOrDefault(g => g.GumpId == gumpId);
        }

        public virtual int GetTextEntryCount()
        {
            _cancel.ThrowIfCancelled();
            var gump = _world.CurrentGump;
            if (gump == null || string.IsNullOrEmpty(gump.Layout)) return 0;
            return System.Text.RegularExpressions.Regex.Matches(gump.Layout, @"\{\s*textentry\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
        }

        public virtual int GetSwitchCount()
        {
            _cancel.ThrowIfCancelled();
            var gump = _world.CurrentGump;
            if (gump == null || string.IsNullOrEmpty(gump.Layout)) return 0;
            return System.Text.RegularExpressions.Regex.Matches(gump.Layout, @"\{\s*(checkmark|radio)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
        }

        public virtual bool WaitForGumpSerial(uint gumpSerial, int timeoutMs = 5000)
        {
            _cancel.ThrowIfCancelled();
            var deadline = System.Environment.TickCount64 + timeoutMs;
            while (System.Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if (_world.OpenGumps.ContainsKey(gumpSerial)) return true;
                Thread.Sleep(50);
            }
            return _world.OpenGumps.ContainsKey(gumpSerial);
        }

        public virtual List<uint> GetOpenGumpSerials()
        {
            _cancel.ThrowIfCancelled();
            return new List<uint>(_world.OpenGumps.Keys);
        }

        // =========================================================================
        // MIGRATED METHODS (TMRazor Compatibility)
        // =========================================================================

        public virtual bool IsValid(int gumpId)
        {
            return true;
        }

        public virtual GumpData CreateGump(bool movable = true, bool closable = true, bool disposable = true, bool resizeable = true)
        {
            GumpData gd = new GumpData();
            if (!movable) gd.gumpDefinition += "{ nomove}";
            if (!closable) gd.gumpDefinition += "{ noclose}";
            if (!disposable) gd.gumpDefinition += "{ nodispose}";
            if (!resizeable) gd.gumpDefinition += "{ noresize}";
            return gd;
        }

        public virtual void AddPage(ref GumpData gd, int page)
        {
            gd.gumpDefinition += $"{{ page {page} }}";
        }

        public virtual void AddAlphaRegion(ref GumpData gd, int x, int y, int width, int height)
        {
            gd.gumpDefinition += $"{{ checkertrans {x} {y} {width} {height} }}";
        }

        public virtual void AddBackground(ref GumpData gd, int x, int y, int width, int height, int gumpId)
        {
            gd.gumpDefinition += $"{{ resizepic {x} {y} {gumpId} {width} {height} }}";
        }

        public virtual void AddButton(ref GumpData gd, int x, int y, int normalID, int pressedID, int buttonID, int type, int param)
        {
            gd.gumpDefinition += $"{{ button {x} {y} {normalID} {pressedID} {type} {param} {buttonID} }}";
        }

        public virtual void AddCheck(ref GumpData gd, int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            gd.gumpDefinition += $"{{ checkbox {x} {y} {inactiveID} {activeID} {(initialState ? 1 : 0)} {switchID} }}";
        }

        public virtual void AddGroup(ref GumpData gd, int group)
        {
            gd.gumpDefinition += $"{{ group {group} }}";
        }

        public virtual void AddTooltip(ref GumpData gd, int number)
        {
            gd.gumpDefinition += $"{{ tooltip {number} }}";
        }

        public virtual void AddTooltip(ref GumpData gd, string text)
        {
            gd.gumpDefinition += $"{{ tooltip 1114778 @{text}@ }}";
        }

        public virtual void AddTooltip(ref GumpData gd, int cliloc, string text)
        {
            gd.gumpDefinition += $"{{ tooltip {cliloc} @{text}@ }}";
        }

        public virtual void AddHtml(ref GumpData gd, int x, int y, int width, int height, string text, bool background, bool scrollbar)
        {
            gd.gumpStrings.Add(text);
            AddHtml(ref gd, x, y, width, height, gd.gumpStrings.Count - 1, background, scrollbar);
        }

        public virtual void AddHtml(ref GumpData gd, int x, int y, int width, int height, int textID, bool background, bool scrollbar)
        {
            gd.gumpDefinition += $"{{ htmlgump {x} {y} {width} {height} {textID} {(background ? 1 : 0)} {(scrollbar ? 1 : 0)} }}";
        }

        public virtual void AddHtmlLocalized(ref GumpData gd, int x, int y, int width, int height, int number, bool background, bool scrollbar)
        {
            gd.gumpDefinition += $"{{ xmfhtmlgump {x} {y} {width} {height} {number} {(background ? 1 : 0)} {(scrollbar ? 1 : 0)} }}";
        }

        public virtual void AddHtmlLocalized(ref GumpData gd, int x, int y, int width, int height, int number, int color, bool background, bool scrollbar)
        {
            gd.gumpDefinition += $"{{ xmfhtmlgumpcolor {x} {y} {width} {height} {number} {(background ? 1 : 0)} {(scrollbar ? 1 : 0)} {color} }}";
        }

        public virtual void AddHtmlLocalized(ref GumpData gd, int x, int y, int width, int height, int number, string args, int color, bool background, bool scrollbar)
        {
            gd.gumpDefinition += $"{{ xmfhtmltok {x} {y} {width} {height} {(background ? 1 : 0)} {(scrollbar ? 1 : 0)} {color} {number} @{args}@ }}";
        }

        public virtual void AddImage(ref GumpData gd, int x, int y, int gumpId)
        {
            gd.gumpDefinition += $"{{ gumppic {x} {y} {gumpId} }}";
        }

        public virtual void AddImage(ref GumpData gd, int x, int y, int gumpId, int hue)
        {
            gd.gumpDefinition += $"{{ gumppic {x} {y} {gumpId} hue={hue} }}";
        }

        public virtual void AddSpriteImage(ref GumpData gd, int x, int y, int gumpId, int spriteX, int spriteY, int spriteW, int spriteH)
        {
            gd.gumpDefinition += $"{{ picinpic {x} {y} {gumpId} {spriteX} {spriteY} {spriteW} {spriteH} }}";
        }

        public virtual void AddImageTiled(ref GumpData gd, int x, int y, int width, int height, int gumpId)
        {
            gd.gumpDefinition += $"{{ gumppictiled {x} {y} {width} {height} {gumpId} }}";
        }

        public virtual void AddImageTiledButton(ref GumpData gd, int x, int y, int normalID, int pressedID, int buttonID, int type, int param, int itemID, int hue, int width, int height)
        {
            gd.gumpDefinition += $"{{ buttontileart {x} {y} {normalID} {pressedID} {type} {param} {buttonID} {itemID} {hue} {width} {height} }}";
        }

        public virtual void AddImageTiledButton(ref GumpData gd, int x, int y, int normalID, int pressedID, int buttonID, int type, int param, int itemID, int hue, int width, int height, int localizedTooltip)
        {
            gd.gumpDefinition += $"{{ buttontileart {x} {y} {normalID} {pressedID} {type} {param} {buttonID} {itemID} {hue} {width} {height} }}{{ tooltip {localizedTooltip} }}";
        }

        public virtual void AddItem(ref GumpData gd, int x, int y, int itemID)
        {
            gd.gumpDefinition += $"{{ tilepic {x} {y} {itemID} }}";
        }

        public virtual void AddItem(ref GumpData gd, int x, int y, int itemID, int hue)
        {
            gd.gumpDefinition += $"{{ tilepichue {x} {y} {itemID} {hue} }}";
        }

        public virtual void AddLabel(ref GumpData gd, int x, int y, int hue, string text)
        {
            gd.gumpStrings.Add(text);
            gd.gumpDefinition += $"{{ text {x} {y} {hue} {gd.gumpStrings.Count - 1} }}";
        }

        public virtual void AddLabel(ref GumpData gd, int x, int y, int hue, int textID)
        {
            gd.gumpDefinition += $"{{ text {x} {y} {hue} {textID} }}";
        }

        public virtual void AddLabelCropped(ref GumpData gd, int x, int y, int width, int height, int hue, string text)
        {
            gd.gumpStrings.Add(text);
            gd.gumpDefinition += $"{{ croppedtext {x} {y} {width} {height} {hue} {gd.gumpStrings.Count - 1} }}";
        }

        public virtual void AddLabelCropped(ref GumpData gd, int x, int y, int width, int height, int hue, int textID)
        {
            gd.gumpDefinition += $"{{ croppedtext {x} {y} {width} {height} {hue} {textID} }}";
        }

        public virtual void AddRadio(ref GumpData gd, int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            gd.gumpDefinition += $"{{ radio {x} {y} {inactiveID} {activeID} {(initialState ? 1 : 0)} {switchID} }}";
        }

        public virtual void AddTextEntry(ref GumpData gd, int x, int y, int width, int height, int hue, int entryID, string initialText)
        {
            gd.gumpStrings.Add(initialText);
            gd.gumpDefinition += $"{{ textentry {x} {y} {width} {height} {hue} {entryID} {gd.gumpStrings.Count - 1} }}";
        }

        public virtual void AddTextEntry(ref GumpData gd, int x, int y, int width, int height, int hue, int entryID, int initialTextID)
        {
            gd.gumpDefinition += $"{{ textentry {x} {y} {width} {height} {hue} {entryID} {initialTextID} }}";
        }

        public virtual List<uint> AllGumpIDs()
        {
            _cancel.ThrowIfCancelled();
            return _world.OpenGumps.Values.Select(g => g.GumpId).Distinct().ToList();
        }

        public virtual void CloseGump(uint gumpid)
        {
            if (gumpid == 0)
            {
                var cur = _world.CurrentGump;
                if (cur != null)
                {
                    _packet.SendToServer(TMRazorImproved.Core.Utilities.PacketBuilder.RespondGump(cur.Serial, cur.GumpId, 0, null, null));
                    _world.RemoveGump(cur.Serial);
                }
            }
            else
            {
                var gump = GetGumpById(gumpid);
                if (gump != null)
                {
                    _packet.SendToServer(TMRazorImproved.Core.Utilities.PacketBuilder.RespondGump(gump.Serial, gump.GumpId, 0, null, null));
                    _world.RemoveGump(gump.Serial);
                }
            }
        }

        private byte[] BuildGenericGumpPacket(uint gumpId, uint serial, uint x, uint y, string layout, string[] texts)
        {
            var layoutBytes = System.Text.Encoding.ASCII.GetBytes(layout ?? string.Empty);
            int textLinesBytes = 0;
            if (texts != null)
            {
                foreach (var t in texts)
                {
                    textLinesBytes += 2 + (t.Length * 2);
                }
            }

            int pktLen = 21 + layoutBytes.Length + 2 + textLinesBytes;
            byte[] pkt = new byte[pktLen];
            pkt[0] = 0xB0;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(7), gumpId);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(11), x);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(15), y);
            
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(19), (ushort)layoutBytes.Length);
            Array.Copy(layoutBytes, 0, pkt, 21, layoutBytes.Length);
            
            int offset = 21 + layoutBytes.Length;
            ushort textCount = (ushort)(texts?.Length ?? 0);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(offset), textCount);
            offset += 2;
            
            if (texts != null)
            {
                foreach (var t in texts)
                {
                    ushort len = (ushort)t.Length;
                    BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(offset), len);
                    offset += 2;
                    var chars = t.ToCharArray();
                    foreach (var c in chars)
                    {
                        BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(offset), c);
                        offset += 2;
                    }
                }
            }
            return pkt;
        }

        public virtual void SendGump(GumpData gd)
        {
            SendGump(gd, gd.x, gd.y);
        }

        public virtual void SendGump(GumpData gd, uint x, uint y)
        {
            _packet.SendToClient(BuildGenericGumpPacket(gd.gumpId, gd.serial, x, y, gd.gumpDefinition, gd.gumpStrings.ToArray()));
        }

        public virtual void SendGump(uint gumpid, uint serial, uint x, uint y, string gumpDefinition, List<string> gumpStrings)
        {
            _packet.SendToClient(BuildGenericGumpPacket(gumpid, serial, x, y, gumpDefinition, gumpStrings.ToArray()));
        }

        public virtual string GetGumpRawData(uint gumpid)
        {
            return GetGumpRawLayout(gumpid);
        }

        public virtual string GetGumpRawLayout(uint gumpid)
        {
            var g = GetGumpById(gumpid);
            return g?.Layout ?? string.Empty;
        }

        public virtual List<string> GetGumpRawText(uint gumpid)
        {
            var g = GetGumpById(gumpid);
            if (g != null)
                return new List<string>(g.Strings);
            return new List<string>();
        }

        public virtual string GetLine(uint gumpId, int line_num)
        {
            var g = GetGumpById(gumpId);
            if (g != null && line_num >= 0 && line_num < g.Strings.Count)
                return g.Strings[line_num];
            return "";
        }

        public virtual List<string> GetLineList(uint gumpId, bool dataOnly = false)
        {
            var g = GetGumpById(gumpId);
            if (g != null)
                return new List<string>(g.Strings);
            return new List<string>();
        }

        public virtual List<string> GetResolvedStringPieces(uint gumpid)
        {
            var pieces = new List<string>();
            var layout = GetGumpRawLayout(gumpid);
            if (!string.IsNullOrEmpty(layout))
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(layout, @"\{([^}]+)\}");
                var strings = GetGumpRawText(gumpid);
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    string content = m.Groups[1].Value.Trim();
                    string resolved = content;
                    var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        var type = parts[0].ToLower();
                        if ((type == "text" || type == "html" || type == "croppedtext" || type == "htmlgump") && parts.Length > 1)
                        {
                            if (int.TryParse(parts[parts.Length - 1], out int idx) && idx >= 0 && idx < strings.Count)
                            {
                                resolved += "," + strings[idx];
                            }
                        }
                    }
                    pieces.Add(resolved);
                }
            }
            return pieces;
        }

        public virtual string? GetTextByID(GumpData gd, int id)
        {
            for (int i = 0; i < gd.textID.Count; i++)
            {
                if (gd.textID[i] == id)
                    return gd.text[i];
            }
            return null;
        }

        public virtual string LastGumpGetLine(int line_num)
        {
            var gump = _world.CurrentGump;
            if (gump != null && line_num >= 0 && line_num < gump.Strings.Count)
            {
                return gump.Strings[line_num];
            }
            return "";
        }

        public virtual List<string> LastGumpGetLineList()
        {
            var gump = _world.CurrentGump;
            if (gump != null)
            {
                return new List<string>(gump.Strings);
            }
            return new List<string>();
        }

        public virtual string LastGumpRawLayout()
        {
            var gump = _world.CurrentGump;
            return gump?.Layout ?? string.Empty;
        }

        public virtual bool LastGumpTextExist(string text)
        {
            var gump = _world.CurrentGump;
            if (gump != null)
            {
                return gump.Strings.Any(s => s.Contains(text));
            }
            return false;
        }

        public virtual bool LastGumpTextExistByLine(int line_num, string text)
        {
            var line = LastGumpGetLine(line_num);
            if (!string.IsNullOrEmpty(line))
                return line.Contains(text);
            return false;
        }

        public virtual List<int> LastGumpTile()
        {
            var tiles = new List<int>();
            var layout = LastGumpRawLayout();
            if (!string.IsNullOrEmpty(layout))
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(layout, @"\{\s*tilepic(?:hue)?\s+\d+\s+\d+\s+(\d+)");
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    if (int.TryParse(m.Groups[1].Value, out int tileId))
                        tiles.Add(tileId);
                }
            }
            return tiles;
        }

        public virtual void ResetGump()
        {
            var gump = _world.CurrentGump;
            if (gump != null)
                _world.RemoveGump(gump.Serial);
        }

        public virtual void SendAdvancedAction(uint gumpid, int buttonid, List<int> inSwitches)
        {
            SendAction(buttonid, inSwitches?.ToArray());
        }

        public virtual void SendAdvancedAction(uint gumpid, int buttonid, object[] inSwitches)
        {
            var sw = inSwitches?.Select(s => Convert.ToInt32(s)).ToArray();
            var g = GetGumpById(gumpid) ?? _world.CurrentGump;
            if (g != null)
            {
                ReplyGump(g.Serial, g.GumpId, buttonid, sw, null);
            }
        }

        public virtual void SendAdvancedAction(uint gumpid, int buttonid, List<int> textlist_id, List<string> textlist_str)
        {
            SendAdvancedAction(gumpid, buttonid, new List<int>(), textlist_id, textlist_str);
        }

        public virtual void SendAdvancedAction(uint gumpid, int buttonid, List<int> inSwitches, List<int> textlist_id, List<string> textlist_str)
        {
            var g = GetGumpById(gumpid) ?? _world.CurrentGump;
            if (g != null)
            {
                ReplyGump(g.Serial, g.GumpId, buttonid, inSwitches?.ToArray(), textlist_str?.ToArray());
            }
        }

        public virtual void SendAdvancedAction(uint gumpid, int buttonid, object[] inSwitches, object[] textlist_id, object[] textlist_str)
        {
            var sw = inSwitches?.Select(s => Convert.ToInt32(s)).ToList();
            var t_id = textlist_id?.Select(s => Convert.ToInt32(s)).ToList();
            var t_str = textlist_str?.Select(s => s?.ToString() ?? "").ToList();

            SendAdvancedAction(gumpid, buttonid, sw ?? new List<int>(), t_id ?? new List<int>(), t_str ?? new List<string>());
        }
    }
}

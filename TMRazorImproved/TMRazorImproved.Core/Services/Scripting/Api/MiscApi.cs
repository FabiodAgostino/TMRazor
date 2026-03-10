using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API esposta agli script Python come variabile <c>Misc</c>.
    /// Contiene funzioni di utilità: pause, output, timing, file I/O, context menu, script control, ecc.
    ///
    /// Pause() usa uno sleep chunked (10ms slice) per permettere la cancellazione
    /// anche durante attese lunghe da script (il sys.settrace non si attiva
    /// mentre l'esecuzione è bloccata in codice .NET nativo).
    /// </summary>
    public class MiscApi
    {
        private readonly IWorldService        _world;
        private readonly IPacketService       _packetService;
        private readonly IClientInteropService _interop;
        private readonly ITargetingService?   _targeting;
        private readonly IScriptingService?   _scripting;
        private readonly ISoundService?       _sound;
        private readonly IScreenCaptureService? _capture;
        private readonly IConfigService?      _config;
        private readonly ScriptCancellationController _cancel;
        private readonly Action<string>?      _outputCallback;

        // Named lists shared per-script-run (reset on new execution via ScriptingService)
        private readonly ConcurrentDictionary<string, List<object>> _lists = new(StringComparer.OrdinalIgnoreCase);

        // Shared values accessible across all scripts (global, static)
        private static readonly ConcurrentDictionary<string, object> _sharedValues = new(StringComparer.OrdinalIgnoreCase);

        public MiscApi(
            IWorldService world,
            IPacketService packetService,
            IClientInteropService interop,
            ScriptCancellationController cancel,
            Action<string>? outputCallback = null,
            ITargetingService? targeting   = null,
            IScriptingService? scripting   = null,
            ISoundService? sound           = null,
            IScreenCaptureService? capture = null,
            IConfigService? config         = null)
        {
            _world          = world;
            _packetService  = packetService;
            _interop        = interop;
            _targeting      = targeting;
            _scripting      = scripting;
            _sound          = sound;
            _capture        = capture;
            _config         = config;
            _cancel         = cancel;
            _outputCallback = outputCallback;
        }

        // ------------------------------------------------------------------
        // Inner helper classes
        // ------------------------------------------------------------------

        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Point() { }
            public Point(int x, int y) { X = x; Y = y; }
        }

        public class Rectangle
        {
            public int X      { get; set; }
            public int Y      { get; set; }
            public int Width  { get; set; }
            public int Height { get; set; }
        }

        public class MapInfo
        {
            public uint   Serial    { get; set; }
            public int    PinX      { get; set; }
            public int    PinY      { get; set; }
            public int    MapOriginX { get; set; }
            public int    MapOriginY { get; set; }
            public int    MapEndX   { get; set; }
            public int    MapEndY   { get; set; }
            public ushort Facet     { get; set; }
        }

        // ------------------------------------------------------------------
        // Mouse
        // ------------------------------------------------------------------

        public virtual Point MouseLocation()
        {
            var (x, y) = _interop.GetMousePosition();
            return new Point { X = x, Y = y };
        }

        public virtual void MouseMove(int x, int y)
        {
            _interop.SetMousePosition(x, y);
        }

        // ------------------------------------------------------------------
        // Physical mouse clicks (Win32)
        // ------------------------------------------------------------------

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")] private static extern bool SetCursorPos_PInvoke(int x, int y);
        [DllImport("user32.dll")] private static extern bool GetCursorPos(ref System.Drawing.Point lp);
        [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lp);
        [DllImport("user32.dll")] private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtra);

        private const int MOUSEEVENTF_LEFTDOWN  = 0x0002;
        private const int MOUSEEVENTF_LEFTUP    = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP   = 0x0010;

        /// <summary>Left click at (xpos, ypos). clientCoords=true: relative to UO window.</summary>
        public virtual void LeftMouseClick(int xpos, int ypos, bool clientCoords = true)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var hwnd = _interop.GetWindowHandle();
                var old  = new System.Drawing.Point();
                GetCursorPos(ref old);
                if (clientCoords)
                {
                    var pnt = new System.Drawing.Point(xpos, ypos);
                    ClientToScreen(hwnd, ref pnt);
                    xpos = pnt.X; ypos = pnt.Y;
                }
                SetCursorPos_PInvoke(xpos, ypos);
                mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP,   xpos, ypos, 0, 0);
                SetCursorPos_PInvoke(old.X, old.Y);
            }
            catch { }
        }

        /// <summary>Right click at (xpos, ypos). clientCoords=true: relative to UO window.</summary>
        public virtual void RightMouseClick(int xpos, int ypos, bool clientCoords = true)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var hwnd = _interop.GetWindowHandle();
                var old  = new System.Drawing.Point();
                GetCursorPos(ref old);
                if (clientCoords)
                {
                    var pnt = new System.Drawing.Point(xpos, ypos);
                    ClientToScreen(hwnd, ref pnt);
                    xpos = pnt.X; ypos = pnt.Y;
                }
                SetCursorPos_PInvoke(xpos, ypos);
                mouse_event(MOUSEEVENTF_RIGHTDOWN, xpos, ypos, 0, 0);
                mouse_event(MOUSEEVENTF_RIGHTUP,   xpos, ypos, 0, 0);
                SetCursorPos_PInvoke(old.X, old.Y);
            }
            catch { }
        }

        /// <summary>Get UO window size/position as a Rectangle.</summary>
        public virtual Rectangle GetWindowSize()
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var hwnd = _interop.GetWindowHandle();
                if (hwnd == IntPtr.Zero) return new Rectangle();
                if (!GetWindowRect(hwnd, out var r)) return new Rectangle();
                return new Rectangle { X = r.Left, Y = r.Top, Width = r.Right - r.Left, Height = r.Bottom - r.Top };
            }
            catch { return new Rectangle(); }
        }

        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT r);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        // ------------------------------------------------------------------
        // SendMessage
        // ------------------------------------------------------------------

        /// <summary>
        /// Invia un messaggio al client (overhead o di sistema).
        /// </summary>
        public virtual void SendMessage(string msg, int color = 945)
        {
            if (_world.Player == null) return;

            byte[] textBytes = Encoding.BigEndianUnicode.GetBytes(msg + "\0");
            int size = 48 + textBytes.Length;
            byte[] packet = new byte[size];

            packet[0] = 0xAE;
            packet[1] = (byte)(size >> 8);
            packet[2] = (byte)size;
            packet[3] = 0xFF; packet[4] = 0xFF; packet[5] = 0xFF; packet[6] = 0xFF; // Serial
            packet[7] = 0xFF; packet[8] = 0xFF; // Graphic
            packet[9] = 0x00; // Type: Regular
            packet[10] = (byte)(color >> 8);
            packet[11] = (byte)color;
            packet[12] = 0x00; packet[13] = 0x03; // Font
            packet[14] = (byte)'e'; packet[15] = (byte)'n'; packet[16] = (byte)'u'; packet[17] = 0;
            string sysName = "System";
            for (int i = 0; i < sysName.Length; i++) packet[18 + i] = (byte)sysName[i];
            Array.Copy(textBytes, 0, packet, 48, textBytes.Length);

            _packetService.SendToClient(packet);
        }

        public virtual void SendMessage(object obj)              => SendMessage(obj?.ToString() ?? "null");
        public virtual void SendMessage(object obj, int color)   => SendMessage(obj?.ToString() ?? "null", color);
        public virtual void SendMessage(int num)                 => SendMessage(num.ToString());
        public virtual void SendMessage(int num, int color)      => SendMessage(num.ToString(), color);
        public virtual void SendMessage(uint num)                => SendMessage(num.ToString());
        public virtual void SendMessage(uint num, int color)     => SendMessage(num.ToString(), color);
        public virtual void SendMessage(bool val)                => SendMessage(val.ToString());
        public virtual void SendMessage(bool val, int color)     => SendMessage(val.ToString(), color);
        public virtual void SendMessage(double val)              => SendMessage(val.ToString());
        public virtual void SendMessage(double val, int color)   => SendMessage(val.ToString(), color);
        public virtual void SendMessage(float val)               => SendMessage(val.ToString());
        public virtual void SendMessage(float val, int color)    => SendMessage(val.ToString(), color);
        public virtual void SendMessage(string msg, bool wait)   => SendMessage(msg);
        public virtual void SendMessage(string msg, int color, bool wait) => SendMessage(msg, color);

        // ------------------------------------------------------------------
        // Pause / Wait
        // ------------------------------------------------------------------

        /// <summary>
        /// Attende per il numero di millisecondi specificato rispettando la cancellazione.
        /// </summary>
        public virtual void Pause(int milliseconds)
        {
            if (milliseconds <= 0) return;
            var deadline = Environment.TickCount64 + milliseconds;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                var remaining = (int)(deadline - Environment.TickCount64);
                Thread.Sleep(Math.Min(10, Math.Max(0, remaining)));
            }
        }

        public virtual bool WaitFor(Func<bool> condition, int timeoutMs = 5000, int pollMs = 50)
        {
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if (condition()) return true;
                Thread.Sleep(Math.Max(1, pollMs));
            }
            return false;
        }

        public virtual bool WaitForTarget(int timeoutMs = 5000)
        {
            if (_targeting != null)
                return WaitFor(() => _targeting.HasTargetCursor, timeoutMs);
            return false;
        }

        public virtual bool WaitGump(uint gumpId, int timeoutMs = 5000)
        {
            return WaitFor(() => _world.CurrentGump?.GumpId == gumpId, timeoutMs);
        }

        public virtual bool WaitForGumpAny(int timeoutMs = 5000)
        {
            return WaitFor(() => _world.CurrentGump != null || _world.OpenGumps.Count > 0, timeoutMs);
        }

        // ------------------------------------------------------------------
        // Log / Timestamp
        // ------------------------------------------------------------------

        public virtual void Log(string message)
        {
            _outputCallback?.Invoke($"[Script] {message}");
        }

        public virtual long Timestamp()              => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public virtual DateTime Now()                => DateTime.UtcNow;
        public virtual long DeltaTimestamp(long t)   => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - t;

        // ------------------------------------------------------------------
        // File I/O
        // ------------------------------------------------------------------

        public virtual string ReadFile(string path)
        {
            _cancel.ThrowIfCancelled();
            try { return File.Exists(path) ? File.ReadAllText(path) : string.Empty; }
            catch { return string.Empty; }
        }

        public virtual void WriteFile(string path, string content)
        {
            _cancel.ThrowIfCancelled();
            try { File.WriteAllText(path, content); }
            catch { }
        }

        public virtual void AppendToFile(string path, string line)
        {
            _cancel.ThrowIfCancelled();
            try { File.AppendAllText(path, line + Environment.NewLine); }
            catch { }
        }

        /// <summary>Appende una riga solo se non è già presente nel file.</summary>
        public virtual bool AppendNotDupToFile(string path, string line)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                if (File.Exists(path))
                {
                    var lines = File.ReadLines(path).ToList();
                    if (lines.Contains(line)) return true;
                }
                File.AppendAllText(path, line + Environment.NewLine);
                return true;
            }
            catch { return false; }
        }

        /// <summary>Rimuove tutte le occorrenze di una riga dal file.</summary>
        public virtual bool RemoveLineInFile(string path, string line)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                if (!File.Exists(path)) return true;
                var lines = File.ReadLines(path).Where(l => l != line).ToList();
                File.WriteAllLines(path, lines);
                return true;
            }
            catch { return false; }
        }

        /// <summary>Elimina il file se esiste.</summary>
        public virtual bool DeleteFile(string path)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                if (File.Exists(path)) File.Delete(path);
                return true;
            }
            catch { return false; }
        }

        // ------------------------------------------------------------------
        // Random / Utility
        // ------------------------------------------------------------------

        private static readonly Random _rng = new();

        public virtual int Random(int min, int max)
        {
            _cancel.ThrowIfCancelled();
            if (min > max) (min, max) = (max, min);
            return _rng.Next(min, max + 1);
        }

        public virtual int Random(int max) => Random(0, max);

        /// <summary>Just do nothing and enjoy the present moment.</summary>
        public virtual void NoOperation() { }

        /// <summary>Clear the Drag-n-Drop queue (stub).</summary>
        public virtual void ClearDragQueue() { }

        // ------------------------------------------------------------------
        // Distance helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Euclidean (Pythagorean) distance between two Point3D-like objects.
        /// Accepts any object with X, Y properties (or dynamic).
        /// </summary>
        public virtual double DistanceSqrt(dynamic pointA, dynamic pointB)
        {
            _cancel.ThrowIfCancelled();
            double dx = (double)pointA.X - (double)pointB.X;
            double dy = (double)pointA.Y - (double)pointB.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Chebyshev (UO tile) distance between two coordinates.</summary>
        public virtual int Distance(int x1, int y1, int x2, int y2)
        {
            _cancel.ThrowIfCancelled();
            return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
        }

        // ------------------------------------------------------------------
        // Sound
        // ------------------------------------------------------------------

        /// <summary>
        /// Send a sound effect to the UO client.
        /// x/y/z are ignored if ISoundService doesn't support positional audio.
        /// </summary>
        public virtual void PlaySound(int sound, int x = 0, int y = 0, int z = 0)
        {
            _cancel.ThrowIfCancelled();
            if (sound <= 0 || _world.Player == null) return;
            if (_sound != null)
            {
                _sound.PlaySound((ushort)sound);
                return;
            }
            // Fallback: send raw 0x54 packet to client
            byte[] pkt = new byte[12];
            pkt[0] = 0x54;
            pkt[1] = 0x01; // play mode
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(2), (ushort)sound);
            // [4-5] volume (0), [6-7] x, [8-9] y, [10-11] z
            BinaryPrimitives.WriteInt16BigEndian(pkt.AsSpan(6),  (short)x);
            BinaryPrimitives.WriteInt16BigEndian(pkt.AsSpan(8),  (short)y);
            BinaryPrimitives.WriteInt16BigEndian(pkt.AsSpan(10), (short)z);
            _packetService.SendToClient(pkt);
        }

        /// <summary>Play system beep.</summary>
        public virtual void Beep()
        {
            _cancel.ThrowIfCancelled();
            try { Console.Beep(); } catch { }
        }

        // ------------------------------------------------------------------
        // UO Client / Connection
        // ------------------------------------------------------------------

        /// <summary>Send keystroke(s) to the UO window.</summary>
        public virtual void SendToClient(string keys)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var hwnd = _interop.GetWindowHandle();
                if (hwnd != IntPtr.Zero) SetForegroundWindow(hwnd);
                // Note: System.Windows.Forms not available in this project —
                // forward via PostMessage WM_CHAR for each character as best-effort stub.
                const uint WM_CHAR = 0x0102;
                foreach (char c in keys)
                    _interop.PostMessage(hwnd, WM_CHAR, (IntPtr)c, IntPtr.Zero);
            }
            catch { }
        }

        /// <summary>Force disconnect (sends 0x01 logout packet to server).</summary>
        public virtual void Disconnect()
        {
            _cancel.ThrowIfCancelled();
            // 0x01 Disconnect Notification: cmd(1) + pattern(4) = 5 bytes
            byte[] pkt = new byte[5];
            pkt[0] = 0x01;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), 0xFFFFFFFF);
            _packetService.SendToServer(pkt);
        }

        /// <summary>Trigger a client ReSync.</summary>
        public virtual void Resync()
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = { 0x22, 0xFF, 0x00 };
            _packetService.SendToServer(pkt);
        }

        /// <summary>Bring the UO window to the foreground.</summary>
        public virtual void FocusUO()
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var hwnd = _interop?.GetWindowHandle() ?? IntPtr.Zero;
                if (hwnd != IntPtr.Zero)
                {
                    ShowWindow(hwnd, 9); // SW_RESTORE
                    SetForegroundWindow(hwnd);
                }
            }
            catch { }
        }

        /// <summary>Alias for FocusUO (RazorEnhanced compatibility).</summary>
        public virtual void FocusUOWindow() => FocusUO();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>Serial del player corrente.</summary>
        public virtual uint GetPlayerSerial()
        {
            _cancel.ThrowIfCancelled();
            return _world.Player?.Serial ?? 0;
        }

        /// <summary>Name of the current shard (stub).</summary>
        public virtual string ShardName()
        {
            _cancel.ThrowIfCancelled();
            return string.Empty;
        }

        /// <summary>Enable/disable season filter (stub).</summary>
        public virtual void FilterSeason(bool enable, uint seasonFlag) { }

        // ------------------------------------------------------------------
        // Serial type checks
        // ------------------------------------------------------------------

        /// <summary>True if the serial belongs to an item (>= 0x40000000).</summary>
        public virtual bool IsItem(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return serial >= 0x40000000;
        }

        /// <summary>True if the serial belongs to a mobile (< 0x40000000).</summary>
        public virtual bool IsMobile(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return serial > 0 && serial < 0x40000000;
        }

        // ------------------------------------------------------------------
        // Directories
        // ------------------------------------------------------------------

        public virtual string RazorDirectory()
        {
            _cancel.ThrowIfCancelled();
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public virtual string ScriptDirectory()
        {
            _cancel.ThrowIfCancelled();
            return Path.Combine(RazorDirectory(), "Scripts");
        }

        public virtual string ConfigDirectory()
        {
            _cancel.ThrowIfCancelled();
            return Path.Combine(RazorDirectory(), "Config");
        }

        public virtual string DataDirectory()
        {
            _cancel.ThrowIfCancelled();
            return Path.Combine(RazorDirectory(), "Data");
        }

        // ------------------------------------------------------------------
        // Context Menu
        // ------------------------------------------------------------------

        /// <summary>
        /// Send a context menu request to the server for <paramref name="serial"/> (0xBF sub 0x0E).
        /// The server will respond with 0xBF sub 0x14, which WorldPacketHandler parses into ContextMenuStore.
        /// </summary>
        public virtual void ContextMenu(uint serial)
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = new byte[9];
            pkt[0] = 0xBF;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 9);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x0E);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial);
            _packetService.SendToServer(pkt);
        }

        /// <summary>
        /// Request context menu and wait up to <paramref name="delay"/> ms for the server response.
        /// Returns list of Context entries (Response = menu index, Entry = display text).
        /// </summary>
        public virtual List<ContextMenuEntry> WaitForContext(uint serial, int delay)
        {
            _cancel.ThrowIfCancelled();
            ContextMenuStore.Clear();
            ContextMenu(serial);
            return ContextMenuStore.WaitForSerial(serial, delay);
        }

        /// <summary>
        /// Send a context menu response (0xBF sub 0x10).
        /// <paramref name="responseNum"/> is the Response value from a ContextMenuEntry.
        /// </summary>
        public virtual void ContextReply(uint serial, int responseNum)
        {
            _cancel.ThrowIfCancelled();
            // 0xBF sub 0x10: cmd(1) len(2) sub(2) serial(4) responseId(2) = 11 bytes
            byte[] pkt = new byte[11];
            pkt[0] = 0xBF;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 11);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x10);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(9), (ushort)responseNum);
            _packetService.SendToServer(pkt);
        }

        /// <summary>Context reply by menu entry text (case-insensitive).</summary>
        public virtual void ContextReply(uint serial, string menuName)
        {
            _cancel.ThrowIfCancelled();
            var entries = ContextMenuStore.WaitForSerial(serial, 0);
            var match   = entries.FirstOrDefault(e => string.Equals(e.Entry.Trim(), menuName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null)
                ContextReply(serial, match.Response);
        }

        /// <summary>
        /// Open context menu, find entry by text, and click it.
        /// Returns true on success.
        /// </summary>
        public virtual bool UseContextMenu(uint serial, string choice, int delay)
        {
            _cancel.ThrowIfCancelled();
            var entries = WaitForContext(serial, delay);
            var match   = entries.FirstOrDefault(e => string.Equals(e.Entry.Trim(), choice.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match == null) return false;
            ContextReply(serial, match.Response);
            return true;
        }

        // ------------------------------------------------------------------
        // Prompt
        // ------------------------------------------------------------------

        public virtual bool HasPrompt()
        {
            _cancel.ThrowIfCancelled();
            return _targeting?.HasPrompt ?? false;
        }

        public virtual bool WaitForPrompt(int delay)
        {
            _cancel.ThrowIfCancelled();
            if (_targeting == null) return false;
            return WaitFor(() => _targeting.HasPrompt, delay);
        }

        public virtual void CancelPrompt()
        {
            _cancel.ThrowIfCancelled();
            if (_targeting == null) return;
            uint serial   = _targeting.PendingPromptSerial;
            uint promptId = _targeting.PendingPromptId;
            // 0x9A PromptResponse: cmd(1) len(2) serial(4) promptId(4) type(4) lang(4) text(0)+null(1) = 16 bytes
            byte[] pkt = new byte[16];
            pkt[0] = 0x9A;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1),  16);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3),  serial);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(7),  promptId);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(11), 0); // cancel
            _packetService.SendToServer(pkt);
            _targeting.SetPrompt(false);
        }

        public virtual void ResponsePrompt(string text)
        {
            _cancel.ThrowIfCancelled();
            if (_targeting == null) return;
            _targeting.SendPrompt(text);
            _targeting.SetPrompt(false);
        }

        // ------------------------------------------------------------------
        // Old Menu stubs (pre-context-menu UO menus)
        // ------------------------------------------------------------------

        public virtual bool HasMenu()        { _cancel.ThrowIfCancelled(); return false; }
        public virtual void CloseMenu()      { _cancel.ThrowIfCancelled(); }
        public virtual bool MenuContain(string text) { _cancel.ThrowIfCancelled(); return false; }
        public virtual string GetMenuTitle() { _cancel.ThrowIfCancelled(); return string.Empty; }
        public virtual bool WaitForMenu(int delay) { _cancel.ThrowIfCancelled(); return false; }
        public virtual void MenuResponse(string text) { _cancel.ThrowIfCancelled(); }

        // ------------------------------------------------------------------
        // Query String stubs
        // ------------------------------------------------------------------

        public virtual bool HasQueryString() { _cancel.ThrowIfCancelled(); return false; }
        public virtual bool WaitForQueryString(int delay) { _cancel.ThrowIfCancelled(); return false; }
        public virtual void QueryStringResponse(bool okcancel, string response) { _cancel.ThrowIfCancelled(); }

        // ------------------------------------------------------------------
        // Script control
        // ------------------------------------------------------------------

        public virtual void ScriptRun(string scriptfile)
        {
            _cancel.ThrowIfCancelled();
            if (_scripting == null) return;
            _ = _scripting.RunScript(scriptfile);
        }

        public virtual void ScriptStop(string scriptfile)
        {
            _cancel.ThrowIfCancelled();
            if (_scripting == null) return;
            if (string.Equals(_scripting.CurrentScriptName, scriptfile, StringComparison.OrdinalIgnoreCase))
                _ = _scripting.StopAsync();
        }

        public virtual void ScriptStopAll(bool skipCurrent = false)
        {
            _cancel.ThrowIfCancelled();
            if (_scripting == null) return;
            if (!skipCurrent)
                _ = _scripting.StopAsync();
        }

        public virtual string ScriptCurrent(bool fullpath = true)
        {
            _cancel.ThrowIfCancelled();
            return _scripting?.CurrentScriptName ?? string.Empty;
        }

        public virtual void ScriptSuspend(string scriptfile) { _cancel.ThrowIfCancelled(); /* stub */ }
        public virtual void ScriptResume(string scriptfile)  { _cancel.ThrowIfCancelled(); /* stub */ }
        public virtual bool ScriptIsSuspended(string scriptfile) { _cancel.ThrowIfCancelled(); return false; }

        public virtual bool ScriptStatus(string scriptfile)
        {
            _cancel.ThrowIfCancelled();
            if (_scripting == null) return false;
            return string.Equals(_scripting.CurrentScriptName, scriptfile, StringComparison.OrdinalIgnoreCase)
                   && _scripting.IsRunning;
        }

        // ------------------------------------------------------------------
        // Screen capture
        // ------------------------------------------------------------------

        public virtual string CaptureNow()
        {
            _cancel.ThrowIfCancelled();
            if (_capture == null) return string.Empty;
            try
            {
                var task = _capture.CaptureAsync();
                task.Wait();
                return task.Result ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        // ------------------------------------------------------------------
        // Map info (stub)
        // ------------------------------------------------------------------

        public virtual MapInfo GetMapInfo(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return new MapInfo { Serial = serial };
        }

        // ------------------------------------------------------------------
        // Pet Rename
        // ------------------------------------------------------------------

        /// <summary>Rename a pet by serial (0x75 RenameRequest).</summary>
        public virtual void PetRename(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            int len = 1 + 4 + nameBytes.Length + 1; // cmd + serial + name + null
            byte[] pkt = new byte[len];
            pkt[0] = 0x75;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            Array.Copy(nameBytes, 0, pkt, 5, nameBytes.Length);
            _packetService.SendToServer(pkt);
        }

        // ------------------------------------------------------------------
        // No Run Stealth stubs
        // ------------------------------------------------------------------

        public virtual void NoRunStealthToggle(bool enable) { _cancel.ThrowIfCancelled(); }
        public virtual bool NoRunStealthStatus()            { _cancel.ThrowIfCancelled(); return false; }

        // ------------------------------------------------------------------
        // Ignore list
        // ------------------------------------------------------------------

        private readonly ConcurrentDictionary<uint, byte> _ignoreList = new();

        public virtual void Ignore(uint serial)         { _ignoreList.TryAdd(serial, 0); }
        public virtual void UnIgnore(uint serial)       { _ignoreList.TryRemove(serial, out _); }
        public virtual bool IsIgnored(uint serial)      => _ignoreList.ContainsKey(serial);
        public virtual void ClearIgnores()              { _ignoreList.Clear(); }
        public virtual List<uint> GetIgnoreList()       => new List<uint>(_ignoreList.Keys);

        // RazorEnhanced-named aliases
        public virtual void IgnoreObject(uint serial)       => Ignore(serial);
        public virtual bool CheckIgnoreObject(uint serial)  => IsIgnored(serial);
        public virtual void UnIgnoreObject(uint serial)     => UnIgnore(serial);
        public virtual void ClearIgnore()                   => ClearIgnores();

        // ------------------------------------------------------------------
        // Shared Values (global, accessible across all scripts)
        // ------------------------------------------------------------------

        public virtual object? ReadSharedValue(string name)
        {
            _cancel.ThrowIfCancelled();
            return _sharedValues.TryGetValue(name, out var val) ? val : null;
        }

        public virtual void SetSharedValue(string name, object value)
        {
            _cancel.ThrowIfCancelled();
            _sharedValues.AddOrUpdate(name, value, (_, _) => value);
        }

        public virtual void RemoveSharedValue(string name)
        {
            _cancel.ThrowIfCancelled();
            _sharedValues.TryRemove(name, out _);
        }

        public virtual bool CheckSharedValue(string name)
        {
            _cancel.ThrowIfCancelled();
            return _sharedValues.ContainsKey(name);
        }

        public virtual List<string> AllSharedValue()
        {
            _cancel.ThrowIfCancelled();
            return _sharedValues.Keys.ToList();
        }

        // ------------------------------------------------------------------
        // Missing Misc API additions
        // ------------------------------------------------------------------

        public class Context
        {
            public int Response { get; set; }
            public string Entry { get; set; } = string.Empty;
        }

        public ConcurrentDictionary<string, object> SharedScriptData
        {
            get => _sharedValues;
            set
            {
                _sharedValues.Clear();
                if (value != null)
                {
                    foreach (var kvp in value)
                        _sharedValues.TryAdd(kvp.Key, kvp.Value);
                }
            }
        }

        public virtual void ChangeProfile(string profileName)
        {
            _cancel.ThrowIfCancelled();
            Engine.MainWindow.SafeAction(s => s.changeProfile(profileName));
        }

        public virtual void CloseBackpack()
        {
            _cancel.ThrowIfCancelled();
            RazorEnhanced.UoWarper.UODLLHandleClass = new RazorEnhanced.UoWarper.UO();

            if (!RazorEnhanced.UoWarper.UODLLHandleClass.Open())
            {
                while (!RazorEnhanced.UoWarper.UODLLHandleClass.Open())
                {
                    Thread.Sleep(50);
                }
            }
            RazorEnhanced.UoWarper.UODLLHandleClass.CloseBackpack();
        }

        public virtual string CurrentScriptDirectory()
        {
            return ScriptDirectory();
        }

        public virtual void ExportPythonAPI(string path = null, bool pretty = true)
        {
            _cancel.ThrowIfCancelled();
            AutoDocIO.ExportPythonAPI(path, pretty);
        }

        public virtual Point GetContPosition()
        {
            _cancel.ThrowIfCancelled();
            RazorEnhanced.UoWarper.UODLLHandleClass = new RazorEnhanced.UoWarper.UO();

            if (!RazorEnhanced.UoWarper.UODLLHandleClass.Open())
            {
                while (!RazorEnhanced.UoWarper.UODLLHandleClass.Open())
                {
                    Thread.Sleep(50);
                }
            }
            var point = RazorEnhanced.UoWarper.UODLLHandleClass.GetContPosition();
            return new Point(point.X, point.Y);
        }

        public virtual void Inspect()
        {
            _cancel.ThrowIfCancelled();
            Assistant.Targeting.OneTimeTarget(true, new Assistant.Targeting.TargetResponseCallback(Assistant.Commands.GetInfoTarget_Callback));
        }

        public virtual object? LastHotKey()
        {
            _cancel.ThrowIfCancelled();
            return HotKeyEvent.LastEvent;
        }

        public virtual void NextContPosition(int x, int y)
        {
            _cancel.ThrowIfCancelled();
            RazorEnhanced.UoWarper.UODLLHandleClass = new RazorEnhanced.UoWarper.UO();

            if (!RazorEnhanced.UoWarper.UODLLHandleClass.Open())
            {
                while (!RazorEnhanced.UoWarper.UODLLHandleClass.Open())
                {
                    Thread.Sleep(50);
                }
            }
            RazorEnhanced.UoWarper.UODLLHandleClass.NextContPosition(x, y);
        }

        public virtual void OpenPaperdoll()
        {
            _cancel.ThrowIfCancelled();
            RazorEnhanced.UoWarper.UODLLHandleClass = new RazorEnhanced.UoWarper.UO();

            if (!RazorEnhanced.UoWarper.UODLLHandleClass.Open())
            {
                while (!RazorEnhanced.UoWarper.UODLLHandleClass.Open())
                {
                    Thread.Sleep(50);
                }
            }
            RazorEnhanced.UoWarper.UODLLHandleClass.OpenPaperdoll();
        }

        public virtual void ResetPrompt()
        {
            _cancel.ThrowIfCancelled();
            if (_targeting != null)
                _targeting.SetPrompt(false);
        }

        public virtual bool SetCursorPos(int x, int y)
        {
            return SetCursorPos_PInvoke(x, y);
        }

        public virtual void ConcurrentDictionary() { }

        // ------------------------------------------------------------------
        // Named Lists — compatibilità RazorEnhanced/UOSteam
        // ------------------------------------------------------------------

        public virtual void CreateList(string name)
        {
            _cancel.ThrowIfCancelled();
            _lists.TryAdd(name, new List<object>());
        }

        public virtual void PushList(string name, object value, string pos = "front")
        {
            _cancel.ThrowIfCancelled();
            var list = _lists.GetOrAdd(name, _ => new List<object>());
            lock (list)
            {
                if (pos.Equals("back", StringComparison.OrdinalIgnoreCase))
                    list.Add(value);
                else
                    list.Insert(0, value);
            }
        }

        public virtual object? PopList(string name, string pos = "front")
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return null;
            lock (list)
            {
                if (list.Count == 0) return null;
                int idx = pos.Equals("back", StringComparison.OrdinalIgnoreCase) ? list.Count - 1 : 0;
                var val = list[idx];
                list.RemoveAt(idx);
                return val;
            }
        }

        public virtual void RemoveList(string name, object value)
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return;
            lock (list) { list.Remove(value); }
        }

        public virtual void ClearList(string name)
        {
            _cancel.ThrowIfCancelled();
            if (_lists.TryGetValue(name, out var list)) lock (list) { list.Clear(); }
        }

        public virtual void DestroyList(string name)
        {
            _cancel.ThrowIfCancelled();
            _lists.TryRemove(name, out _);
        }

        public virtual bool ListExists(string name)
        {
            _cancel.ThrowIfCancelled();
            return _lists.ContainsKey(name);
        }

        public virtual int ListCount(string name)
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return 0;
            lock (list) { return list.Count; }
        }

        public virtual List<object> GetList(string name)
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return new List<object>();
            lock (list) { return new List<object>(list); }
        }

        public virtual bool ListContains(string name, object value)
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return false;
            lock (list) { return list.Contains(value); }
        }

    }
}

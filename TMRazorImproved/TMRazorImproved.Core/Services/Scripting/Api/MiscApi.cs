using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API esposta agli script Python come variabile <c>Misc</c>.
    /// Contiene funzioni di utilità: pause, output, timing.
    ///
    /// Pause() usa uno sleep chunked (10ms slice) per permettere la cancellazione
    /// anche durante attese lunghe da script (il sys.settrace non si attiva
    /// mentre l'esecuzione è bloccata in codice .NET nativo).
    /// </summary>
    public class MiscApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packetService;
        private readonly IClientInteropService _interop;
        private readonly ITargetingService? _targeting;
        private readonly ScriptCancellationController _cancel;
        private readonly Action<string>? _outputCallback;

        // Named lists shared per-script-run (reset on new execution via ScriptingService)
        private readonly ConcurrentDictionary<string, List<object>> _lists = new(StringComparer.OrdinalIgnoreCase);

        public MiscApi(IWorldService world, IPacketService packetService, IClientInteropService interop,
            ScriptCancellationController cancel, Action<string>? outputCallback = null,
            ITargetingService? targeting = null)
        {
            _world = world;
            _packetService = packetService;
            _interop = interop;
            _targeting = targeting;
            _cancel = cancel;
            _outputCallback = outputCallback;
        }

        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public virtual Point MouseLocation()
        {
            var (x, y) = _interop.GetMousePosition();
            return new Point { X = x, Y = y };
        }

        public virtual void MouseMove(int x, int y)
        {
            _interop.SetMousePosition(x, y);
        }

        /// <summary>
        /// Invia un messaggio al client (overhead o di sistema).
        /// </summary>
        /// <param name="msg">Testo del messaggio.</param>
        /// <param name="color">Colore del messaggio (default 945).</param>
        public virtual void SendMessage(string msg, int color = 945)
        {
            if (_world.Player == null) return;

            // Packet 0xAE: Unicode Message
            // [1] ID (0xAE)
            // [2] Size (short)
            // [4] Serial (0xFFFFFFFF = System)
            // [2] Graphic (0xFFFF)
            // [1] Type (0x00 = Regular)
            // [2] Hue
            // [2] Font (3)
            // [4] Language ("enu")
            // [30] Name ("System")
            // [var] Text (null-terminated Unicode BE)

            byte[] textBytes = System.Text.Encoding.BigEndianUnicode.GetBytes(msg + "\0");
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
            
            packet[14] = (byte)'e'; packet[15] = (byte)'n'; packet[16] = (byte)'u'; packet[17] = 0; // Language
            
            string sysName = "System";
            for (int i = 0; i < sysName.Length; i++) packet[18 + i] = (byte)sysName[i];

            Array.Copy(textBytes, 0, packet, 48, textBytes.Length);

            _packetService.SendToClient(packet);
        }

        public virtual void SendMessage(object obj) => SendMessage(obj?.ToString() ?? "null");
        public virtual void SendMessage(object obj, int color) => SendMessage(obj?.ToString() ?? "null", color);
        public virtual void SendMessage(int num) => SendMessage(num.ToString());
        public virtual void SendMessage(int num, int color) => SendMessage(num.ToString(), color);
        public virtual void SendMessage(uint num) => SendMessage(num.ToString());
        public virtual void SendMessage(uint num, int color) => SendMessage(num.ToString(), color);
        public virtual void SendMessage(bool val) => SendMessage(val.ToString());
        public virtual void SendMessage(bool val, int color) => SendMessage(val.ToString(), color);
        public virtual void SendMessage(double val) => SendMessage(val.ToString());
        public virtual void SendMessage(double val, int color) => SendMessage(val.ToString(), color);
        public virtual void SendMessage(float val) => SendMessage(val.ToString());
        public virtual void SendMessage(float val, int color) => SendMessage(val.ToString(), color);
        public virtual void SendMessage(string msg, bool wait) => SendMessage(msg); // Ignora wait per ora
        public virtual void SendMessage(string msg, int color, bool wait) => SendMessage(msg, color);

        /// <summary>
        /// Attende per il numero di millisecondi specificato rispettando la cancellazione.
        /// Equivalente a time.sleep() ma interrompibile da StopAsync().
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

        /// <summary>
        /// Attende finché la condizione non è vera, con timeout.
        /// </summary>
        /// <param name="condition">Funzione che ritorna true quando l'attesa deve terminare.</param>
        /// <param name="timeoutMs">Timeout massimo in millisecondi.</param>
        /// <param name="pollMs">Intervallo di polling in millisecondi (default 50ms).</param>
        /// <returns>True se la condizione è diventata vera, False se è scattato il timeout.</returns>
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
            return WaitFor(() => false, timeoutMs);
        }

        public virtual bool WaitGump(uint gumpId, int timeoutMs = 5000)
        {
            return WaitFor(() => _world.CurrentGump?.GumpId == gumpId, timeoutMs);
        }

        /// <summary>Attende qualsiasi gump (indipendentemente dall'ID).</summary>
        public virtual bool WaitForGumpAny(int timeoutMs = 5000)
        {
            return WaitFor(() => _world.CurrentGump != null || _world.OpenGumps.Count > 0, timeoutMs);
        }

        /// <summary>Scrive una riga sul pannello output della UI (non sulla chat UO).</summary>
        public virtual void Log(string message)
        {
            _outputCallback?.Invoke($"[Script] {message}");
        }

        /// <summary>Ritorna il timestamp Unix corrente in millisecondi.</summary>
        public virtual long Timestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // ------------------------------------------------------------------
        // File I/O
        // ------------------------------------------------------------------

        /// <summary>Legge il contenuto di un file di testo. Ritorna stringa vuota se il file non esiste.</summary>
        public virtual string ReadFile(string path)
        {
            _cancel.ThrowIfCancelled();
            try { return File.Exists(path) ? File.ReadAllText(path) : string.Empty; }
            catch { return string.Empty; }
        }

        /// <summary>Scrive (sovrascrive) un file di testo con il contenuto specificato.</summary>
        public virtual void WriteFile(string path, string content)
        {
            _cancel.ThrowIfCancelled();
            try { File.WriteAllText(path, content); }
            catch { }
        }

        /// <summary>Aggiunge una riga in coda a un file di testo.</summary>
        public virtual void AppendToFile(string path, string line)
        {
            _cancel.ThrowIfCancelled();
            try { File.AppendAllText(path, line + Environment.NewLine); }
            catch { }
        }

        // ------------------------------------------------------------------
        // Random e tempo
        // ------------------------------------------------------------------

        private static readonly Random _rng = new();

        /// <summary>Ritorna un intero casuale tra min (incluso) e max (incluso).</summary>
        public virtual int Random(int min, int max)
        {
            _cancel.ThrowIfCancelled();
            if (min > max) (min, max) = (max, min);
            return _rng.Next(min, max + 1);
        }

        /// <summary>Ritorna un intero casuale tra 0 e max (incluso).</summary>
        public virtual int Random(int max) => Random(0, max);

        /// <summary>Ritorna l'ora corrente come DateTime (UTC).</summary>
        public virtual DateTime Now() => DateTime.UtcNow;

        /// <summary>Ritorna i millisecondi trascorsi dal timestamp <paramref name="t"/> (da <see cref="Timestamp"/>).</summary>
        public virtual long DeltaTimestamp(long t) => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - t;

        // ------------------------------------------------------------------
        // Ignore list
        // ------------------------------------------------------------------

        private readonly ConcurrentDictionary<uint, byte> _ignoreList = new();

        /// <summary>Aggiunge il serial alla lista degli ignorati.</summary>
        public virtual void Ignore(uint serial) { _ignoreList.TryAdd(serial, 0); }

        /// <summary>Rimuove il serial dalla lista degli ignorati.</summary>
        public virtual void UnIgnore(uint serial) { _ignoreList.TryRemove(serial, out _); }

        /// <summary>True se il serial è nella lista degli ignorati.</summary>
        public virtual bool IsIgnored(uint serial) => _ignoreList.ContainsKey(serial);

        /// <summary>Svuota la lista degli ignorati.</summary>
        public virtual void ClearIgnores() { _ignoreList.Clear(); }

        /// <summary>Ritorna tutti i serial ignorati.</summary>
        public virtual List<uint> GetIgnoreList() => new List<uint>(_ignoreList.Keys);

        // ------------------------------------------------------------------
        // UO utility
        // ------------------------------------------------------------------

        /// <summary>Richiede resync al server (0x22 con flag 0xFF).</summary>
        public virtual void Resync()
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = { 0x22, 0xFF, 0x00 };
            _packetService.SendToServer(pkt);
        }

        /// <summary>
        /// Apre il context menu di un'entità (pacchetto 0xBF sub 0x0E).
        /// Il server risponderà con 0xBF sub 0x0F con le voci del menu.
        /// </summary>
        public virtual void ContextMenu(uint serial)
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = new byte[9];
            pkt[0] = 0xBF;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 9);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x0E); // sub
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial);
            _packetService.SendToServer(pkt);
        }

        /// <summary>
        /// Porta la finestra UO in primo piano (SetForegroundWindow Win32 API).
        /// </summary>
        public virtual void FocusUO()
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var hwnd = _interop?.GetWindowHandle() ?? IntPtr.Zero;
                if (hwnd != IntPtr.Zero) SetForegroundWindow(hwnd);
            }
            catch { }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>Serial del player corrente.</summary>
        public virtual uint GetPlayerSerial()
        {
            _cancel.ThrowIfCancelled();
            return _world.Player?.Serial ?? 0;
        }

        // ------------------------------------------------------------------
        // Named Lists — compatibilità RazorEnhanced/UOSteam
        // ------------------------------------------------------------------

        /// <summary>Crea una lista nominata (vuota se già esiste).</summary>
        public virtual void CreateList(string name)
        {
            _cancel.ThrowIfCancelled();
            _lists.TryAdd(name, new List<object>());
        }

        /// <summary>Aggiunge un valore in fondo alla lista.</summary>
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

        /// <summary>Rimuove e restituisce il primo elemento dalla lista (null se vuota).</summary>
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

        /// <summary>Rimuove il primo elemento uguale a <paramref name="value"/> dalla lista.</summary>
        public virtual void RemoveList(string name, object value)
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return;
            lock (list) { list.Remove(value); }
        }

        /// <summary>Svuota una lista senza distruggerla.</summary>
        public virtual void ClearList(string name)
        {
            _cancel.ThrowIfCancelled();
            if (_lists.TryGetValue(name, out var list)) lock (list) { list.Clear(); }
        }

        /// <summary>Elimina completamente una lista.</summary>
        public virtual void DestroyList(string name)
        {
            _cancel.ThrowIfCancelled();
            _lists.TryRemove(name, out _);
        }

        /// <summary>True se la lista esiste (anche se vuota).</summary>
        public virtual bool ListExists(string name)
        {
            _cancel.ThrowIfCancelled();
            return _lists.ContainsKey(name);
        }

        /// <summary>Numero di elementi nella lista (0 se non esiste).</summary>
        public virtual int ListCount(string name)
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return 0;
            lock (list) { return list.Count; }
        }

        /// <summary>Copia snapshot della lista (thread-safe).</summary>
        public virtual List<object> GetList(string name)
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return new List<object>();
            lock (list) { return new List<object>(list); }
        }

        /// <summary>True se la lista contiene l'elemento.</summary>
        public virtual bool ListContains(string name, object value)
        {
            _cancel.ThrowIfCancelled();
            if (!_lists.TryGetValue(name, out var list)) return false;
            lock (list) { return list.Contains(value); }
        }
    }
}

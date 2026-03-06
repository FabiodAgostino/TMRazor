using System;
using System.Threading;
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
        private readonly ScriptCancellationController _cancel;
        private readonly Action<string>? _outputCallback;

        public MiscApi(IWorldService world, IPacketService packetService, IClientInteropService interop, ScriptCancellationController cancel, Action<string>? outputCallback = null)
        {
            _world = world;
            _packetService = packetService;
            _interop = interop;
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
            // TODO: Implementare flag HasTarget in WorldService o PacketService
            return WaitFor(() => false, timeoutMs); 
        }

        public virtual bool WaitGump(uint gumpId, int timeoutMs = 5000)
        {
            return WaitFor(() => _world.CurrentGump?.GumpId == gumpId, timeoutMs);
        }

        /// <summary>Scrive una riga sul pannello output della UI (non sulla chat UO).</summary>
        public virtual void Log(string message)
        {
            _outputCallback?.Invoke($"[Script] {message}");
        }

        /// <summary>Ritorna il timestamp Unix corrente in millisecondi.</summary>
        public virtual long Timestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

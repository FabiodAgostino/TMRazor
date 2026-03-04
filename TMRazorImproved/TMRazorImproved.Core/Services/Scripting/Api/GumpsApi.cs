using System;
using System.Buffers.Binary;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
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

        public virtual bool HasGump() => _world.CurrentGump != null;

        public virtual uint CurrentGump => _world.CurrentGump?.GumpId ?? 0;

        public virtual uint CurrentID() => CurrentGump;

        public virtual void SendAction(int buttonId, int[]? switches = null)
        {
            _cancel.ThrowIfCancelled();
            var gump = _world.CurrentGump;
            if (gump == null) return;

            int switchesCount = switches?.Length ?? 0;
            byte[] packet = new byte[23 + (switchesCount * 4)];
            packet[0] = 0xB1;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(1), (ushort)packet.Length);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(3), gump.Serial);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(7), gump.GumpId);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(11), (uint)buttonId);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(15), (uint)switchesCount); // Switches count
            
            int offset = 19;
            if (switches != null)
            {
                foreach (int sw in switches)
                {
                    BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(offset), (uint)sw);
                    offset += 4;
                }
            }

            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(offset), 0); // Text entries count

            _packet.SendToServer(packet);
            
            // Rimuoviamo il gump localmente dato che abbiamo risposto
            _world.RemoveGump(gump.GumpId);
        }

        public virtual void Close() => SendAction(0);

        /// <summary>Ritorna i dati completi del gump corrente se l'ID corrisponde.</summary>
        public virtual UOGump? GetGumpData(uint gumpId)
        {
            _cancel.ThrowIfCancelled();
            var g = _world.CurrentGump;
            if (g != null && g.GumpId == gumpId) return g;
            return null;
        }

        /// <summary>Attende la comparsa di un nuovo gump specifico.</summary>
        public virtual bool WaitForGump(uint gumpId, int timeoutMs = 5000)
        {
            // Se è già aperto, ritorna subito
            if (_world.CurrentGump?.GumpId == gumpId) return true;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            
            // Usiamo il messenger per ricevere il messaggio dal WorldPacketHandler
            var recipient = new object();
            _messenger.Register<GumpMessage>(recipient, (r, msg) =>
            {
                if (msg.Value.GumpId == gumpId)
                    tcs.TrySetResult(true);
            });

            try
            {
                using var cts = new CancellationTokenSource();
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _cancel.Token);
                cts.CancelAfter(timeoutMs);

                var task = tcs.Task;
                // WaitSync in .NET 6+ o Task.WhenAny per compatibilità
                if (Task.WhenAny(task, Task.Delay(timeoutMs, linkedCts.Token)).GetAwaiter().GetResult() == task)
                    return task.Result;

                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            finally
            {
                _messenger.UnregisterAll(recipient);
            }
        }

        /// <summary>Ritorna il numero di righe di testo presenti nel gump corrente.</summary>
        public virtual int GetLineCount()
        {
            _cancel.ThrowIfCancelled();
            return _world.CurrentGump?.Strings.Count ?? 0;
        }

        /// <summary>Ritorna il testo di una riga specifica del gump corrente (alias di GetStringLine).</summary>
        public virtual string GetGumpText(int index) => GetStringLine(index);

        public virtual uint LastGumpId() => CurrentGump;

        /// <summary>Ritorna il testo di una riga specifica del gump corrente.</summary>
        public virtual string GetStringLine(int index)
        {
            _cancel.ThrowIfCancelled();
            var gump = _world.CurrentGump;
            if (gump == null || index < 0 || index >= gump.Strings.Count) return string.Empty;
            return gump.Strings[index];
        }
    }
}

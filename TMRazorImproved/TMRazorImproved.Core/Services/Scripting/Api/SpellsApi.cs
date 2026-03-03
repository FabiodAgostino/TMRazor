using System;
using System.Text;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class SpellsApi
    {
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        public SpellsApi(IPacketService packet, ScriptCancellationController cancel)
        {
            _packet = packet;
            _cancel = cancel;
        }

        public virtual void Cast(int spellId)
        {
            _cancel.ThrowIfCancelled();
            string cmd = spellId.ToString();
            byte[] cmdBytes = Encoding.ASCII.GetBytes(cmd);
            byte[] packet = new byte[3 + 1 + cmdBytes.Length + 1];
            packet[0] = 0x12;
            ushort len = (ushort)packet.Length;
            packet[1] = (byte)(len >> 8);
            packet[2] = (byte)(len & 0xff);
            packet[3] = 0x56; // CastSpell
            Array.Copy(cmdBytes, 0, packet, 4, cmdBytes.Length);
            packet[packet.Length - 1] = 0x00; // Null-terminator

            _packet.SendToServer(packet);
        }

        public virtual void Cast(string name)
        {
            _cancel.ThrowIfCancelled();
            // TODO: Implementare dictionary mapping name -> ID
            // int spellId = GetSpellIdByName(name);
            // Cast(spellId);
        }

        public virtual void CastMagery(string name) => Cast(name);
        public virtual void CastNecro(string name) => Cast(name);
        public virtual void CastChivalry(string name) => Cast(name);
        public virtual void CastBushido(string name) => Cast(name);
        public virtual void CastNinjitsu(string name) => Cast(name);
        public virtual void CastSpellweaving(string name) => Cast(name);
        public virtual void CastMysticism(string name) => Cast(name);
    }
}

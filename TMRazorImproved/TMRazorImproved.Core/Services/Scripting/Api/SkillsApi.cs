using System;
using System.Linq;
using System.Text;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class SkillsApi
    {
        private readonly ISkillsService _skillsService;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        public SkillsApi(ISkillsService skillsService, IPacketService packet, ScriptCancellationController cancel)
        {
            _skillsService = skillsService;
            _packet = packet;
            _cancel = cancel;
        }

        public virtual void UseSkill(string name)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (skill != null)
            {
                // packet 0x12 type 0x24
                string cmd = $"{skill.ID + 1} 0";
                byte[] cmdBytes = Encoding.ASCII.GetBytes(cmd);
                byte[] packet = new byte[3 + 1 + cmdBytes.Length + 1];
                packet[0] = 0x12;
                ushort len = (ushort)packet.Length;
                packet[1] = (byte)(len >> 8);
                packet[2] = (byte)(len & 0xff);
                packet[3] = 0x24; // UseSkill
                Array.Copy(cmdBytes, 0, packet, 4, cmdBytes.Length);
                packet[packet.Length - 1] = 0x00;

                _packet.SendToServer(packet);
            }
        }

        public virtual void SetLock(string name, string lockType)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (skill == null) return;

            SkillLock lt = lockType.ToLower() switch
            {
                "up" => SkillLock.Up,
                "down" => SkillLock.Down,
                "locked" or "lock" => SkillLock.Lock,
                _ => SkillLock.Up
            };

            _skillsService.SetLock(skill.ID, lt);
        }

        public virtual double GetValue(string name)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return skill?.Value ?? 0;
        }

        public virtual double GetCap(string name)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return skill?.Cap ?? 100.0;
        }

        public virtual string GetLock(string name)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (skill == null) return "Up";

            return skill.Lock switch
            {
                SkillLock.Up => "Up",
                SkillLock.Down => "Down",
                SkillLock.Lock => "Locked",
                _ => "Up"
            };
        }

        /// <summary>Valore base (non modificato da buff/debuff) dello skill.</summary>
        public virtual double GetBase(string name)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return skill?.BaseValue ?? 0;
        }

        /// <summary>Alias di GetBase — compatibilità RazorEnhanced.</summary>
        public virtual double GetReal(string name) => GetBase(name);

        /// <summary>Delta dell'ultima variazione dello skill (positivo = guadagno).</summary>
        public virtual double GetDelta(string name)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return skill?.Delta ?? 0;
        }

        /// <summary>Attende un gain sullo skill specificato entro il timeout (ms). True se il gain è avvenuto.</summary>
        public virtual bool WaitGain(string name, int timeoutMs = 30000)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (skill == null) return false;

            double initial = skill.Value;
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                var current = _skillsService.Skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (current != null && current.Value > initial) return true;
                System.Threading.Thread.Sleep(500);
            }
            return false;
        }

        /// <summary>Lista di tutti gli skill disponibili (IDs + nomi).</summary>
        public virtual System.Collections.Generic.List<SkillInfo> GetAll()
        {
            _cancel.ThrowIfCancelled();
            return new System.Collections.Generic.List<SkillInfo>(_skillsService.Skills);
        }
    }
}

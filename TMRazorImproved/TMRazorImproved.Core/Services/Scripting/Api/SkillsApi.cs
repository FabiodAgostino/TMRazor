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
    }
}

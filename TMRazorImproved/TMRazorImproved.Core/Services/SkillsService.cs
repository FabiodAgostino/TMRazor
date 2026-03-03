using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;
using System.Buffers.Binary;

namespace TMRazorImproved.Core.Services
{
    public class SkillsService : ISkillsService, IRecipient<UOPacketMessage>
    {
        private readonly List<SkillInfo> _skills = new();
        private readonly IPacketService _packetService;
        private readonly ILogger<SkillsService> _logger;

        public IReadOnlyList<SkillInfo> Skills => _skills;

        public double TotalReal => _skills.Sum(s => s.Value);
        public double TotalBase => _skills.Sum(s => s.BaseValue);

        public SkillsService(IPacketService packetService, IMessenger messenger, ILogger<SkillsService> logger)
        {
            _packetService = packetService;
            _logger = logger;
            messenger.Register<UOPacketMessage>(this);

            InitializeSkills();
        }

        private void InitializeSkills()
        {
            string[] names = {
                "Alchemy", "Anatomy", "Animal Lore", "Item ID", "Arms Lore", "Parrying", "Begging", "Blacksmithing", "Bowcraft", "Peacemaking",
                "Camping", "Carpentry", "Cartography", "Cooking", "Detect Hidden", "Discordance", "Eval Int", "Healing", "Fishing", "Forensic Eval",
                "Herding", "Hiding", "Provocation", "Inscription", "Lockpicking", "Magery", "Resist Spells", "Tactics", "Snooping", "Musicianship",
                "Poisoning", "Archery", "Spirit Speak", "Stealing", "Tailoring", "Animal Taming", "Taste ID", "Tinkering", "Tracking", "Veterinary",
                "Swordsmanship", "Maces", "Fencing", "Wrestling", "Lumberjacking", "Mining", "Meditation", "Stealth", "Remove Trap", "Necromancy",
                "Focus", "Chivalry", "Bushido", "Ninjitsu", "Spellweaving", "Mysticism", "Imbuing", "Throwing"
            };

            for (int i = 0; i < names.Length; i++)
            {
                _skills.Add(new SkillInfo(i, names[i]));
            }
        }

        public void Receive(UOPacketMessage message)
        {
            if (message.Path != Shared.Enums.PacketPath.ServerToClient) return;
            byte[] data = message.Value.Data;

            if (data.Length > 0 && data[0] == 0x3A) // Skill Update
            {
                HandleSkillUpdate(data);
            }
        }

        private void HandleSkillUpdate(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x3A
            ushort length = reader.ReadUInt16();
            byte type = reader.ReadByte();

            if (type == 0xFF) // Full list
            {
                for (int i = 0; i < _skills.Count; i++)
                {
                    ushort skillId = reader.ReadUInt16();
                    if (skillId > 0 && skillId <= _skills.Count)
                    {
                        var skill = _skills[skillId - 1];
                        UpdateSkill(skill, reader);
                    }
                }
            }
            else // Single skill
            {
                ushort skillId = reader.ReadUInt16();
                if (skillId >= 0 && skillId < _skills.Count)
                {
                    UpdateSkill(_skills[skillId], reader);
                }
            }
        }

        private void UpdateSkill(SkillInfo skill, UOBufferReader reader)
        {
            ushort val = reader.ReadUInt16();
            ushort baseVal = reader.ReadUInt16();
            byte lockType = reader.ReadByte();
            ushort cap = reader.ReadUInt16();

            double newValue = val / 10.0;
            double newBaseValue = baseVal / 10.0;

            if (skill.Value != 0 && newValue != skill.Value)
            {
                skill.Delta += (newValue - skill.Value);
            }

            skill.Value = newValue;
            skill.BaseValue = newBaseValue;
            skill.Lock = (SkillLock)lockType;
            skill.Cap = cap / 10.0;
        }

        public void ResetDelta()
        {
            foreach (var s in _skills) s.Delta = 0;
        }

        public void SetLock(int skillId, SkillLock lockType)
        {
            // Pacchetto 0x3A per settare il lock lato server
            byte[] packet = new byte[6];
            packet[0] = 0x3A;
            // ... pacchetto specifico per il client ...
            // In una implementazione reale, qui invieremmo il pacchetto tramite IPacketService
        }
    }
}

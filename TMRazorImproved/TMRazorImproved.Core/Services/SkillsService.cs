using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;
using System.Buffers.Binary;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    // FIX P0-02: cambiato da IRecipient<UOPacketMessage> a IRecipient<SkillsUpdatedMessage>.
    // In precedenza SkillsService ascoltava UOPacketMessage raw e filtrava per 0x3A,
    // ma WorldPacketHandler.HandleSkillsUpdate invia SkillsUpdatedMessage — architettura duplicata
    // e non allineata. Ora SkillsService riceve il messaggio tipizzato corretto.
    public class SkillsService : ISkillsService, IRecipient<SkillsUpdatedMessage>
    {
        private readonly List<SkillInfo> _skills = new();
        private readonly List<SkillGainRecord> _gainHistory = new();
        private readonly object _skillsLock = new(); // FIX P2-01: protezione thread-safety
        private readonly IPacketService _packetService;
        private readonly ILogger<SkillsService> _logger;

        public IReadOnlyList<SkillInfo> Skills { get { lock (_skillsLock) return _skills.AsReadOnly(); } }
        public IReadOnlyList<SkillGainRecord> GainHistory { get { lock (_skillsLock) return _gainHistory.AsReadOnly(); } }

        public double TotalReal { get { lock (_skillsLock) return _skills.Sum(s => s.Value); } }
        public double TotalBase { get { lock (_skillsLock) return _skills.Sum(s => s.BaseValue); } }

        public SkillsService(IPacketService packetService, IMessenger messenger, ILogger<SkillsService> logger)
        {
            _packetService = packetService;
            _logger = logger;
            messenger.Register<SkillsUpdatedMessage>(this);

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

        public void Receive(SkillsUpdatedMessage message)
        {
            byte[] data = message.Value;
            if (data.Length > 0 && data[0] == 0x3A)
                HandleSkillUpdate(data);
        }

        private void HandleSkillUpdate(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x3A
            ushort length = reader.ReadUInt16();
            byte type = reader.ReadByte();

            lock (_skillsLock) // FIX P2-01: lock su tutta la sequenza di aggiornamento
            {
                if (type == 0xFF) // Full list — ogni entry è (skillId 1-based, val, baseVal, lock, cap), terminata da skillId=0
                {
                    while (reader.Remaining >= 2)
                    {
                        ushort skillId = reader.ReadUInt16();
                        if (skillId == 0) break; // terminator

                        if (reader.Remaining < 7) break; // packet troncato

                        int idx = skillId - 1; // converti da 1-based a 0-based
                        if (idx >= 0 && idx < _skills.Count)
                        {
                            UpdateSkill(_skills[idx], reader);
                        }
                        else
                        {
                            // skill fuori range: salta i 7 byte del body (val+baseVal+lock+cap)
                            reader.Skip(7);
                        }
                    }
                }
                else // Single skill — skillId 0-based
                {
                    ushort skillId = reader.ReadUInt16();
                    if (skillId < _skills.Count)
                    {
                        UpdateSkill(_skills[skillId], reader);
                    }
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

                if (newValue > skill.Value)
                {
                    _gainHistory.Add(new SkillGainRecord(DateTime.Now, skill.Name, skill.Value, newValue));
                }
            }

            skill.Value = newValue;
            skill.BaseValue = newBaseValue;
            skill.Lock = (SkillLock)lockType;
            skill.Cap = cap / 10.0;
        }

        public void ResetDelta()
        {
            lock (_skillsLock)
            {
                foreach (var s in _skills) s.Delta = 0;
            }
        }

        public void SetLock(int skillId, SkillLock lockType)
        {
            SkillInfo? skill;
            lock (_skillsLock)
            {
                if (skillId < 0 || skillId >= _skills.Count) return;
                skill = _skills[skillId];
                skill.Lock = lockType;
            }

            _logger.LogDebug("Setting Skill Lock: {SkillName} to {LockType}", skill.Name, lockType);
            _packetService.SendToServer(PacketBuilder.SetSkillLock(skillId, (byte)lockType));
        }
    }
}

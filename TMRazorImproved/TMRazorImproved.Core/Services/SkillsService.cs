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

        public IReadOnlyList<SkillInfo> Skills { get { lock (_skillsLock) return new List<SkillInfo>(_skills); } }
        public IReadOnlyList<SkillGainRecord> GainHistory { get { lock (_skillsLock) return new List<SkillGainRecord>(_gainHistory); } }

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

            // Dump first 40 bytes for format analysis
            var hexDump = string.Join(" ", System.Linq.Enumerable.Range(0, Math.Min(40, data.Length)).Select(i => data[i].ToString("X2")));
            System.Diagnostics.Trace.WriteLine($"[SkillsService] HandleSkillUpdate type=0x{type:X2} dataLen={data.Length} bytes[0..39]: {hexDump}");

            lock (_skillsLock)
            {
                switch (type)
                {
                    case 0x02: // Lista completa CON cap (client moderni) — skillId 1-based, terminata da id=0
                    {
                        int updated = 0;
                        while (reader.Remaining >= 2)
                        {
                            int posBefore = reader.Position;
                            ushort skillId = reader.ReadUInt16();
                            System.Diagnostics.Trace.WriteLine($"[SkillsService] 0x02 loop: pos={posBefore} skillId={skillId} remaining={reader.Remaining}");
                            if (skillId == 0) { System.Diagnostics.Trace.WriteLine("[SkillsService] 0x02: terminator (skillId=0)"); break; }
                            if (reader.Remaining < 7) { System.Diagnostics.Trace.WriteLine($"[SkillsService] 0x02: not enough data (remaining={reader.Remaining})"); break; }
                            int idx = skillId - 1;
                            EnsureSkillSlot(idx);
                            if (idx >= 0 && idx < _skills.Count)
                            {
                                UpdateSkill(_skills[idx], ref reader, hasCap: true);
                                updated++;
                            }
                            else
                                reader.Skip(7);
                        }
                        System.Diagnostics.Trace.WriteLine($"[SkillsService] 0x02: updated {updated} skills. First skill: {(_skills.Count > 0 ? $"{_skills[0].Name}={_skills[0].Value}" : "none")}");
                        break;
                    }
                    case 0x00: // Lista completa SENZA cap (client vecchi) — skillId 1-based, terminata da id=0
                    {
                        while (reader.Remaining >= 2)
                        {
                            ushort skillId = reader.ReadUInt16();
                            if (skillId == 0) break;
                            if (reader.Remaining < 5) break;
                            int idx = skillId - 1;
                            EnsureSkillSlot(idx);
                            if (idx >= 0 && idx < _skills.Count)
                                UpdateSkill(_skills[idx], ref reader, hasCap: false);
                            else
                                reader.Skip(5);
                        }
                        break;
                    }
                    case 0xDF: // Singola skill CON cap (client moderni) — skillId 0-based
                    {
                        if (reader.Remaining < 9) break;
                        ushort skillId = reader.ReadUInt16();
                        EnsureSkillSlot(skillId);
                        if (skillId < _skills.Count)
                            UpdateSkill(_skills[skillId], ref reader, hasCap: true);
                        break;
                    }
                    case 0xFF: // Singola skill SENZA cap (client vecchi) — skillId 0-based
                    {
                        if (reader.Remaining < 7) break;
                        ushort skillId = reader.ReadUInt16();
                        EnsureSkillSlot(skillId);
                        if (skillId < _skills.Count)
                            UpdateSkill(_skills[skillId], ref reader, hasCap: false);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Espande la lista skill se lo shard ha skill custom con indice oltre quelli hardcoded.
        /// </summary>
        private void EnsureSkillSlot(int idx)
        {
            while (idx >= _skills.Count)
            {
                int newId = _skills.Count;
                _skills.Add(new SkillInfo(newId, $"Skill_{newId}"));
            }
        }

        private void UpdateSkill(SkillInfo skill, ref UOBufferReader reader, bool hasCap = true)
        {
            ushort val = reader.ReadUInt16();
            ushort baseVal = reader.ReadUInt16();
            byte lockType = reader.ReadByte();
            ushort cap = hasCap ? reader.ReadUInt16() : (ushort)1000;

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

        /// <summary>
        /// Tenta di leggere i nomi delle skill da skills.idx + skills.mul nella cartella dati
        /// del client (funziona per qualsiasi shard ClassicUO). Se i file non esistono o sono
        /// corrotti, i nomi hardcoded rimangono invariati.
        /// </summary>
        public void LoadNamesFromDataPath(string dataPath)
        {
            if (string.IsNullOrEmpty(dataPath)) return;
            string idxPath = System.IO.Path.Combine(dataPath, "skills.idx");
            string mulPath = System.IO.Path.Combine(dataPath, "skills.mul");
            if (!System.IO.File.Exists(idxPath) || !System.IO.File.Exists(mulPath)) return;

            try
            {
                byte[] idx = System.IO.File.ReadAllBytes(idxPath);
                byte[] mul = System.IO.File.ReadAllBytes(mulPath);
                int count = idx.Length / 12;

                lock (_skillsLock)
                {
                    for (int i = 0; i < count; i++)
                    {
                        int baseOff = i * 12;
                        int offset  = BitConverter.ToInt32(idx, baseOff);
                        int size    = BitConverter.ToInt32(idx, baseOff + 4);
                        if (offset < 0 || size <= 1 || offset + size > mul.Length) continue;

                        // Format: [1 byte use][name bytes, no null terminator]
                        int nameLen = size - 1;
                        string name = System.Text.Encoding.Latin1.GetString(mul, offset + 1, nameLen).TrimEnd('\0');
                        if (string.IsNullOrWhiteSpace(name)) continue;

                        EnsureSkillSlot(i);
                        _skills[i].SetName(name);
                    }
                }
                _logger.LogInformation("Loaded {Count} skill names from {Path}", count, dataPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load skill names from {Path}", dataPath);
            }
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
            // Feed a fake SkillUpdate S→C to ClassicUO so its skill gump reflects the new lock immediately
            _packetService.SendToClient(PacketBuilder.SkillUpdate(skillId, skill.Value, skill.BaseValue, skill.Cap, (byte)lockType));
        }
    }
}

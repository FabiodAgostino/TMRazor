using System;
using System.Collections.Generic;
using System.Linq;

namespace TMRazorImproved.Shared.Models
{
    public class SpellInfo
    {
        public int ID { get; }
        public string Name { get; }
        public string Circle { get; }

        public SpellInfo(int id, string name, string circle)
        {
            ID = id;
            Name = name;
            Circle = circle;
        }
    }

    public static class SpellDefinitions
    {
        private static readonly List<SpellInfo> _spells = new()
        {
            // Magery - Circle 1
            new SpellInfo(1, "Clumsy", "Magery 1"),
            new SpellInfo(2, "Create Food", "Magery 1"),
            new SpellInfo(3, "Feeblemind", "Magery 1"),
            new SpellInfo(4, "Heal", "Magery 1"),
            new SpellInfo(5, "Magic Arrow", "Magery 1"),
            new SpellInfo(6, "Night Sight", "Magery 1"),
            new SpellInfo(7, "Reactive Armor", "Magery 1"),
            new SpellInfo(8, "Weaken", "Magery 1"),

            // Magery - Circle 2
            new SpellInfo(9, "Agility", "Magery 2"),
            new SpellInfo(10, "Cunning", "Magery 2"),
            new SpellInfo(11, "Cure", "Magery 2"),
            new SpellInfo(12, "Harm", "Magery 2"),
            new SpellInfo(13, "Magic Trap", "Magery 2"),
            new SpellInfo(14, "Magic Untrap", "Magery 2"),
            new SpellInfo(15, "Protection", "Magery 2"),
            new SpellInfo(16, "Strength", "Magery 2"),

            // Magery - Circle 3
            new SpellInfo(17, "Bless", "Magery 3"),
            new SpellInfo(18, "Fireball", "Magery 3"),
            new SpellInfo(19, "Magic Lock", "Magery 3"),
            new SpellInfo(20, "Poison", "Magery 3"),
            new SpellInfo(21, "Telekinesis", "Magery 3"),
            new SpellInfo(22, "Teleport", "Magery 3"),
            new SpellInfo(23, "Unlock", "Magery 3"),
            new SpellInfo(24, "Wall of Stone", "Magery 3"),

            // Magery - Circle 4
            new SpellInfo(25, "Arch Cure", "Magery 4"),
            new SpellInfo(26, "Arch Protection", "Magery 4"),
            new SpellInfo(27, "Curse", "Magery 4"),
            new SpellInfo(28, "Fire Field", "Magery 4"),
            new SpellInfo(29, "Greater Heal", "Magery 4"),
            new SpellInfo(30, "Lightning", "Magery 4"),
            new SpellInfo(31, "Mana Drain", "Magery 4"),
            new SpellInfo(32, "Recall", "Magery 4"),

            // Magery - Circle 5
            new SpellInfo(33, "Blade Spirits", "Magery 5"),
            new SpellInfo(34, "Dispel Field", "Magery 5"),
            new SpellInfo(35, "Incognito", "Magery 5"),
            new SpellInfo(36, "Magic Reflection", "Magery 5"),
            new SpellInfo(37, "Mind Blast", "Magery 5"),
            new SpellInfo(38, "Paralyze", "Magery 5"),
            new SpellInfo(39, "Poison Field", "Magery 5"),
            new SpellInfo(40, "Summon Creature", "Magery 5"),

            // Magery - Circle 6
            new SpellInfo(41, "Dispel", "Magery 6"),
            new SpellInfo(42, "Energy Bolt", "Magery 6"),
            new SpellInfo(43, "Explosion", "Magery 6"),
            new SpellInfo(44, "Invisibility", "Magery 6"),
            new SpellInfo(45, "Mark", "Magery 6"),
            new SpellInfo(46, "Mass Curse", "Magery 6"),
            new SpellInfo(47, "Paralyze Field", "Magery 6"),
            new SpellInfo(48, "Reveal", "Magery 6"),

            // Magery - Circle 7
            new SpellInfo(49, "Chain Lightning", "Magery 7"),
            new SpellInfo(50, "Energy Field", "Magery 7"),
            new SpellInfo(51, "Flame Strike", "Magery 7"),
            new SpellInfo(52, "Gate Travel", "Magery 7"),
            new SpellInfo(53, "Mana Flare", "Magery 7"),
            new SpellInfo(54, "Meteor Swarm", "Magery 7"),
            new SpellInfo(55, "Polymorph", "Magery 7"),
            new SpellInfo(56, "Earthquake", "Magery 7"),

            // Magery - Circle 8
            new SpellInfo(57, "Energy Vortex", "Magery 8"),
            new SpellInfo(58, "Resurrection", "Magery 8"),
            new SpellInfo(59, "Summon Air Elemental", "Magery 8"),
            new SpellInfo(60, "Summon Daemon", "Magery 8"),
            new SpellInfo(61, "Summon Earth Elemental", "Magery 8"),
            new SpellInfo(62, "Summon Fire Elemental", "Magery 8"),
            new SpellInfo(63, "Summon Water Elemental", "Magery 8"),
            new SpellInfo(64, "Wildfire", "Magery 8"),

            // Necromancy
            new SpellInfo(101, "Animate Dead", "Necromancy"),
            new SpellInfo(102, "Blood Oath", "Necromancy"),
            new SpellInfo(103, "Corpse Skin", "Necromancy"),
            new SpellInfo(104, "Curse Weapon", "Necromancy"),
            new SpellInfo(105, "Evil Omen", "Necromancy"),
            new SpellInfo(106, "Horrific Beast", "Necromancy"),
            new SpellInfo(107, "Lich Form", "Necromancy"),
            new SpellInfo(108, "Mind Rot", "Necromancy"),
            new SpellInfo(109, "Pain Spike", "Necromancy"),
            new SpellInfo(110, "Poison Strike", "Necromancy"),
            new SpellInfo(111, "Strangle", "Necromancy"),
            new SpellInfo(112, "Summon Familiar", "Necromancy"),
            new SpellInfo(113, "Vampiric Embrace", "Necromancy"),
            new SpellInfo(114, "Vengeful Spirit", "Necromancy"),
            new SpellInfo(115, "Wither", "Necromancy"),
            new SpellInfo(116, "Wraith Form", "Necromancy"),
            new SpellInfo(117, "Exorcism", "Necromancy"),

            // Chivalry
            new SpellInfo(201, "Cleanse by Fire", "Chivalry"),
            new SpellInfo(202, "Close Wounds", "Chivalry"),
            new SpellInfo(203, "Consecrate Weapon", "Chivalry"),
            new SpellInfo(204, "Dispel Evil", "Chivalry"),
            new SpellInfo(205, "Divine Fury", "Chivalry"),
            new SpellInfo(206, "Enemy of One", "Chivalry"),
            new SpellInfo(207, "Holy Light", "Chivalry"),
            new SpellInfo(208, "Noble Sacrifice", "Chivalry"),
            new SpellInfo(209, "Remove Curse", "Chivalry"),
            new SpellInfo(210, "Sacred Journey", "Chivalry"),

            // Bushido
            new SpellInfo(401, "Honorable Execution", "Bushido"),
            new SpellInfo(402, "Confidence", "Bushido"),
            new SpellInfo(403, "Evasion", "Bushido"),
            new SpellInfo(404, "Counter Attack", "Bushido"),
            new SpellInfo(405, "Lightning Strike", "Bushido"),
            new SpellInfo(406, "Momentum Strike", "Bushido"),

            // Ninjitsu
            new SpellInfo(501, "Focus Attack", "Ninjitsu"),
            new SpellInfo(502, "Death Strike", "Ninjitsu"),
            new SpellInfo(503, "Animal Form", "Ninjitsu"),
            new SpellInfo(504, "Ki Attack", "Ninjitsu"),
            new SpellInfo(505, "Surprise Attack", "Ninjitsu"),
            new SpellInfo(506, "Backstab", "Ninjitsu"),
            new SpellInfo(507, "Shadow Jump", "Ninjitsu"),
            new SpellInfo(508, "Mirror Image", "Ninjitsu"),

            // Spellweaving
            new SpellInfo(601, "Arcane Circle", "Spellweaving"),
            new SpellInfo(602, "Gift of Renewal", "Spellweaving"),
            new SpellInfo(603, "Immolating Weapon", "Spellweaving"),
            new SpellInfo(604, "Attunement", "Spellweaving"),
            new SpellInfo(605, "Thunderstorm", "Spellweaving"),
            new SpellInfo(606, "Nature's Fury", "Spellweaving"),
            new SpellInfo(607, "Momentum Strike", "Spellweaving"),
            new SpellInfo(608, "Ethereal Voyage", "Spellweaving"),
            new SpellInfo(609, "Gift of Life", "Spellweaving"),
            new SpellInfo(610, "Arcane Empowerment", "Spellweaving"),
            new SpellInfo(611, "Wildfire", "Spellweaving"),
            new SpellInfo(612, "Essence of Wind", "Spellweaving"),
            new SpellInfo(613, "Dryad Bless", "Spellweaving"),
            new SpellInfo(614, "Etheral Burst", "Spellweaving"),
            new SpellInfo(615, "Manifestation", "Spellweaving")
        };

        public static IEnumerable<SpellInfo> All => _spells;

        public static bool TryGetSpellId(string name, out int id)
        {
            var spell = _spells.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (spell != null)
            {
                id = spell.ID;
                return true;
            }

            // Ricerca parziale se non trovata esatta
            spell = _spells.FirstOrDefault(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (spell != null)
            {
                id = spell.ID;
                return true;
            }

            // Fallback: Levenshtein distance per abbreviazioni e typo
            spell = _spells.OrderBy(s => ComputeLevenshteinDistance(s.Name.ToLower(), name.ToLower())).FirstOrDefault();
            if (spell != null && ComputeLevenshteinDistance(spell.Name.ToLower(), name.ToLower()) < 3)
            {
                id = spell.ID;
                return true;
            }

            id = 0;
            return false;
        }

        private static int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        public static string GetName(int id)
        {
            return _spells.FirstOrDefault(s => s.ID == id)?.Name ?? "Unknown";
        }
    }
}

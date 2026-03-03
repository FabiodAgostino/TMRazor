using System;
using System.Collections.Generic;
using System.Text;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class SpellsApi
    {
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        // FIX BUG-P1-02: dictionary completo spell name → ID per tutti i circoli UO
        private static readonly Dictionary<string, int> SpellIds = new(StringComparer.OrdinalIgnoreCase)
        {
            // Magery (1-64)
            { "Clumsy", 1 }, { "Create Food", 2 }, { "Feeblemind", 3 }, { "Heal", 4 },
            { "Magic Arrow", 5 }, { "Night Sight", 6 }, { "Reactive Armor", 7 }, { "Weaken", 8 },
            { "Agility", 9 }, { "Cunning", 10 }, { "Cure", 11 }, { "Harm", 12 },
            { "Magic Trap", 13 }, { "Magic Untrap", 14 }, { "Protection", 15 }, { "Strength", 16 },
            { "Bless", 17 }, { "Fireball", 18 }, { "Magic Lock", 19 }, { "Poison", 20 },
            { "Telekinesis", 21 }, { "Teleport", 22 }, { "Unlock", 23 }, { "Wall of Stone", 24 },
            { "Arch Cure", 25 }, { "Arch Protection", 26 }, { "Curse", 27 }, { "Fire Field", 28 },
            { "Greater Heal", 29 }, { "Lightning", 30 }, { "Mana Drain", 31 }, { "Recall", 32 },
            { "Blade Spirits", 33 }, { "Dispel Field", 34 }, { "Incognito", 35 }, { "Magic Reflection", 36 },
            { "Mind Blast", 37 }, { "Paralyze", 38 }, { "Poison Field", 39 }, { "Summon Creature", 40 },
            { "Dispel", 41 }, { "Energy Bolt", 42 }, { "Explosion", 43 }, { "Invisibility", 44 },
            { "Mark", 45 }, { "Mass Curse", 46 }, { "Paralyze Field", 47 }, { "Reveal", 48 },
            { "Chain Lightning", 49 }, { "Energy Field", 50 }, { "Flamestrike", 51 }, { "Gate Travel", 52 },
            { "Mana Vampire", 53 }, { "Mass Dispel", 54 }, { "Meteor Swarm", 55 }, { "Polymorph", 56 },
            { "Earthquake", 57 }, { "Energy Vortex", 58 }, { "Resurrection", 59 }, { "Air Elemental", 60 },
            { "Summon Daemon", 61 }, { "Earth Elemental", 62 }, { "Fire Elemental", 63 }, { "Water Elemental", 64 },
            // Necromancy (101-117)
            { "Animate Dead", 101 }, { "Blood Oath", 102 }, { "Corpse Skin", 103 }, { "Curse Weapon", 104 },
            { "Evil Omen", 105 }, { "Horrific Beast", 106 }, { "Lich Form", 107 }, { "Mind Rot", 108 },
            { "Pain Spike", 109 }, { "Poison Strike", 110 }, { "Strangle", 111 }, { "Summon Familiar", 112 },
            { "Vampiric Embrace", 113 }, { "Vengeful Spirit", 114 }, { "Wither", 115 }, { "Wraith Form", 116 },
            { "Exorcism", 117 },
            // Chivalry (201-210)
            { "Cleanse by Fire", 201 }, { "Close Wounds", 202 }, { "Consecrate Weapon", 203 },
            { "Dispel Evil", 204 }, { "Divine Fury", 205 }, { "Enemy of One", 206 },
            { "Holy Light", 207 }, { "Noble Sacrifice", 208 }, { "Remove Curse", 209 },
            { "Sacred Journey", 210 },
            // Bushido (401-406)
            { "Honorable Execution", 401 }, { "Confidence", 402 }, { "Counter Attack", 403 },
            { "Lightning Strike", 404 }, { "Momentum Strike", 405 }, { "Evasion", 406 },
            // Ninjitsu (501-506)
            { "Animal Form", 501 }, { "Back Stab", 502 }, { "Death Strike", 503 },
            { "Focus Attack", 504 }, { "Ki Attack", 505 }, { "Mirror Image", 506 },
            // Spellweaving (601-611)
            { "Arcane Circle", 601 }, { "Gift of Renewal", 602 }, { "Immolating Weapon", 603 },
            { "Reaper Form", 604 }, { "Wildfire", 605 }, { "Essence of Wind", 606 },
            { "Dryad Allure", 607 }, { "Ethereal Voyage", 608 }, { "Word of Death", 609 },
            { "Gift of Life", 610 }, { "Arcane Empowerment", 611 },
            // Mysticism (678-693)
            { "Nether Bolt", 678 }, { "Healing Stone", 679 }, { "Purge Magic", 680 }, { "Enchant", 681 },
            { "Sleep", 682 }, { "Eagle Strike", 683 }, { "Animated Weapon", 684 }, { "Stone Form", 685 },
            { "SpellTrigger", 686 }, { "Mass Sleep", 687 }, { "Cleansing Winds", 688 }, { "Bombard", 689 },
            { "Spell Plague", 690 }, { "Hail Storm", 691 }, { "Nether Cyclone", 692 }, { "Rising Colossus", 693 },
        };

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
            packet[packet.Length - 1] = 0x00;

            _packet.SendToServer(packet);
        }

        public virtual void Cast(string name)
        {
            _cancel.ThrowIfCancelled();
            // FIX BUG-P1-02: ricerca esatta, poi parziale case-insensitive
            if (SpellIds.TryGetValue(name, out int spellId))
            {
                Cast(spellId);
                return;
            }
            // Partial match (es. "Greater" corrisponde a "Greater Heal")
            foreach (var kv in SpellIds)
            {
                if (kv.Key.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    Cast(kv.Value);
                    return;
                }
            }
        }

        public virtual void CastMagery(string name) => Cast(name);
        public virtual void CastNecro(string name) => Cast(name);
        public virtual void CastChivalry(string name) => Cast(name);
        public virtual void CastBushido(string name) => Cast(name);
        public virtual void CastNinjitsu(string name) => Cast(name);
        public virtual void CastSpellweaving(string name) => Cast(name);
        public virtual void CastMysticism(string name) => Cast(name);

        /// <summary>Ritorna l'ID di un incantesimo dal suo nome. 0 se non trovato.</summary>
        public virtual int GetSpellId(string name)
        {
            _cancel.ThrowIfCancelled();
            return SpellIds.TryGetValue(name, out int id) ? id : 0;
        }

        /// <summary>API statica per uso interno (es. PlayerApi.Cast) senza dipendenza circolare.</summary>
        internal static bool TryGetSpellId(string name, out int id)
        {
            if (SpellIds.TryGetValue(name, out id)) return true;
            foreach (var kv in SpellIds)
            {
                if (kv.Key.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    id = kv.Value;
                    return true;
                }
            }
            id = 0;
            return false;
        }
    }
}

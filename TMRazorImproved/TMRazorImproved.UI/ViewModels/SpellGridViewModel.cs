using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public sealed partial class SpellGridViewModel : ViewModelBase, IDisposable
    {
        private readonly IPacketService _packet;
        private readonly IConfigService _config;
        private readonly DispatcherTimer _cooldownTimer;

        public ObservableCollection<SpellIcon> ActiveSpells { get; } = new();

        public IRelayCommand<SpellIcon> CastCommand { get; }

        public SpellGridViewModel(IPacketService packet, IConfigService config)
        {
            _packet = packet;
            _config = config;

            CastCommand = new RelayCommand<SpellIcon>(Cast);

            // Inizializza timer cooldown (100ms precision)
            _cooldownTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _cooldownTimer.Tick += CooldownTimer_Tick;
            _cooldownTimer.Start();

            LoadSpells();
        }

        private void LoadSpells()
        {
            // For now, load default magery spells
            ActiveSpells.Clear();
            AddMagerySpell(1, "Reactive Armor", 2.0);
            AddMagerySpell(2, "Clumsy", 1.0);
            AddMagerySpell(3, "Create Food", 1.0);
            AddMagerySpell(4, "Feeblemind", 1.0);
            AddMagerySpell(5, "Heal", 1.0);
            AddMagerySpell(6, "Magic Arrow", 1.0);
            AddMagerySpell(7, "Night Sight", 1.0);
            AddMagerySpell(8, "Weaken", 1.0);

            // 2nd Circle
            AddMagerySpell(9, "Agility", 1.5);
            AddMagerySpell(10, "Cunning", 1.5);
            AddMagerySpell(11, "Cure", 1.5);
            AddMagerySpell(12, "Harm", 1.5);
            AddMagerySpell(13, "Magic Trap", 1.5);
            AddMagerySpell(14, "Magic Untrap", 1.5);
            AddMagerySpell(15, "Protection", 1.5);
            AddMagerySpell(16, "Strength", 1.5);

            // 3rd Circle
            AddMagerySpell(17, "Bless", 2.0);
            AddMagerySpell(18, "Fireball", 2.0);
            AddMagerySpell(19, "Magic Lock", 2.0);
            AddMagerySpell(20, "Poison", 2.0);
            AddMagerySpell(21, "Telekinesis", 2.0);
            AddMagerySpell(22, "Teleport", 2.0);
            AddMagerySpell(23, "Unlock", 2.0);
            AddMagerySpell(24, "Wall of Stone", 2.0);

            // 4th Circle
            AddMagerySpell(25, "Arch Cure", 2.5);
            AddMagerySpell(26, "Arch Protection", 2.5);
            AddMagerySpell(27, "Curse", 2.5);
            AddMagerySpell(28, "Fire Field", 2.5);
            AddMagerySpell(29, "Greater Heal", 2.5);
            AddMagerySpell(30, "Lightning", 2.5);
            AddMagerySpell(31, "Mana Drain", 2.5);
            AddMagerySpell(32, "Recall", 2.5);

            // High Circles (Selection)
            AddMagerySpell(43, "Invisibility", 4.0);
            AddMagerySpell(49, "Dispel Field", 5.0);
            AddMagerySpell(51, "Flamestrike", 5.0);
            AddMagerySpell(52, "Gate Travel", 5.0);
            AddMagerySpell(61, "Resurrection", 6.0);
        }

        private void AddMagerySpell(int id, string name, double cooldown)
        {
            ActiveSpells.Add(new SpellIcon 
            { 
                SpellId = id, 
                Name = name, 
                Category = "Magery",
                CooldownSeconds = cooldown
            });
        }

        private void Cast(SpellIcon? spell)
        {
            if (spell == null || spell.IsOnCooldown) return;

            // Start Cooldown
            spell.IsOnCooldown = true;
            spell.RemainingCooldown = spell.CooldownSeconds;

            // Invia pacchetto Action per cast (0x12 sub 0x27)
            string spellStr = $"{spell.SpellId}";
            byte[] spellBytes = System.Text.Encoding.ASCII.GetBytes(spellStr);
            byte[] pkt = new byte[4 + spellBytes.Length + 1];
            pkt[0] = 0x12; // Cast uses speech normally in old clients
            pkt[1] = (byte)(pkt.Length >> 8);
            pkt[2] = (byte)pkt.Length;
            pkt[3] = 0x27; // Type: Cast Spell
            Array.Copy(spellBytes, 0, pkt, 4, spellBytes.Length);
            _packet.SendToServer(pkt);
        }

        private void CooldownTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var spell in ActiveSpells.Where(s => s.IsOnCooldown))
            {
                spell.RemainingCooldown -= 0.1;
                if (spell.RemainingCooldown <= 0)
                {
                    spell.RemainingCooldown = 0;
                    spell.IsOnCooldown = false;
                }
            }
        }

        public void Dispose()
        {
            _cooldownTimer.Stop();
        }
    }
}

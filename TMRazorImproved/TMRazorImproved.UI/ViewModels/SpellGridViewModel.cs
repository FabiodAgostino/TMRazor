using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.UI.ViewModels
{
    public sealed partial class SpellGridViewModel : ViewModelBase, IDisposable
    {
        private readonly IPacketService _packet;
        private readonly IConfigService _config;
        private readonly DispatcherTimer _cooldownTimer;

        [ObservableProperty]
        private int _rows = 4;

        [ObservableProperty]
        private int _columns = 8;

        public ObservableCollection<SpellIcon> ActiveSpells { get; } = new();

        public SpellGridViewModel(IPacketService packet, IConfigService config)
        {
            _packet = packet;
            _config = config;

            _cooldownTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _cooldownTimer.Tick += CooldownTimer_Tick;
            _cooldownTimer.Start();

            LoadConfig();
        }

        private void LoadConfig()
        {
            var profile = _config.CurrentProfile;
            if (profile != null && profile.SpellGrid != null)
            {
                Rows = profile.SpellGrid.Rows;
                Columns = profile.SpellGrid.Columns;
                
                ActiveSpells.Clear();
                foreach (var spell in profile.SpellGrid.Spells)
                {
                    ActiveSpells.Add(spell);
                }
            }
            else
            {
                LoadDefaults();
            }
        }

        private void LoadDefaults()
        {
            ActiveSpells.Clear();
            // Default: Magery Circle 1-4
            for (int i = 0; i < 32; i++)
            {
                ActiveSpells.Add(new SpellIcon 
                { 
                    SpellId = i + 1, 
                    Name = $"Spell {i + 1}", 
                    Row = i / 8, 
                    Column = i % 8,
                    CooldownSeconds = 1.0 + (i / 8) * 0.5 
                });
            }
        }

        [RelayCommand]
        private void Cast(SpellIcon? spell)
        {
            if (spell == null || spell.IsOnCooldown) return;

            spell.IsOnCooldown = true;
            spell.RemainingCooldown = spell.CooldownSeconds;

            byte[] pkt = PacketBuilder.CastSpell(spell.SpellId);
            _packet.SendToServer(pkt);
        }

        [RelayCommand]
        private void Save()
        {
            var profile = _config.CurrentProfile;
            if (profile != null)
            {
                profile.SpellGrid ??= new Shared.Models.Config.SpellGridConfig();
                profile.SpellGrid.Rows = Rows;
                profile.SpellGrid.Columns = Columns;
                profile.SpellGrid.Spells = ActiveSpells.ToList();
                _config.Save();
            }
        }

        private void CooldownTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var spell in ActiveSpells.Where(s => s.IsOnCooldown))
            {
                spell.RemainingCooldown -= 0.05;
                if (spell.RemainingCooldown <= 0)
                {
                    spell.RemainingCooldown = 0;
                    spell.IsOnCooldown = false;
                }
                // CooldownProgress è notificato da SpellIcon.OnRemainingCooldownChanged
            }
        }

        public void Dispose()
        {
            _cooldownTimer.Tick -= CooldownTimer_Tick;
            _cooldownTimer.Stop();
        }
    }
}

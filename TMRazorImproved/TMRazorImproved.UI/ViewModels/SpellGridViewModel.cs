using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Core.Utilities;
using System.Threading.Tasks;
using Wpf.Ui;

namespace TMRazorImproved.UI.ViewModels
{
    public sealed partial class SpellGridViewModel : ViewModelBase, IDisposable
    {
        private readonly IPacketService _packet;
        private readonly IConfigService _config;
        private readonly IContentDialogService _dialogService;
        private readonly DispatcherTimer _cooldownTimer;

        [ObservableProperty]
        private int _rows = 4;

        [ObservableProperty]
        private int _columns = 8;

        [ObservableProperty]
        private double _windowX = double.NaN;

        [ObservableProperty]
        private double _windowY = double.NaN;

        public ObservableCollection<SpellIcon> ActiveSpells { get; } = new();

        public SpellGridViewModel(IPacketService packet, IConfigService config, IContentDialogService dialogService)
        {
            _packet = packet;
            _config = config;
            _dialogService = dialogService;

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
                WindowX = profile.SpellGrid.X;
                WindowY = profile.SpellGrid.Y;
                
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
        private async Task Configure(SpellIcon? spell)
        {
            if (spell == null) return;

            var input = new Wpf.Ui.Controls.NumberBox
            {
                Minimum = 1,
                Maximum = 700,
                Value = spell.SpellId,
                PlaceholderText = "Spell ID (es. 1 per Clumsy)"
            };

            var dialog = new Wpf.Ui.Controls.ContentDialog(_dialogService.GetDialogHost())
            {
                Title = "Configura Slot",
                Content = input,
                PrimaryButtonText = "Salva",
                CloseButtonText = "Annulla"
            };

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary && input.Value != null)
            {
                spell.SpellId = (int)input.Value;
                spell.Name = $"Spell {spell.SpellId}";
                // Triggera l'aggiornamento UI ricreando o forzando la notifica (ObservableObject)
                var index = ActiveSpells.IndexOf(spell);
                if (index >= 0)
                {
                    ActiveSpells[index] = spell;
                }
                Save();
            }
        }

        [RelayCommand]
        public void Save()
        {
            var profile = _config.CurrentProfile;
            if (profile != null)
            {
                profile.SpellGrid ??= new Shared.Models.Config.SpellGridConfig();
                profile.SpellGrid.Rows = Rows;
                profile.SpellGrid.Columns = Columns;
                profile.SpellGrid.X = WindowX;
                profile.SpellGrid.Y = WindowY;
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
            }
        }

        public void Dispose()
        {
            _cooldownTimer.Tick -= CooldownTimer_Tick;
            _cooldownTimer.Stop();
        }
    }
}


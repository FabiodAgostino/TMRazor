using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels.Agents
{
    public sealed partial class BandageHealViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly ILogService _log;
        private readonly ILanguageService _lang;
        private readonly object _lock = new();

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private int _hpStart = 80;

        [ObservableProperty]
        private bool _poisonPriority = true;

        [ObservableProperty]
        private uint _bandageSerial;

        [ObservableProperty]
        private string _bandageName = string.Empty;

        [ObservableProperty]
        private int _customDelay = 0;

        [ObservableProperty]
        private bool _healPoison = true;

        [ObservableProperty]
        private bool _healMortal = true;

        [ObservableProperty]
        private bool _hiddenStop = true;

        [ObservableProperty]
        private int _maxRange = 1;

        [ObservableProperty]
        private bool _showCountdown;

        [ObservableProperty]
        private bool _autoStart;

        [ObservableProperty]
        private string _targetType = "Self";

        [ObservableProperty]
        private bool _ignoreCount;

        [ObservableProperty]
        private bool _timeWithBuff;

        [ObservableProperty]
        private bool _useCustomBandage;

        [ObservableProperty]
        private int _customBandageID;

        [ObservableProperty]
        private int _customBandageColor;

        [ObservableProperty]
        private bool _sendTextMsg;

        [ObservableProperty]
        private string _textMsgTarget = "[band";

        [ObservableProperty]
        private string _textMsgSelf = "[bandself";

        [ObservableProperty]
        private bool _useNormalTarget;

        [ObservableProperty]
        private bool _poisonBlock;

        [ObservableProperty]
        private bool _mortalBlock;

        public ObservableCollection<string> TargetTypes { get; } = new() { "Self", "Last", "Friend", "Target" };
        public ObservableCollection<LogEntry> Logs { get; } = new();

        public IAsyncRelayCommand SetBandageCommand { get; }

        public BandageHealViewModel(IConfigService config, ITargetingService targeting, ILogService log, ILanguageService languageService)
        {
            _config = config;
            _targeting = targeting;
            _log = log;
            _lang = languageService;

            _bandageName = _lang.GetString("Agents.General.NotSet");

            EnableThreadSafeCollection(Logs, _lock);

            _log.OnNewLog += entry =>
            {
                if (entry.Source == "BandageHeal")
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Logs.Insert(0, entry);
                        if (Logs.Count > 50) Logs.RemoveAt(50);
                    });
                }
            };

            SetBandageCommand = new AsyncRelayCommand(SetBandageAsync);

            LoadConfig();
        }

        private void LoadConfig()
        {
            var bh = _config.CurrentProfile?.BandageHeal;
            if (bh != null)
            {
                IsEnabled = bh.Enabled;
                HpStart = bh.HpStart;
                PoisonPriority = bh.PoisonPriority;
                BandageSerial = bh.BandageSerial;
                BandageName = BandageSerial != 0 ? $"0x{BandageSerial:X8}" : _lang.GetString("Agents.General.NotSet");
                CustomDelay = bh.CustomDelay;
                HealPoison = bh.HealPoison;
                HealMortal = bh.HealMortal;
                HiddenStop = bh.HiddenStop;
                MaxRange = bh.MaxRange;
                ShowCountdown = bh.ShowCountdown;
                AutoStart = bh.AutoStart;
                TargetType = bh.TargetType;
                IgnoreCount = bh.IgnoreCount;
                TimeWithBuff = bh.TimeWithBuff;
                UseCustomBandage = bh.UseCustomBandage;
                CustomBandageID = bh.CustomBandageID;
                CustomBandageColor = bh.CustomBandageColor;
                SendTextMsg = bh.SendTextMsg;
                TextMsgTarget = bh.TextMsgTarget;
                TextMsgSelf = bh.TextMsgSelf;
                UseNormalTarget = bh.UseNormalTarget;
                PoisonBlock = bh.PoisonBlock;
                MortalBlock = bh.MortalBlock;
            }
        }

        private async Task SetBandageAsync()
        {
            StatusText = _lang.GetString("Agents.General.SelectItem");
            var targetInfo = await _targeting.AcquireTargetAsync(); var serial = targetInfo.Serial;
            if (serial != 0)
            {
                BandageSerial = serial;
                BandageName = $"0x{serial:X8}";
                if (_config.CurrentProfile?.BandageHeal != null)
                {
                    _config.CurrentProfile.BandageHeal.BandageSerial = serial;
                    _config.Save();
                }
                StatusText = $"{_lang.GetString("Agents.General.ContainerSet")} {BandageName}";
            }
        }

        partial void OnIsEnabledChanged(bool value) => SaveConfig();
        partial void OnHpStartChanged(int value) => SaveConfig();
        partial void OnPoisonPriorityChanged(bool value) => SaveConfig();
        partial void OnCustomDelayChanged(int value) => SaveConfig();
        partial void OnHealPoisonChanged(bool value) => SaveConfig();
        partial void OnHealMortalChanged(bool value) => SaveConfig();
        partial void OnHiddenStopChanged(bool value) => SaveConfig();
        partial void OnMaxRangeChanged(int value) => SaveConfig();
        partial void OnShowCountdownChanged(bool value) => SaveConfig();
        partial void OnAutoStartChanged(bool value) => SaveConfig();
        partial void OnTargetTypeChanged(string value) => SaveConfig();
        partial void OnIgnoreCountChanged(bool value) => SaveConfig();
        partial void OnTimeWithBuffChanged(bool value) => SaveConfig();
        partial void OnUseCustomBandageChanged(bool value) => SaveConfig();
        partial void OnCustomBandageIDChanged(int value) => SaveConfig();
        partial void OnCustomBandageColorChanged(int value) => SaveConfig();
        partial void OnSendTextMsgChanged(bool value) => SaveConfig();
        partial void OnTextMsgTargetChanged(string value) => SaveConfig();
        partial void OnTextMsgSelfChanged(string value) => SaveConfig();
        partial void OnUseNormalTargetChanged(bool value) => SaveConfig();
        partial void OnPoisonBlockChanged(bool value) => SaveConfig();
        partial void OnMortalBlockChanged(bool value) => SaveConfig();

        private void SaveConfig()
        {
            if (_config.CurrentProfile?.BandageHeal != null)
            {
                var bh = _config.CurrentProfile.BandageHeal;
                bh.Enabled = IsEnabled;
                bh.HpStart = HpStart;
                bh.PoisonPriority = PoisonPriority;
                bh.CustomDelay = CustomDelay;
                bh.HealPoison = HealPoison;
                bh.HealMortal = HealMortal;
                bh.HiddenStop = HiddenStop;
                bh.MaxRange = MaxRange;
                bh.ShowCountdown = ShowCountdown;
                bh.AutoStart = AutoStart;
                bh.TargetType = TargetType;
                bh.IgnoreCount = IgnoreCount;
                bh.TimeWithBuff = TimeWithBuff;
                bh.UseCustomBandage = UseCustomBandage;
                bh.CustomBandageID = CustomBandageID;
                bh.CustomBandageColor = CustomBandageColor;
                bh.SendTextMsg = SendTextMsg;
                bh.TextMsgTarget = TextMsgTarget;
                bh.TextMsgSelf = TextMsgSelf;
                bh.UseNormalTarget = UseNormalTarget;
                bh.PoisonBlock = PoisonBlock;
                bh.MortalBlock = MortalBlock;
                _config.Save();
            }
        }
    }
}

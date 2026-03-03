using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.Core.Services
{
    public class DPSMeterService : IDPSMeterService, IRecipient<DamageMessage>, IDisposable
    {
        private readonly IWorldService _worldService;
        private readonly System.Timers.Timer _timer;
        private DateTime _startTime;
        private readonly List<(DateTime Time, ushort Amount)> _damageHistory = new();
        private readonly Dictionary<uint, long> _targetDamage = new();

        public double CurrentDPS { get; private set; }
        public double MaxDPS { get; private set; }
        public long TotalDamage { get; private set; }
        public TimeSpan CombatTime => IsActive ? DateTime.Now - _startTime : _totalTime;
        public bool IsActive { get; private set; }

        public IReadOnlyDictionary<uint, long> TargetDamage => new ReadOnlyDictionary<uint, long>(_targetDamage);

        private TimeSpan _totalTime = TimeSpan.Zero;

        public event Action? Updated;

        public DPSMeterService(IMessenger messenger, IWorldService worldService)
        {
            _worldService = worldService;
            messenger.RegisterAll(this);

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => CalculateDPS();
        }

        public void Start()
        {
            if (IsActive) return;
            IsActive = true;
            _startTime = DateTime.Now;
            _timer.Start();
        }

        public void Stop()
        {
            if (!IsActive) return;
            IsActive = false;
            _timer.Stop();
            _totalTime += DateTime.Now - _startTime;
        }

        public void Reset()
        {
            Stop();
            TotalDamage = 0;
            MaxDPS = 0;
            CurrentDPS = 0;
            _totalTime = TimeSpan.Zero;
            lock (_damageHistory) { _damageHistory.Clear(); }
            lock (_targetDamage) { _targetDamage.Clear(); }
            Updated?.Invoke();
        }

        public void Receive(DamageMessage message)
        {
            var (serial, amount) = message.Value;
            
            // Heuristic: damage to our attack target is ours
            if (_worldService.Player != null && _worldService.Player.AttackTarget == serial && serial != 0)
            {
                if (!IsActive) Start();

                lock (_damageHistory)
                {
                    _damageHistory.Add((DateTime.Now, amount));
                    TotalDamage += (long)amount;
                }
                
                lock (_targetDamage)
                {
                    if (_targetDamage.ContainsKey(serial))
                        _targetDamage[serial] += amount;
                    else
                        _targetDamage[serial] = amount;
                }
                
                Updated?.Invoke();
            }
        }

        private void CalculateDPS()
        {
            if (!IsActive) return;

            DateTime now = DateTime.Now;
            DateTime windowStart = now.AddSeconds(-10); // 10s rolling window

            double dps = 0;
            lock (_damageHistory)
            {
                var windowDamage = _damageHistory.Where(d => d.Time >= windowStart).Sum(d => (int)d.Amount);
                dps = windowDamage / 10.0;
            }

            CurrentDPS = dps;
            if (dps > MaxDPS) MaxDPS = dps;
            
            Updated?.Invoke();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}

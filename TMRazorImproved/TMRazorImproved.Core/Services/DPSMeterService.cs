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

        // BUG-NEW-01 FIX: backing fields con Interlocked/Volatile per visibilità cross-thread
        // (timer thread scrive, UI thread legge — senza sync il JIT può cachare i valori)
        private readonly object _statsLock = new();
        private double _currentDPS;
        private double _maxDPS;
        private long _totalDamage;

        public double CurrentDPS { get { lock (_statsLock) return _currentDPS; } }
        public double MaxDPS { get { lock (_statsLock) return _maxDPS; } }
        public long TotalDamage { get { lock (_statsLock) return _totalDamage; } }
        public TimeSpan CombatTime => IsActive ? DateTime.Now - _startTime : _totalTime;
        public bool IsActive { get; private set; }

        public IReadOnlyDictionary<uint, long> TargetDamage
        {
            get { lock (_targetDamage) return new Dictionary<uint, long>(_targetDamage); }
        }

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
            lock (_statsLock) { _totalDamage = 0; _maxDPS = 0; _currentDPS = 0; }
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
                }
                lock (_statsLock) { _totalDamage += (long)amount; }
                
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

            lock (_statsLock)
            {
                _currentDPS = dps;
                if (dps > _maxDPS) _maxDPS = dps;
            }
            
            Updated?.Invoke();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}

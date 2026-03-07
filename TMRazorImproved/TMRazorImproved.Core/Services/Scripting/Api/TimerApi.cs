using System;
using System.Collections.Concurrent;
using System.Timers;
using TMRazorImproved.Core.Services.Scripting;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class ScriptTimer : System.Timers.Timer
    {
        internal string Name = string.Empty;
        internal string Message = string.Empty;
        internal DateTime DueTime;
        internal Action<string>? SendMessageCallback;

        public ScriptTimer() : base()
        {
            this.Elapsed += ElapsedAction;
        }

        protected new void Dispose()
        {
            this.Elapsed -= ElapsedAction;
            base.Dispose();
        }

        public double TimeLeft => (this.DueTime - DateTime.Now).TotalMilliseconds;

        public new void Start()
        {
            this.DueTime = DateTime.Now.AddMilliseconds(this.Interval);
            base.Start();
        }

        private void ElapsedAction(object? sender, ElapsedEventArgs e)
        {
            if (this.AutoReset)
                this.DueTime = DateTime.Now.AddMilliseconds(this.Interval);
        }
    }

    /// <summary>
    /// API per la gestione dei timer nominativi negli script.
    /// Compatibile con l'oggetto <c>Timer</c> di RazorEnhanced.
    /// </summary>
    public class TimerApi
    {
        private readonly ScriptCancellationController _cancel;
        private readonly MiscApi _miscApi;
        private static readonly ConcurrentDictionary<string, ScriptTimer> _timers = new();

        public static ConcurrentDictionary<string, ScriptTimer> Timers { get => _timers; }

        public TimerApi(ScriptCancellationController cancel, MiscApi miscApi)
        {
            _cancel = cancel;
            _miscApi = miscApi;
        }

        public virtual void Create(string name, int delay)
        {
            Create(name, delay, string.Empty);
        }

        public virtual void Create(string name, int delay, string message)
        {
            _cancel.ThrowIfCancelled();

            if (_timers.TryGetValue(name, out ScriptTimer? existingTimer))
            {
                existingTimer.Stop();
                existingTimer.Dispose();
                _timers.TryRemove(name, out _);
            }

            ScriptTimer newtimer = new();
            newtimer.Elapsed += OnTimedEvent;
            newtimer.Interval = delay;
            newtimer.Enabled = true;
            newtimer.Name = name;
            newtimer.Message = message;
            newtimer.SendMessageCallback = msg => _miscApi.SendMessage(msg);
            newtimer.Start();

            _timers[name] = newtimer;
        }

        private static void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            if (source is ScriptTimer t)
            {
                if (!string.IsNullOrEmpty(t.Message))
                    t.SendMessageCallback?.Invoke(t.Message);

                t.Stop();
                t.Dispose();
                _timers.TryRemove(t.Name, out _);
            }
        }

        public virtual bool Check(string name)
        {
            _cancel.ThrowIfCancelled();
            if (_timers.TryGetValue(name, out ScriptTimer? t))
            {
                return t != null;
            }
            return false;
        }

        public virtual int Remaining(string name)
        {
            _cancel.ThrowIfCancelled();
            if (_timers.TryGetValue(name, out ScriptTimer? t))
            {
                if (t != null)
                    return (int)t.TimeLeft;
            }
            return -1;
        }

        public virtual int Value(string name) => Remaining(name);

        public virtual void Remove(string name)
        {
            _cancel.ThrowIfCancelled();
            if (_timers.TryGetValue(name, out ScriptTimer? existingTimer))
            {
                existingTimer.Stop();
                existingTimer.Dispose();
                _timers.TryRemove(name, out _);
            }
        }

        public virtual bool Exists(string name)
        {
            _cancel.ThrowIfCancelled();
            return _timers.ContainsKey(name);
        }
    }
}

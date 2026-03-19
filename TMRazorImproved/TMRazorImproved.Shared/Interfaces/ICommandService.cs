using System;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ICommandService : IDisposable
    {
        void Start();
        void Stop();
    }
}

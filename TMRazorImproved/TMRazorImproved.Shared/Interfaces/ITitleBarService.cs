using System;
using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ITitleBarService
    {
        bool IsEnabled { get; set; }
        string Template { get; set; }
        event Action<string>? TitleChanged;
        void Start();
        Task StopAsync();
    }
}

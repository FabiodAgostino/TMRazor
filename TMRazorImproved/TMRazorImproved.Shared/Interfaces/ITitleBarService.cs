using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ITitleBarService
    {
        bool IsEnabled { get; set; }
        string Template { get; set; }
        void Start();
        Task StopAsync();
    }
}

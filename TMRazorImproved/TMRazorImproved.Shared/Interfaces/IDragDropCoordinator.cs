using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IDragDropCoordinator
    {
        Task<bool> RequestDragDrop(uint serial, uint destination, ushort amount = 1, int timeoutMs = 2000);
    }
}

using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IAgentService
    {
        bool IsRunning { get; }
        void Start();
        Task StopAsync();
    }
    
    public interface IAutoLootService : IAgentService
    {
        // Metodi specifici per l'AutoLoot se necessari (es. settaggio manuale del container)
    }

    public interface IScavengerService : IAgentService
    {
    }

    public interface IOrganizerService : IAgentService
    {
        // Evento lanciato al completamento dell'operazione di riordino
        event Action OnComplete;
    }

    public interface IBandageHealService : IAgentService
    {
    }

    public interface IDressService : IAgentService
    {
        void Dress(string listName);
        void Undress(string listName);
    }

    public interface IVendorService : IAgentService
    {
    }
}

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
        void ChangeList(string listName);
    }

    public interface IScavengerService : IAgentService
    {
        void ChangeList(string listName);
    }

    public interface IOrganizerService : IAgentService
    {
        void ChangeList(string listName);
        event Action OnComplete;
    }

    public interface IBandageHealService : IAgentService
    {
    }

    public interface IDressService : IAgentService
    {
        void ChangeList(string listName);
        void Dress(string listName);
        void Undress(string listName);
        void DressUp();
        void Undress();
    }

    public interface IVendorService : IAgentService
    {
        void ExecuteBuy(uint vendorSerial, System.Collections.Generic.List<(uint Serial, ushort Amount)> items);
        void ExecuteSell(uint vendorSerial, System.Collections.Generic.List<(uint Serial, ushort Amount)> items);
        void SetBuyList(string listName);
        void SetSellList(string listName);
        void ClearBuyList();
        void ClearSellList();
    }

    public interface IRestockService : IAgentService
    {
        void ChangeList(string listName);
        event Action OnComplete;
    }
}

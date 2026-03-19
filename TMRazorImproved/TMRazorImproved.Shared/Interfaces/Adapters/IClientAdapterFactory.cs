using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Shared.Interfaces.Adapters
{
    public interface IClientAdapterFactory
    {
        IClientAdapter CreateAdapter(string clientType);
        IClientAdapter CreateAdapter(ClientStartType clientType);
        IClientAdapter GetActiveAdapter();
        void SetActiveType(ClientStartType clientType);
    }
}

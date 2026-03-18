namespace TMRazorImproved.Shared.Interfaces.Adapters
{
    public interface IClientAdapterFactory
    {
        IClientAdapter CreateAdapter(string clientType);
    }
}

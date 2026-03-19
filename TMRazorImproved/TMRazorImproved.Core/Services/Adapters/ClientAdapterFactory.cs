using System;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Interfaces.Adapters;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Core.Services.Adapters
{
    public class ClientAdapterFactory : IClientAdapterFactory
    {
        private readonly IClientInteropService _interopService;
        private ClientStartType _activeType = ClientStartType.TmClient;
        private IClientAdapter? _activeAdapter;

        public ClientAdapterFactory(IClientInteropService interopService)
        {
            _interopService = interopService;
        }

        public IClientAdapter CreateAdapter(string clientType)
        {
            if (string.Equals(clientType, "OSI", StringComparison.OrdinalIgnoreCase))
                return new OsiClientAdapter(_interopService);
            if (string.Equals(clientType, "ClassicUO", StringComparison.OrdinalIgnoreCase))
                return new ClassicUOAdapter(_interopService);
            return new TmClientAdapter(_interopService);
        }

        public IClientAdapter CreateAdapter(ClientStartType clientType)
        {
            return clientType switch
            {
                ClientStartType.OSI => new OsiClientAdapter(_interopService),
                ClientStartType.ClassicUO => new ClassicUOAdapter(_interopService),
                _ => new TmClientAdapter(_interopService)
            };
        }

        public void SetActiveType(ClientStartType clientType)
        {
            _activeType = clientType;
            _activeAdapter = CreateAdapter(clientType);
        }

        public IClientAdapter GetActiveAdapter()
        {
            _activeAdapter ??= CreateAdapter(_activeType);
            return _activeAdapter;
        }
    }
}

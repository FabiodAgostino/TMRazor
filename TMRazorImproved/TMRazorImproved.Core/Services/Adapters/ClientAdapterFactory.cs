using System;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Interfaces.Adapters;

namespace TMRazorImproved.Core.Services.Adapters
{
    public class ClientAdapterFactory : IClientAdapterFactory
    {
        private readonly IClientInteropService _interopService;

        public ClientAdapterFactory(IClientInteropService interopService)
        {
            _interopService = interopService;
        }

        public IClientAdapter CreateAdapter(string clientType)
        {
            if (string.Equals(clientType, "ClassicUO", StringComparison.OrdinalIgnoreCase))
            {
                return new ClassicUOAdapter();
            }

            // Default to OSI client adapter which uses existing Interop logic
            return new OsiClientAdapter(_interopService);
        }
    }
}

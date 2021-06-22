using RestSharp;
using System;
using System.Threading.Tasks;

namespace APIGateway.Core.RoutingService
{
    public abstract class RoutingService
    {
        private static readonly string SharedIdentificatorPatern = "{sessionID}_{type}_{x}";

        public abstract Task<IRestResponse> SendRoutingRequest(string themaId, string externalId, string url, string preferedAgent = null);
        public string CreateSharedId(string sessionId, object externalId, RoutingType type, int number)
        {
            return SharedIdentificatorPatern
               .Replace("{sessionID}", sessionId)
               .Replace("{type}", Enum.GetName(typeof(RoutingType), type))
               .Replace("{x}", number.ToString());
        }
    }
}

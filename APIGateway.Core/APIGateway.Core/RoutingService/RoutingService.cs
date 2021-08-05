using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using RestSharp;

namespace APIGateway.Core.RoutingService
{
    public abstract class RoutingService
    {
      
        private static readonly string SharedIdentificatorPatern = "{sessionID}_{type}_{x}";

        public abstract Task<IRestResponse> SendRoutingRequest(string themaId, string externalId, string url, string sessionId, string routingCode,
            string preferredEmploy = null);

        public string CreateSharedId(string sessionId, RoutingType type, int number)
        {
            return SharedIdentificatorPatern
                .Replace("{sessionID}", sessionId)
                .Replace("{type}", Enum.GetName(typeof(RoutingType), type))
                .Replace("{x}", number.ToString());
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIGateway.Core.RoutingService
{
    public enum RoutingType
    {
        Video,
        Chat,
        Chatbot
    }

    public interface IRoutingManager
    {
        Task<string> SendRequestForRouting(string sessionId, RoutingType type, string themaId, string sessionUrl,
            string routingCode, string prefferedOperator = null);

        Task<RoutingRequest> OnRoutingRequestReceived(string sharedIdentificator, string employeeId, string markerId);
        Task<List<RoutingRequest>> GetWaitingRoutingRequests();
        Task DeleteOldRoutingRequests();
    }
}
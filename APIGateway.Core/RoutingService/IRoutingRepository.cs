using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIGateway.Core.RoutingService
{
    public interface IRoutingRepository
    {
        Task<int> GetRoutingsCount(string sessionId);
        Task AddNewRoutingRequest(string sessionId, string externalId);
        Task<RoutingRequest> SetResultOfRoutingRequest(string sharedIdentificator, string operatorId);
        Task<List<RoutingRequest>> GetWaitingRoutingRequests();
        Task DeleteOldRoutingRequests();
    }
}

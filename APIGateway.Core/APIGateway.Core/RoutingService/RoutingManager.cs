using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace APIGateway.Core.RoutingService
{
    public class RoutingManager : IRoutingManager
    {
        private readonly ILogger<RoutingManager> _log;
        private readonly IOperatorToSharedUserId _operatorToEmployee;
        private readonly IRoutingRepository _routingRepository;
        private readonly RoutingService _routingService;

        public RoutingManager(
            RoutingService routingService,
            IOperatorToSharedUserId operatorToEmployee,
            ILogger<RoutingManager> log,
            IRoutingRepository routingRepository)
        {
            _routingRepository = routingRepository;
            _routingService = routingService;
            _operatorToEmployee = operatorToEmployee;
            _log = log;
        }

        public async Task<string> SendRequestForRouting(string sessionId, RoutingType type, string themaId,
            string sessionUrl, string prefferedOperator = null)
        {
            //Translate mluvii operatorID to genesys operatorId
            string preferredEmploy = _operatorToEmployee.OperatorToEmployee(prefferedOperator);
            int number = await _routingRepository.GetRoutingsCount(sessionId);

            //Create shared ID between genesys and mluvii
            var externalId = _routingService.CreateSharedId(sessionId, type, type, number);

            //Send routing request to external routing system
            var res = await _routingService.SendRoutingRequest(themaId, externalId, sessionUrl, preferredEmploy);

            if (res.IsSuccessful)
            {
                //Save routing request and wait for response in API.
                await _routingRepository.AddNewRoutingRequest(sessionId, externalId);

                return externalId;
            }

            _log.LogError("Cannot create routing request: Response from genesys " + res.Content);
            throw new Exception("Cannot create routing request: Response from genesys " + res.Content);
        }

        public async Task<RoutingRequest> OnRoutingRequestReceived(string sharedIdentificator, string employeeId,
            string markerId)
        {
            var operatorId = _operatorToEmployee.EmployeeToOperator(employeeId);

            if (!string.IsNullOrEmpty(operatorId))
                return await _routingRepository.SetResultOfRoutingRequest(sharedIdentificator, operatorId);
            return await _routingRepository.SetResultOfRoutingRequest(sharedIdentificator, "unknown");
        }

        public async Task<List<RoutingRequest>> GetWaitingRoutingRequests()
        {
            return await _routingRepository.GetWaitingRoutingRequests();
        }

        public async Task DeleteOldRoutingRequests()
        {
            await _routingRepository.DeleteOldRoutingRequests();
        }
    }
}
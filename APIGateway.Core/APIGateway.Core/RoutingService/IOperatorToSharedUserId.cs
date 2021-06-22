using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIGateway.Core.RoutingService
{
    public interface IOperatorToSharedUserId
    {
        string EmployeeToOperator(string employeeId);
        string OperatorToEmployee(string operatorId);
    }
}

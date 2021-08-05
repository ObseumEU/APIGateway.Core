using System.Threading.Tasks;

namespace APIGateway.Core.RoutingService
{
    public interface IOperatorToSharedUserId
    {
        Task<string> EmployeeToOperator(string employeeId);
        Task<string> OperatorToEmployee(string operatorEmail);
    }
}
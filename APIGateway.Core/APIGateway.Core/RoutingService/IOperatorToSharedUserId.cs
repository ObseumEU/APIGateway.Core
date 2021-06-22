namespace APIGateway.Core.RoutingService
{
    public interface IOperatorToSharedUserId
    {
        string EmployeeToOperator(string employeeId);
        string OperatorToEmployee(string operatorId);
    }
}
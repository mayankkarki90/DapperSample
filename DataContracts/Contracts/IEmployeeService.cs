using DataContracts.Models;

namespace DataContracts.Contracts
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
    }
}

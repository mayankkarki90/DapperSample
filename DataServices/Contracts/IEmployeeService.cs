using DataContracts.Models;

namespace DataServices.Contracts
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
    }
}

using DataContracts.Models;

namespace DataContracts.Contracts
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllAsync();

        Task<Employee> GetByCodeAsync(string code);

        Task AddAsync(Employee employee);

        Task UpdateAsync(Employee employee);

        Task DeleteAsync(int id);
    }
}

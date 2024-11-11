using System.Data.SqlClient;
using Dapper;
using DataContracts.Contracts;
using DataContracts.Models;
using Microsoft.Extensions.Configuration;

namespace DataServices.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IConfiguration _configuration;

        public EmployeeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            var connection = GetConnection();
            var employees = await connection.QueryAsync<Employee>("Select * from Employee");
            return employees.ToList();
        }

        public Task<Employee> GetByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(Employee employee)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Employee employee)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string code)
        {
            throw new NotImplementedException();
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
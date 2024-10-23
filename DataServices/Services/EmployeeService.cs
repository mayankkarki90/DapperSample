using System.Data.SqlClient;
using Dapper;
using DataContracts.Models;
using DataServices.Contracts;
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

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
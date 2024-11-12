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
            using var connection = GetConnection();
            var employees = await connection.QueryAsync<Employee>("Select * from Employee");
            return employees.ToList();
        }

        public async Task<Employee> GetByCodeAsync(string code)
        {
            using var connection = GetConnection();
            var employee = await connection.QueryFirstOrDefaultAsync<Employee>("Select * from Employee where Code=@Code",
                        new { Code = code });
            return employee;
        }

        public async Task AddAsync(Employee employee)
        {
            using var connection = GetConnection();
            await connection.ExecuteAsync("Insert into Employee (FirstName, LastName, Code, DateOfBirth) Values (@FirstName, @LastName, @Code, @DateOfBirth)",
                employee);
        }

        public async Task UpdateAsync(Employee employee)
        {
            using var connection = GetConnection();
            await connection.ExecuteAsync(
                "Update Employee set FirstName=@FirstName, LastName=@LastName, DateOfBirth=@DateOfBirth where ID=@ID",
                employee);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = GetConnection();
            await connection.ExecuteAsync("Delete From Employee where ID=@ID", new { ID = id });
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
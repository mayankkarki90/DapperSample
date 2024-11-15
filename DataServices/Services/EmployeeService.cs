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

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var sql = @"SELECT E.*, ED.*, D.*, DSG.*, P.*
                        FROM Employee E
                        LEFT JOIN EmployeeDetails ED ON E.Id = ED.EmployeeId
                        LEFT JOIN Department D ON D.Id = ED.DepartmentId
                        LEFT JOIN Designation DSG ON DSG.Id = ED.DesignationId
                        LEFT JOIN EmployeeProjects EP ON E.Id = EP.EmployeeId
                        LEFT JOIN Project P ON P.Id = EP.ProjectId";
            var employeeDictionary = new Dictionary<int, Employee>();

            using var connection = GetConnection();
            var employees = await connection.QueryAsync<Employee, EmployeeDetails, Department, Designation, Project, Employee>(
                sql,
                (employee, employeeDetails, department, designation, project) =>
                {
                    if (!employeeDictionary.TryGetValue(employee.Id, out var currentEmployee))
                    {
                        currentEmployee = employee;
                        employeeDetails.Designation = designation;
                        employeeDetails.Department = department;
                        currentEmployee.Details = employeeDetails;
                        currentEmployee.Projects = new List<Project>();
                        employeeDictionary.Add(currentEmployee.Id, currentEmployee);
                    }

                    if (project != null && currentEmployee.Projects.All(p => p.Id != project.Id))
                    {
                        currentEmployee.Projects.Add(project);
                    }

                    return currentEmployee;
                }, splitOn: "EmployeeId, Id, Id, Id");

            return employeeDictionary.Values;
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
                "Update Employee set FirstName=@FirstName, LastName=@LastName, DateOfBirth=@DateOfBirth where Id=@Id",
                employee);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = GetConnection();
            await connection.ExecuteAsync("Delete From Employee where Id=@Id", new { Id = id });
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
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
            var sql = GetEmployeesSql(false);
            var employees = await QueryEmployeesAsync(sql);
            return employees;
        }

        public async Task<Employee> GetByCodeAsync(string code)
        {
            var sql = GetEmployeesSql(true);
            var employees = await QueryEmployeesAsync(sql, new { Code = code });
            return employees.FirstOrDefault();
        }

        public async Task AddAsync(Employee employee)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var departmentId = await GetOrCreateDepartmentAsync(connection, transaction,
                            employee.Details?.Department?.Name);
                        var designationId = await GetOrCreateDesignationAsync(connection, transaction,
                            employee.Details?.Designation?.Name);

                        var insertSql =
                            @"Insert into Employee (FirstName, LastName, Code, DateOfBirth) Values(@FirstName, @LastName, @Code, @DateOfBirth);
                                Select Cast(SCOPE_IDENTITY() as int)";
                        var newEmployeeId = await connection.QuerySingleAsync<int>(insertSql, employee, transaction);

                        await CreateEmployeeDetailsAsync(connection, transaction, new
                        {
                            EmployeeId = newEmployeeId,
                            DesignationId = designationId,
                            DepartmentId = departmentId,
                        });

                        if (employee.Projects != null && employee.Projects.Any())
                        {
                            foreach (var project in employee.Projects)
                            {
                                var projectId = await GetOrCreateProjectAsync(connection, transaction, project.Name);
                                await CreateEmployeeProjectsAsync(connection, transaction, new
                                {
                                    EmployeeId = newEmployeeId,
                                    ProjectId = projectId,
                                });
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task UpdateAsync(Employee employee)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var departmentId = await GetOrCreateDepartmentAsync(connection, transaction,
                            employee.Details?.Department?.Name);
                        var designationId = await GetOrCreateDesignationAsync(connection, transaction,
                            employee.Details?.Designation?.Name);

                        await connection.ExecuteAsync(
                            "Update Employee set FirstName=@FirstName, LastName=@LastName, DateOfBirth=@DateOfBirth where Id=@Id",
                            employee, transaction);

                        await UpdateEmployeeDetailsAsync(connection, transaction, new
                        {
                            EmployeeId = employee.Id,
                            DesignationId = designationId,
                            DepartmentId = departmentId,
                        });

                        await DeleteEmployeeProjectsAsync(connection, transaction, employee.Id);
                        if (employee.Projects != null && employee.Projects.Any())
                        {
                            foreach (var project in employee.Projects)
                            {
                                var projectId = await GetOrCreateProjectAsync(connection, transaction, project.Name);
                                await CreateEmployeeProjectsAsync(connection, transaction, new
                                {
                                    EmployeeId = employee.Id,
                                    ProjectId = projectId,
                                });
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await DeleteEmployeeProjectsAsync(connection, transaction, id);
                        await DeleteEmployeeDetailsAsync(connection, transaction, id);
                        await connection.ExecuteAsync("Delete From Employee where Id=@Id", new { Id = id }, transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        private string GetEmployeesSql(bool getByCode)
        {
            var sql = @"SELECT E.*, ED.*, D.*, DSG.*, P.*
                        FROM Employee E
                        LEFT JOIN EmployeeDetails ED ON E.Id = ED.EmployeeId
                        LEFT JOIN Department D ON D.Id = ED.DepartmentId
                        LEFT JOIN Designation DSG ON DSG.Id = ED.DesignationId
                        LEFT JOIN EmployeeProjects EP ON E.Id = EP.EmployeeId
                        LEFT JOIN Project P ON P.Id = EP.ProjectId";

            if (getByCode)
                sql += " WHERE E.Code = @Code";

            return sql;
        }

        private async Task<IEnumerable<Employee>> QueryEmployeesAsync(string sql, object? parameter = null)
        {
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
                }, parameter, splitOn: "EmployeeId, Id, Id, Id");

            return employeeDictionary.Values;
        }

        private async Task<int> GetOrCreateDepartmentAsync(SqlConnection connection, SqlTransaction transaction,
            string departmentName)
        {
            var checkSql = "Select Id from Department where Name=@Name";
            int? existingDepartmentId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql,
                    new { Name = departmentName }, transaction);
            if (existingDepartmentId.HasValue)
                return existingDepartmentId.Value;

            var insertSql = @"Insert into Department (Name) Values(@Name);
                                Select CAST(SCOPE_IDENTITY() as int);";
            var newDepartmentId = await connection.QuerySingleAsync<int>(insertSql,
                new { Name = departmentName }, transaction);
            return newDepartmentId;
        }

        private async Task<int> GetOrCreateDesignationAsync(SqlConnection connection, SqlTransaction transaction,
            string designation)
        {
            var checkSql = "Select Id from Designation where Name=@Name";
            var existingDesignationId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql, new { Name = designation }, transaction);
            if (existingDesignationId.HasValue)
                return existingDesignationId.Value;

            var insertSql = @"Insert into Designation (Name) Values(@Name);
                                Select CAST(SCOPE_IDENTITY() as int);";
            var newDesignationId = await connection.QuerySingleAsync<int>(insertSql, new { Name = designation }, transaction);
            return newDesignationId;
        }

        private async Task<int> GetOrCreateProjectAsync(SqlConnection connection, SqlTransaction transaction,
            string projectName)
        {
            var checkSql = "Select Id from Project where Name = @Name";
            var existingProjectId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql,
                new { Name = projectName }, transaction);
            if (existingProjectId.HasValue)
                return existingProjectId.Value;

            var insertSql = @"Insert into Project (Name) Values(@Name);
                                Select Cast(SCOPE_IDENTITY() as int);";
            var newProjectId = await connection.QuerySingleAsync<int>(insertSql, new { Name = projectName }, transaction);
            return newProjectId;
        }

        private async Task CreateEmployeeDetailsAsync(SqlConnection connection, SqlTransaction transaction,
            object parameter)
        {
            var insertEmployeeDetailSql = "Insert into EmployeeDetails (EmployeeId, DepartmentId, DesignationId) Values(@EmployeeId, @DepartmentId, @DesignationId)";
            await connection.ExecuteAsync(insertEmployeeDetailSql, parameter, transaction);
        }

        private async Task UpdateEmployeeDetailsAsync(SqlConnection connection, SqlTransaction transaction,
            object parameter)
        {
            var updateEmployeeDetailSql = @"Update EmployeeDetails 
                                            Set DepartmentId=@DepartmentId,
                                            DesignationId=@DesignationId
                                            Where EmployeeId=@EmployeeId";
            await connection.ExecuteAsync(updateEmployeeDetailSql, parameter, transaction);
        }

        private async Task DeleteEmployeeDetailsAsync(SqlConnection connection, SqlTransaction transaction,
            int employeeId)
        {
            var deleteSql = "Delete from EmployeeDetails where EmployeeId=@EmployeeId";
            await connection.ExecuteAsync(deleteSql, new { EmployeeId = employeeId }, transaction);
        }

        private async Task CreateEmployeeProjectsAsync(SqlConnection connection, SqlTransaction transaction,
            object parameter)
        {
            var insertEmployeeProjectsSql = "Insert into EmployeeProjects (EmployeeId, ProjectId) Values(@EmployeeId, @ProjectId)";
            await connection.ExecuteAsync(insertEmployeeProjectsSql, parameter, transaction);
        }

        private async Task DeleteEmployeeProjectsAsync(SqlConnection connection, SqlTransaction transaction,
            int employeeId)
        {
            var deleteSql = "Delete from EmployeeProjects where EmployeeId=@EmployeeId";
            await connection.ExecuteAsync(deleteSql, new { EmployeeId = employeeId }, transaction);
        }
    }
}
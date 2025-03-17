using AutoMapper;
using DapperSample.Models;
using DataContracts.Contracts;
using DataContracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace DapperSample.Controllers
{
    /// <summary>
    /// Api's for employee
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeController"/> class.
        /// </summary>
        /// <param name="employeeService">The employee service.</param>
        /// <param name="mapper">The mapper.</param>
        public EmployeeController(IEmployeeService employeeService, IMapper mapper)
        {
            _employeeService = employeeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets all employees.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EmployeeDto>>> GetAllAsync()
        {
            var employees = await _employeeService.GetAllAsync();
            var response = _mapper.Map<List<EmployeeDto>>(employees);
            return Ok(response);
        }

        /// <summary>
        /// Gets the employee by code.
        /// </summary>
        /// <param name="code">The employee code.</param>
        /// <returns></returns>
        /// Swagger gen UI failed to load on using Route method and HttpGet without route.
        /// Seems like it's necessary to provide route in HttpGet on using Name parameter
        /// for Swagger gen to work
        [HttpGet("code/{code}", Name = "GetByCode")]
        public async Task<ActionResult<EmployeeDto>> GetByCodeAsync(string code)
        {
            var employee = await _employeeService.GetByCodeAsync(code);
            if (employee == null)
            {
                return NotFound($"Employee with code '{code}' doesn't exist");
            }

            var response = _mapper.Map<EmployeeDto>(employee);
            return Ok(response);
        }

        /// <summary>Adds an employee.</summary>
        /// <param name="employee">The employee.</param>
        /// <returns>
        /// </returns>
        /// <remarks>
        /// Sample request:
        ///     POST
        ///     {
        ///        "firstName": "Mayank",
        ///        "lastName": "Karki",
        ///        "code": "AE$001",
        ///        "dateOfBirth": "2024-11-28T14:32:38.960Z",
        ///        "details": {
        ///            "department": {
        ///                "name": "Development"
        ///            },
        ///            "designation": {
        ///                "name": "Team Lead"
        ///            }
        ///        },
        ///    "projects": [
        ///    {
        ///        "name": "Rational Will"
        ///    }
        ///    ]
        ///}
        ///
        /// </remarks>
        /// <response code="201">A new employee created</response>
        [HttpPost]
        public async Task<ActionResult> AddAsync(EmployeeDto employee)
        {
            var employeeDb = _mapper.Map<Employee>(employee);
            await _employeeService.AddAsync(employeeDb);
            return CreatedAtAction("GetByCode", new { code = employee.Code }, employee);
        }

        /// <summary>Updates the employee.</summary>
        /// <param name="employee">The employee.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPut]
        public async Task<ActionResult> UpdateAsync(EmployeeDto employee)
        {
            var existingEmployee = await _employeeService.GetByCodeAsync(employee.Code);
            if (existingEmployee == null)
                return NotFound("Employee not found");

            var employeeDb = _mapper.Map<Employee>(employee);
            employeeDb.Id = existingEmployee.Id;
            await _employeeService.UpdateAsync(employeeDb);

            return NoContent();
        }

        /// <summary>Deletes the employee.</summary>
        /// <param name="code">The employee code.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpDelete("code/{code}")]
        public async Task<ActionResult> DeleteAsync(string code)
        {
            var existingEmployee = await _employeeService.GetByCodeAsync(code);
            if (existingEmployee == null)
                return NotFound("Employee not found");

            await _employeeService.DeleteAsync(existingEmployee.Id);
            return NoContent();
        }
    }
}

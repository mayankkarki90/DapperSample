using AutoMapper;
using DapperSample.Models;
using DataContracts.Contracts;
using DataContracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace DapperSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        public EmployeeController(IEmployeeService employeeService, IMapper mapper)
        {
            _employeeService = employeeService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<EmployeeDto>>> GetAllAsync()
        {
            var employees = await _employeeService.GetAllAsync();
            var response = _mapper.Map<List<EmployeeDto>>(employees);
            return Ok(response);
        }

        [HttpGet("/code/{code}", Name = "GetByCode")]
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

        [HttpPost]
        public async Task<ActionResult> AddAsync(EmployeeDto employee)
        {
            var employeeDb = _mapper.Map<Employee>(employee);
            await _employeeService.AddAsync(employeeDb);
            return CreatedAtAction("GetByCode", new { code = employee.Code }, employee);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateAsync(EmployeeDto employee)
        {
            var existingEmployee = await _employeeService.GetByCodeAsync(employee.Code);
            if (existingEmployee == null)
                return NotFound("Employee not found");

            var employeeDb = _mapper.Map<Employee>(employee);
            employeeDb.ID = existingEmployee.ID;
            await _employeeService.UpdateAsync(employeeDb);

            return NoContent();
        }

        [HttpDelete("/code/{code}")]
        public async Task<ActionResult> DeleteAsync(string code)
        {
            var existingEmployee = await _employeeService.GetByCodeAsync(code);
            if (existingEmployee == null)
                return NotFound("Employee not found");

            await _employeeService.DeleteAsync(existingEmployee.ID);
            return NoContent();
        }
    }
}

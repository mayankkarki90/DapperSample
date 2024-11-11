using AutoMapper;
using DapperSample.Models;
using DataContracts.Contracts;
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
        public async Task<ActionResult<EmployeeResponse>> GetAllAsync()
        {
            var employees = await _employeeService.GetAllAsync();
            var response = _mapper.Map<List<EmployeeResponse>>(employees);
            return Ok(response);
        }
    }
}

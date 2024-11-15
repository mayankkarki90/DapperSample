using DataContracts.Models;

namespace DapperSample.Models
{
    public class EmployeeDetailsDto
    {
        public DepartmentDto? Department { get; set; }

        public DesignationDto? Designation { get; set; }
    }
}

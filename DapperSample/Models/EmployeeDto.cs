namespace DapperSample.Models
{
    public class EmployeeDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Code { get; set; }

        public DateTime DateOfBirth { get; set; }

        public EmployeeDetailsDto? Details { get; set; }

        public ProjectDto[] Projects { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContracts.Models
{
    public class EmployeeDetails
    {
        public int EmployeeId { get; set; }

        public int DepartmentId { get; set; }

        public int DesignationId { get; set; }

        public Department? Department { get; set; }

        public Designation? Designation { get; set; }
    }
}

﻿namespace DataContracts.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Code { get; set; }

        public DateTime DateOfBirth { get; set; }

        public EmployeeDetails? Details { get; set; }

        public List<Project> Projects { get; set; }
    }
}
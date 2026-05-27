// /Models/Employee.cs
namespace RPACProductionPlanner.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public string Department { get; set; }
        public int? ShiftId { get; set; }
        public bool IsActive { get; set; }
    }
}

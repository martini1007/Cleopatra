namespace Cleopatra.Data;

public class Employee
{
    public int EmployeeId { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? HireDate { get; set; }
    public int? ScheduleId { get; set; }
}
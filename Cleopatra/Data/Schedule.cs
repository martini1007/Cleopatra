namespace Cleopatra.Data;

public class Schedule
{
    public int ScheduleId { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } // Navigation property
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}
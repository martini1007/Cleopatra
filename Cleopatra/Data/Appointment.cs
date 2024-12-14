namespace Cleopatra.Data;

public class Appointment
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } // Navigation property
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } // Navigation property
    public int ServiceId { get; set; }
    public Service Service { get; set; } // Navigation property
    public DateTime AppointmentDateTime { get; set; }
    public int? Duration { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
}
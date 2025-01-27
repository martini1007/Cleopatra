using System.ComponentModel.DataAnnotations;

namespace Cleopatra.Models;

public class AddScheduleModel
{
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public string StartTime { get; set; }
    
    [Required]
    public string EndTime { get; set; }
}
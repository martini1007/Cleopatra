using Cleopatra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Cleopatra.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly AppDbContext _context;

    public ScheduleController(AppDbContext context)
    {
        _context = context;
    }
    
    // GET : get schedule for employee
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedule(int id)
    {
        IEnumerable<Schedule>? employeesSchedules = await _context.Schedules
            .Where(s => s.EmployeeId == id)
            .Include(s => s.Employee)
            .ToListAsync();
        
        if (employeesSchedules is null || !employeesSchedules.Any())
        {
            return NotFound(new { Message = "No schedules found for this employee." });
        }
        
        return Ok(JsonSerializer.Serialize(employeesSchedules));
    }
}
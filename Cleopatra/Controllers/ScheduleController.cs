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
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
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
    
    // GET : get schedule for given day
    [HttpGet("{date:datetime}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedule(DateTime date)
    {
        IEnumerable<Schedule>? schedules = await _context.Schedules
            .Where(s => s.StartDateTime.Year == date.Year && s.StartDateTime.Month == date.Month && s.StartDateTime.Day == date.Day)
            .Include(s => s.Employee)
            .ToListAsync();
        
        if (schedules is null || !schedules.Any())
        {
            return NotFound(new { Message = "No schedules found for this date." });
        }
        
        return Ok(JsonSerializer.Serialize(schedules));
    }
    
    // GET : get all schedules
    [HttpGet()]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Schedule>>> GetAllSchedules()
    {
        return Ok(JsonSerializer.Serialize(await _context.Schedules.Include(s => s.Employee).ToListAsync()));
    }
}
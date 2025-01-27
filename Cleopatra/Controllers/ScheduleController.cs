using Cleopatra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cleopatra.Models;

namespace Cleopatra.Controllers;

[Authorize(Roles = "Admin")]
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
    [HttpGet]
    
    public async Task<ActionResult<IEnumerable<Schedule>>> GetAllSchedules()
    {
        return Ok(JsonSerializer.Serialize(await _context.Schedules.Include(s => s.Employee).ToListAsync()));
    }

    
    [HttpPost("MoveSchedule/{scheduleId:int}")]
    
    public async Task<ActionResult<Schedule>> MoveSchedule([FromQuery] int ScheduleId, string NewStartTime, string NewEndTime)
    {
        if (!DateTime.TryParse(NewStartTime, out DateTime startTime))
        {
            return BadRequest(new { Message = "Invalid start time format." });
        }
        
        if (!DateTime.TryParse(NewEndTime, out DateTime endTime))
        {
            return BadRequest(new { Message = "Invalid end time format." });
        }
        
        _context.Schedules.First(s => s.ScheduleId == ScheduleId).StartDateTime = startTime;
        _context.Schedules.First(s => s.ScheduleId == ScheduleId).EndDateTime = endTime;
        
        _context.SaveChanges();
        
        return Ok("Schedule moved.");
    }
    
    [HttpPut("AddSchedule")]
    public async Task<ActionResult<Schedule>> AddSchedule([FromBody] AddScheduleModel scheduleModel)
    {

        if (!DateTime.TryParse(scheduleModel.StartTime, out DateTime startTime))
        {
            return BadRequest(new { Message = "Invalid start time format." });
        }

        if (!DateTime.TryParse(scheduleModel.EndTime, out DateTime endTime))
        {
            return BadRequest(new { Message = "Invalid end time format." });
        }
        
        var newSchedule = new Schedule
        {
            EmployeeId = scheduleModel.EmployeeId,
            StartDateTime = startTime,
            EndDateTime = endTime
        };
        
        if (_context.Schedules.Contains(newSchedule))
        {
            return BadRequest(new { Message = "Schedule already exists." });
        }
        
        _context.Schedules.Add(newSchedule);
        await _context.SaveChangesAsync();
        
        return Ok("Schedule added.");
    }
}
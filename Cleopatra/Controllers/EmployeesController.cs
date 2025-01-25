using System.Text.Json;
using Cleopatra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cleopatra.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        IEnumerable<Employee>? employees = await _context.Employees.ToListAsync();
        
        if (employees is null || !employees.Any())
        {
            return NotFound(new { Message = "No employees found!" });
        }
        
        return Ok(JsonSerializer.Serialize(employees));
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesById(int id)
    {
        IEnumerable<Employee>? employees = await _context.Employees.Where(c => c.EmployeeId == id).ToListAsync();
        
        if (employees is null || !employees.Any())
        {
            return NotFound(new { Message = "No employee with provided id is found!" });
        }
        
        return Ok(JsonSerializer.Serialize(employees));
    }
}
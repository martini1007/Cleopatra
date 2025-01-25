using System.Text.Json;
using Cleopatra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cleopatra.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomersController :ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        IEnumerable<Customer>? customers = await _context.Customers.ToListAsync();
        
        if (customers is null || !customers.Any())
        {
            return NotFound(new { Message = "No customers found!" });
        }
        
        return Ok(JsonSerializer.Serialize(customers));
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersById(int id)
    {
        IEnumerable<Customer>? customers = await _context.Customers.Where(c => c.CustomerId == id).ToListAsync();
        
        if (customers is null || !customers.Any())
        {
            return NotFound(new { Message = "No customers with provided id found!" });
        }
        
        return Ok(JsonSerializer.Serialize(customers));
    }
}
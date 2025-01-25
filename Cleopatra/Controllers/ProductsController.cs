using System.Text.Json;
using Cleopatra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cleopatra.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetEmployees()
    {
        IEnumerable<Product>? products = await _context.Products.ToListAsync();
        
        if (products is null || !products.Any())
        {
            return NotFound(new { Message = "No products found!" });
        }
        
        return Ok(JsonSerializer.Serialize(products));
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetEmployeesById(int id)
    {
        IEnumerable<Product>? products = await _context.Products.Where(c => c.ProductId == id).ToListAsync();
        
        if (products is null || !products.Any())
        {
            return NotFound(new { Message = "No product with provided id found!" });
        }
        
        return Ok(JsonSerializer.Serialize(products));
    }
}
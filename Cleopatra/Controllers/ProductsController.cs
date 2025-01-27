using System.Text.Json;
using Cleopatra.Data;
using Cleopatra.Models;
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
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
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
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsById(int id)
    {
        IEnumerable<Product>? products = await _context.Products.Where(c => c.ProductId == id).ToListAsync();
        
        if (products is null || !products.Any())
        {
            return NotFound(new { Message = "No product with provided id found!" });
        }
        
        return Ok(JsonSerializer.Serialize(products));
    }
    
    [HttpPost]
    [Route("AddProduct")]
    public async Task<ActionResult<Product>> AddProduct([FromBody] AddProductModel addProductModel)
    {
        if (!DateTime.TryParse(addProductModel.LastRestockedDate, out DateTime lastRestockedDate))
        {
            return BadRequest(new { Message = "Invalid last restocked date!" });
        }

        var newProduct = new Product
        {
            Name = addProductModel.Name,
            Brand = addProductModel.Brand,
            QuantityInStock = addProductModel.QuantityInStock,
            PricePerUnit = addProductModel.PricePerUnit,
            LastRestockedDate = lastRestockedDate
        };
        
        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();
        
        return Ok("Product added successfully!");
    }
    
    [HttpPost]
    [Route("UpdateProduct/{productId:int}")]
    public async Task<ActionResult<Product>> UpdateProduct([FromRoute] int productId, [FromBody] AddProductModel addProductModel)
    {
        if (!DateTime.TryParse(addProductModel.LastRestockedDate, out DateTime lastRestockedDate))
        {
            return BadRequest(new { Message = "Invalid last restocked date!" });
        }
        
        var newProduct = new Product
        {
            ProductId = productId,
            Name = addProductModel.Name,
            Brand = addProductModel.Brand,
            QuantityInStock = addProductModel.QuantityInStock,
            PricePerUnit = addProductModel.PricePerUnit,
            LastRestockedDate = lastRestockedDate
        };
        
        var oldProduct = await _context.Products.FirstOrDefaultAsync(c => c.ProductId == productId);

        if (oldProduct is null)
        {
            return NotFound(new { Message = "No product with provided id found!" });
        }
        
        _context.Products.Entry(oldProduct).CurrentValues.SetValues(newProduct);
        
        await _context.SaveChangesAsync();
        
        return Ok("Product updated successfully!");
    }
}
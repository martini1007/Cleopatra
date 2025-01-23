using Cleopatra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cleopatra.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/services
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _context.Services.ToListAsync();
            return Ok(services);
        }

        // GET: api/services/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
                return NotFound(new { Message = "Service not found." });

            return Ok(service);
        }

        // POST: api/services (Admin-only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateService([FromBody] Service model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Services.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceById), new { id = model.ServiceId }, model);
        }

        // PUT: api/services/{id} (Admin-only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] Service model)
        {
            if (id != model.ServiceId)
                return BadRequest(new { Message = "Service ID mismatch." });

            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound(new { Message = "Service not found." });

            service.Name = model.Name;
            service.Description = model.Description;
            service.Price = model.Price;

            _context.Entry(service).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/services/{id} (Admin-only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound(new { Message = "Service not found." });

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

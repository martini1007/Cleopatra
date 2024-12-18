using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cleopatra.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleopatra.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // Endpoint: Move Appointment
        [HttpPut("MoveAppointment/{id}")]
        public async Task<IActionResult> MoveAppointment(int id, [FromBody] MoveAppointmentRequest request)
        {
            if (request == null || request.NewDateTime == default)
            {
                return BadRequest("Invalid request data.");
            }

            var appointment = await _context.Appointments.Include(a => a.Employee).FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Check if the new time is within the employee's schedule
            var employeeSchedule = await _context.Schedules.FirstOrDefaultAsync(s => s.EmployeeId == appointment.EmployeeId);

            if (employeeSchedule == null ||
                request.NewDateTime < employeeSchedule.StartDateTime ||
                request.NewDateTime.AddMinutes(appointment.Duration ?? 0) > employeeSchedule.EndDateTime)
            {
                return BadRequest("The new time is outside the employee's schedule.");
            }

            // Check for conflicts with other appointments
            var hasConflicts = await _context.Appointments.AnyAsync(a =>
                a.EmployeeId == appointment.EmployeeId &&
                a.AppointmentId != id &&
                a.AppointmentDateTime < request.NewDateTime.AddMinutes(appointment.Duration ?? 0) &&
                a.AppointmentDateTime.AddMinutes(a.Duration ?? 0) > request.NewDateTime);

            if (hasConflicts)
            {
                return Conflict("The new time conflicts with another appointment.");
            }

            // Update appointment
            appointment.AppointmentDateTime = request.NewDateTime;
            _context.Appointments.Update(appointment);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating appointment: {ex.Message}");
            }

            return Ok("Appointment moved successfully.");
        }
    }

    public class MoveAppointmentRequest
    {
        public DateTime NewDateTime { get; set; }
    }
}
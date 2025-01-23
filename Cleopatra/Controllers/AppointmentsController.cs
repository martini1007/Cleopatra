using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cleopatra.Data;
using Cleopatra.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Cleopatra.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AppointmentsController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Endpoint: Move Appointment
        [HttpPut("MoveAppointment/{id}")]
        public async Task<IActionResult> MoveAppointment(int id, [FromBody] MoveAppointmentRequest request)
        {
            if (request == null || request.NewDateTime == default)
            {
                return BadRequest("Invalid request data.");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Employee)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Check if the new time is within the employee's schedule
            var employeeSchedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.EmployeeId == appointment.EmployeeId);

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

            // Save old date for notification purposes
            var oldDateTime = appointment.AppointmentDateTime;

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

            // Send notification email to the customer
            try
            {
                var emailMessage = $"Dear {appointment.Customer.Name},\n\n" +
                    $"Your appointment scheduled for {oldDateTime:dd MMM yyyy, HH:mm} has been successfully rescheduled to {request.NewDateTime:dd MMM yyyy, HH:mm}.\n\n" +
                    "Thank you for using our services.\n\nBest regards,\nCleopatra Team";

                await _emailService.SendEmailAsync(appointment.Customer.Email, "Appointment Rescheduled", emailMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Appointment moved successfully, but email notification failed: {ex.Message}");
            }

            return Ok("Appointment moved successfully.");
        }
    
        [HttpPost("CreateAppointment")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        if (request == null || request.AppointmentDateTime == default || request.EmployeeId == 0 || request.CustomerId == 0)
        {
            return BadRequest("Invalid request data.");
        }

        // Sprawd�, czy pracownik ma dost�pny termin
        var employeeSchedule = await _context.Schedules.FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId);

        if (employeeSchedule == null ||
            request.AppointmentDateTime < employeeSchedule.StartDateTime ||
            request.AppointmentDateTime.AddMinutes(request.Duration ?? 0) > employeeSchedule.EndDateTime)
        {
            return BadRequest("The appointment time is outside the employee's schedule.");
        }

        // Sprawd�, czy s� konflikty z innymi wizytami
        var hasConflicts = await _context.Appointments.AnyAsync(a =>
            a.EmployeeId == request.EmployeeId &&
            a.AppointmentDateTime < request.AppointmentDateTime.AddMinutes(request.Duration ?? 0) &&
            a.AppointmentDateTime.AddMinutes(a.Duration ?? 0) > request.AppointmentDateTime);

        if (hasConflicts)
        {
            return Conflict("The appointment time conflicts with another appointment.");
        }

        // Utw�rz now� wizyt�
        var appointment = new Appointment
        {
            CustomerId = request.CustomerId,
            EmployeeId = request.EmployeeId,
            AppointmentDateTime = request.AppointmentDateTime,
            Duration = request.Duration,
            Service = request.ServiceType
        };

        _context.Appointments.Add(appointment);

        try
        {
            await _context.SaveChangesAsync();

            // Wy�lij e-mail potwierdzaj�cy
            var emailService = HttpContext.RequestServices.GetService<IEmailService>();
            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                string subject = "Appointment Confirmation";
                string body = $"Dear {customer.Name},<br/><br/>" +
                              $"Your appointment has been confirmed for {request.AppointmentDateTime}.<br/>" +
                              $"Service: {request.ServiceType}<br/><br/>" +
                              "Thank you,<br/>Cleopatra Salon";

                await emailService.SendEmailAsync(customer.Email, subject, body);
            }

            return Ok("Appointment created successfully, and confirmation email sent.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating appointment: {ex.Message}");
        }
    }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .ToListAsync();
            
            return Ok(JsonConvert.SerializeObject(appointments));
        }
    }

    public class MoveAppointmentRequest
    {
        public DateTime NewDateTime { get; set; }
    }
    public class CreateAppointmentRequest
    {
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public int? Duration { get; set; } // Duration in minutes
        public Service ServiceType { get; set; }
    }
}

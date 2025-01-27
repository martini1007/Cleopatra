using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cleopatra.Data;
using SendEmail.Services;
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
        
        // ✅ Endpoint: Przeniesienie wizyty
        [HttpPut("MoveAppointment/{id}")]
        public async Task<IActionResult> MoveAppointment(int id, [FromBody] MoveAppointmentRequest request)
        {
            if (request == null || request.NewDateTime == default)
                return BadRequest("Invalid request data.");

            var appointment = await _context.Appointments
                .Include(a => a.Employee)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound("Appointment not found.");

            // ✅ Sprawdzenie dostępności pracownika
            var employeeSchedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.EmployeeId == appointment.EmployeeId);

            if (employeeSchedule == null ||
                request.NewDateTime < employeeSchedule.StartDateTime ||
                request.NewDateTime > employeeSchedule.EndDateTime)
            {
                return BadRequest("The new time is outside the employee's schedule.");
            }
            
            // ✅ Sprawdzenie konfliktów
            var hasConflicts = await _context.Appointments.AnyAsync(a =>
                a.EmployeeId == appointment.EmployeeId &&
                a.AppointmentId != id &&
                a.AppointmentDateTime < request.NewDateTime &&
                a.AppointmentDateTime > request.NewDateTime);

            if (hasConflicts)
                return Conflict("The new time conflicts with another appointment.");

            // Zapisanie starej daty
            var oldDateTime = appointment.AppointmentDateTime;

            // ✅ Aktualizacja wizyty
            appointment.AppointmentDateTime = request.NewDateTime;
            _context.Appointments.Update(appointment);

            try
            {
                await _context.SaveChangesAsync();

                // ✅ Wysłanie e-maila do klienta
                var emailMessage = $"<h2>Twoja wizyta została przeniesiona!</h2>" +
                    $"<p>Poprzednia data: <b>{oldDateTime:dd MMM yyyy, HH:mm}</b></p>" +
                    $"<p>Nowa data: <b>{request.NewDateTime:dd MMM yyyy, HH:mm}</b></p>" +
                    $"<p>Dziękujemy za skorzystanie z naszych usług!</p>";

                await _emailService.SendEmailAsync(appointment.Customer.Email, "Appointment Rescheduled", emailMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Appointment moved successfully, but email notification failed: {ex.Message}");
            }

            return Ok("Appointment moved successfully.");
        }

        [AllowAnonymous]
        // ✅ Endpoint: Tworzenie wizyty
        [HttpPost("CreateAppointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            if (request == null || request.AppointmentDateTime == default || request.EmployeeId == 0 || request.CustomerId == 0)
                return BadRequest("Invalid request data.");

            // ✅ Sprawdzenie dostępności pracownika
            var employeeSchedule = await _context.Schedules.FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId);
            if (employeeSchedule == null ||
                request.AppointmentDateTime < employeeSchedule.StartDateTime ||
                request.AppointmentDateTime > employeeSchedule.EndDateTime)
            {
                return BadRequest("The appointment time is outside the employee's schedule.");
            }

            // ✅ Sprawdzenie konfliktów
            var hasConflicts = await _context.Appointments.AnyAsync(a =>
                a.EmployeeId == request.EmployeeId &&
                a.AppointmentDateTime < request.AppointmentDateTime &&
                a.AppointmentDateTime> request.AppointmentDateTime);
            
            if (hasConflicts)
                return Conflict("The appointment time conflicts with another appointment.");

            // ✅ Tworzenie nowej wizyty
            var appointment = new Appointment
            {
                CustomerId = request.CustomerId,
                EmployeeId = request.EmployeeId,
                AppointmentDateTime = request.AppointmentDateTime,
                ServiceId = request.ServiceId,
                Status = "Confirmed",
                Notes = string.Empty
            };

            _context.Appointments.Add(appointment);

            try
            {
                await _context.SaveChangesAsync();

                // ✅ Wysłanie e-maila z potwierdzeniem
                var customer = await _context.Customers.FindAsync(request.CustomerId);
                if (customer != null && !string.IsNullOrEmpty(customer.Email))
                {
                    string subject = "Potwierdzenie wizyty";
                    string body = $"<h2>Twoja wizyta została umówiona!</h2>" +
                                  $"<p>Data: <b>{request.AppointmentDateTime:dd MMM yyyy, HH:mm}</b></p>" +
                                  $"<p>Usługa: <b>{_context.Services.FirstOrDefault(s => s.ServiceId == request.ServiceId)?.Name}</b></p>" +
                                  $"<p>Dziękujemy za skorzystanie z naszych usług!</p>";

                    await _emailService.SendEmailAsync(customer.Email, subject, body);
                }

                return Ok("Appointment created successfully, and confirmation email sent.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating appointment: {ex.Message}");
            }
        }

        // ✅ Endpoint: Pobranie wszystkich wizyt
        [Authorize]
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
        
        // ✅ Endpoint: Pobranie wszystkich wizyt
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .FirstAsync(a => a.AppointmentId == id);

            return Ok(JsonConvert.SerializeObject(appointment));
        }

        [Authorize]
        [HttpGet("GetAppointments/{employeeId}")]
        public async Task<IActionResult> GetAllAppointmentsByEmployeeId(int employeeId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.EmployeeId == employeeId)
                .Include(a => a.Customer)
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .ToListAsync();
            
            return Ok(JsonConvert.SerializeObject(appointments));
        }
        
        [Authorize]
        [HttpGet("history/{customerId}")]
        public async Task<IActionResult> GetHistoryOfAppointments(int customerId)
        {
            var pastAppointments = await _context.Appointments
                .Where(a => a.CustomerId == customerId && a.AppointmentDateTime < DateTime.Now)
                .Include(a => a.Customer)
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .ToListAsync();
            
            var futureAppointments = await _context.Appointments
                .Where(a => a.CustomerId == customerId && a.AppointmentDateTime > DateTime.Now)
                .Include(a => a.Customer)
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .ToListAsync();
            
            return Ok(JsonConvert.SerializeObject(
                new
                {
                    PastAppointments = pastAppointments,
                    FutureAppointments = futureAppointments
                }));
        }
    }

    // ✅ Modele DTO
    public class MoveAppointmentRequest
    {
        public DateTime NewDateTime { get; set; }
    }

    public class CreateAppointmentRequest
    {
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public int ServiceId { get; set; }
    }
}
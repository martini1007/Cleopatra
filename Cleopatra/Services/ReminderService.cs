using Cleopatra.Data;
using Cleopatra.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleopatra.Services
{
    public class ReminderService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ReminderService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendRemindersAsync()
        {
            var tomorrow = DateTime.Now.Date.AddDays(1);
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Where(a => a.AppointmentDateTime.Date == tomorrow)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                if (appointment.Customer != null && !string.IsNullOrEmpty(appointment.Customer.Email))
                {
                    string subject = "Appointment Reminder";
                    string body = $"Dear {appointment.Customer.Name},<br/><br/>" +
                                  $"This is a reminder for your appointment scheduled on {appointment.AppointmentDateTime}.<br/>" +
                                  $"Service: {appointment.ServiceType}<br/><br/>" +
                                  "Thank you,<br/>Cleopatra Salon";

                    await _emailService.SendEmailAsync(appointment.Customer.Email, subject, body);
                }
            }
        }
    }
}

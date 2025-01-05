using System.Threading.Tasks;

namespace Cleopatra.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}

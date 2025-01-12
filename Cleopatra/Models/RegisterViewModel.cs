using System.ComponentModel.DataAnnotations;

namespace Cleopatra.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Name { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace ProductManagementApp.Models
{
    public class RegisterRequest
    {
        [Required]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@gmail\.com$", ErrorMessage = "Email harus @gmail.com")]
        public required string Email { get; set; }

        [Required, MinLength(6)]
        public required string Password { get; set; }

        public IFormFile? ProfilePictureFile { get; set; }
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}

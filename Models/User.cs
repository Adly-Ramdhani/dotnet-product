using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations; 
using Microsoft.AspNetCore.Http;

namespace ProductManagementApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }

        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        [RegularExpression(@"^[^@\s]+@gmail\.com$", ErrorMessage = "Email harus menggunakan @gmail.com")]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } 

        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; } 

    }
}

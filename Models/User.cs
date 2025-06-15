using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ProductManagementApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter.")]
        public string FullName { get; set; } = string.Empty;

        // Path atau nama file foto profil yang disimpan
        public string? ProfilePicture { get; set; }

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [RegularExpression(@"^[^@\s]+@gmail\.com$", ErrorMessage = "Email harus menggunakan domain @gmail.com")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        [MinLength(6, ErrorMessage = "Password minimal 6 karakter.")]
        public string Password { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; } = false;

        [StringLength(6)]
        public string? EmailOtpCode { get; set; }

        public DateTime? OtpGeneratedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}

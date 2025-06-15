using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProductManagementApp.Data;
using ProductManagementApp.Models;
using ProductManagementApp.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ProductManagementApp.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _fromEmail;
        private readonly string _emailPassword;

        public AuthController(ApplicationDbContext context, IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _config = config;
            _webHostEnvironment = webHostEnvironment;

            _fromEmail = _config["EmailSettings:FromEmail"]!;
            _emailPassword = _config["EmailSettings:AppPassword"]!;
        }

        [AllowAnonymous]
        [HttpGet("")]
        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpGet("register")]
        public IActionResult Register() => View();

       [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
              if (string.IsNullOrWhiteSpace(request.FullName))
            {
                ViewBag.Error = "Nama lengkap wajib diisi.";
                return View(request);
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                ViewBag.Error = "Email wajib diisi.";
                return View(request);
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                ViewBag.Error = "Password minimal 6 karakter.";
                return View(request);
            }

            // Validasi email unik
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                ViewBag.Error = "Email sudah terdaftar.";
                return View(request);
            }

            var otpCode = GenerateOtp();
            string? fileName = null;

            if (request.ProfilePictureFile != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                fileName = Guid.NewGuid() + Path.GetExtension(request.ProfilePictureFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                request.ProfilePictureFile.CopyTo(stream);
            }

            var tempUser = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                ProfilePicture = fileName,
                EmailOtpCode = otpCode,
                OtpGeneratedAt = DateTime.UtcNow,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            HttpContext.Session.SetString("TempUser", JsonSerializer.Serialize(tempUser));

            SendOtpEmail(tempUser.Email, otpCode);

            return RedirectToAction("VerifyOtp", new { email = tempUser.Email });
        }


        [HttpGet("verify-otp")]
        public IActionResult VerifyOtp([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            ViewBag.Email = email;

            var tempUserJson = HttpContext.Session.GetString("TempUser");
            if (tempUserJson != null)
            {
                var tempUser = JsonSerializer.Deserialize<User>(tempUserJson);
                if (tempUser?.OtpGeneratedAt != null)
                {
                    ViewBag.ExpiredAt = tempUser.OtpGeneratedAt.Value
                        .AddMinutes(5)
                        .ToLocalTime()
                        .ToString("HH:mm");
                }
            }

            return View();
        }


        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp(string email, string otp)
        {
            var tempUserJson = HttpContext.Session.GetString("TempUser");
            if (tempUserJson == null)
            {
                ViewBag.Error = "Data registrasi tidak ditemukan.";
                ViewBag.Email = email;
                return View();
            }

            var tempUser = JsonSerializer.Deserialize<User>(tempUserJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (tempUser == null || tempUser.Email != email)
            {
                ViewBag.Error = "Email tidak sesuai.";
                ViewBag.Email = email;
                return View();
            }

            if (tempUser.EmailOtpCode != otp)
            {
                ViewBag.Error = "Kode OTP salah.";
                ViewBag.Email = email;
                return View();
            }

            if (tempUser.OtpGeneratedAt == null || DateTime.UtcNow > tempUser.OtpGeneratedAt.Value.AddMinutes(5))
            {
                ViewBag.Error = "Kode OTP telah kedaluwarsa.";
                ViewBag.Email = email;
                return View();
            }

            tempUser.IsEmailConfirmed = true;
            tempUser.CreatedAt = DateTime.UtcNow;
            tempUser.UpdatedAt = DateTime.UtcNow;

            _context.Users.Add(tempUser);
            _context.SaveChanges();

            HttpContext.Session.Remove("TempUser");

            return RedirectToAction("Login");
        }



        [HttpPost("resend-otp")]
        public IActionResult ResendOtp(string email)
        {
            var tempUserJson = HttpContext.Session.GetString("TempUser");
            if (tempUserJson == null)
            {
                TempData["Error"] = "Data registrasi tidak ditemukan.";
                return RedirectToAction("VerifyOtp", new { email });
            }

            var tempUser = JsonSerializer.Deserialize<User>(tempUserJson);
            if (tempUser == null || tempUser.Email != email)
            {
                TempData["Error"] = "Email tidak sesuai.";
                return RedirectToAction("VerifyOtp", new { email });
            }

            var newOtp = GenerateOtp();
            tempUser.EmailOtpCode = newOtp;
            tempUser.OtpGeneratedAt = DateTime.UtcNow;

            // Update session
            HttpContext.Session.SetString("TempUser", JsonSerializer.Serialize(tempUser));

            SendOtpEmail(tempUser.Email, newOtp);

            TempData["Message"] = "Kode OTP baru telah dikirim ke email Anda.";
            return RedirectToAction("VerifyOtp", new { email });
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.Password))
                return Unauthorized("Email atau password salah.");

            if (!user.IsEmailConfirmed)
                return Unauthorized("Email belum diverifikasi.");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpPost("loginform")]
        public IActionResult LoginForm(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !PasswordHasher.VerifyPassword(password, user.Password))
            {
                ViewBag.Error = "Email atau password salah.";
                return View("Login");
            }

            if (!user.IsEmailConfirmed)
            {
                ViewBag.Error = "Email belum diverifikasi. Silakan verifikasi terlebih dahulu.";
                ViewBag.Email = user.Email;
                return View("VerifyOtp");
            }

            var token = GenerateJwtToken(user);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(6)
            });

            TempData["LoginSuccess"] = "Selamat datang kembali! Anda berhasil login.";
            return RedirectToAction("Index", "Home");
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }

        private string GenerateOtp()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomInt = BitConverter.ToUInt32(bytes, 0) % 900000 + 100000;
            return randomInt.ToString();
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName)
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private void SendOtpEmail(string toEmail, string otpCode)
        {
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(_fromEmail, _emailPassword)
            };

            var subject = "Verifikasi Email OTP";
            var body = $"<h2>Kode OTP kamu:</h2><h1>{otpCode}</h1>";

            using var message = new MailMessage(_fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            smtp.Send(message);
        }
    }
}

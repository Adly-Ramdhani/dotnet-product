using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProductManagementApp.Data;
using ProductManagementApp.Models;
using ProductManagementApp.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace ProductManagementApp.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: /auth or /auth/login
        
        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        // GET: /auth/register
        [AllowAnonymous]
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /auth/register (API)
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Data tidak valid.";
                return View();
            }

            if (_context.Users.Any(u => u.Email == request.Email))
            {
                ViewBag.Error = "Email sudah terdaftar.";
                return View();
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = PasswordHasher.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // POST: /auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.Password))
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        // POST: /auth/loginform (HTML form)
        [HttpPost("loginform")]
        public IActionResult LoginForm(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !PasswordHasher.VerifyPassword(password, user.Password))
            {
                ViewBag.Error = "Email atau password salah.";
                return View("Login");
            }

            var token = GenerateJwtToken(user);

            // Simpan JWT ke cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true, // tidak bisa diakses JavaScript
                Secure = true,   // hanya via HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(6)
            });

            return RedirectToAction("Index", "Home");
        }


        // POST: /auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
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
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

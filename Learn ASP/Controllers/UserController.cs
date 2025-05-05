using Learn_ASP.Data;
using Learn_ASP.Helpers;
using Learn_ASP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Learn_ASP.Controllers
{
    [ApiController]
    [Route("gsa-api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UsersController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            // Validate
            if (string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Email)
                || string.IsNullOrEmpty(req.FullName) || string.IsNullOrEmpty(req.Password))
            {
                return UnprocessableEntity(new { message = "Validation error: all fields are required." });
            }

            if (!req.Email.Contains("@"))
                return UnprocessableEntity(new { message = "Validation error: email is invalid." });

            if (_context.Users.Any(u => u.Email == req.Email))
                return UnprocessableEntity(new { message = "Validation error: email already exists." });

            if (!ValidatePassword(req.Password))
                return UnprocessableEntity(new { message = "Validation error: password must be at least 8 characters with uppercase, lowercase, number, and symbol." });

            string hashedPassword = PasswordHasher.Hash(req.Password);

            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                Name = req.FullName,
                PasswordHash = hashedPassword,
                Role = "student"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
                return UnprocessableEntity(new { message = "Validation error: email and password are required." });

            if (!req.Email.Contains("@"))
                return UnprocessableEntity(new { message = "Validation error: email is invalid." });

            string hashedInput = PasswordHasher.Hash(req.Password);

            var user = _context.Users.FirstOrDefault(u => u.Email == req.Email && u.PasswordHash == hashedInput);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            // Generate Token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful.",
                data = new
                {
                    userId = user.Id,
                    username = user.Username,
                    role = user.Role,
                    token
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful." });
        }

        private bool ValidatePassword(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    // Request DTOs
    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

}

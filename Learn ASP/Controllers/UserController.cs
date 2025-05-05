using Learn_ASP.Data;
using Learn_ASP.DTOs;
using Learn_ASP.Helpers;
using Learn_ASP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return UnprocessableEntity(new { message = "Validation error: fields are required." });
            }


            if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return UnprocessableEntity(new { message = "Validation error: email is invalid." });
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return UnprocessableEntity(new { message = "Validation error: email already exists." });
            }

            if (!Regex.IsMatch(dto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
            {
                return UnprocessableEntity(new { message = "Validation error: password format invalid." });
            }

            var user = new User
            {
                Username = dto.Username,
                Name = dto.Name,
                Email = dto.Email,
                Password_Hash = PasswordHasher.Hash(dto.Password),
                Role = "student"
            };

            _context.Users.Add(user);
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = ex.InnerException?.Message });
            }


            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return UnprocessableEntity(new { message = "Validation error: email and password are required." });
            }

            if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return UnprocessableEntity(new { message = "Validation error: email is invalid." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.Password_Hash != PasswordHasher.Hash(dto.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var token = JwtHelper.GenerateToken(user.Id, user.Username, user.Role, _config["JwtKey"]);

            return Ok(new
            {
                message = "Login successful.",
                data = new
                {
                    userId = user.Id,
                    username = user.Username,
                    role = user.Role,
                    token = token
                }
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Invalidate token (JWT is stateless, so client must discard it)
            return Ok(new { message = "Logout successful." });
        }
    }
}

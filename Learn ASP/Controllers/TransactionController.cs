using Learn_ASP.Data;
using Learn_ASP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Learn_ASP.Controllers
{
    [ApiController]
    [Route("gsa-api/v1")]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] string? courseName,
            [FromQuery] string? sortBy = "asc",
            [FromQuery] string? userEmail,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            if (page <= 0 || size <= 0)
                return UnprocessableEntity(new { message = "Validation error: page and size must be positive integers." });

            if (sortBy != "asc" && sortBy != "desc")
                return UnprocessableEntity(new { message = "Validation error: sortBy must be 'asc' or 'desc'." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Unauthorized(new { message = "Authorization token missing or invalid." });

            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = userRoles.Contains("Admin");

            var query = _context.Transactions
                .Include(t => t.Course)
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(courseName))
                query = query.Where(t => t.Course.Title.Contains(courseName));

            // Student hanya bisa lihat transaksi dirinya sendiri
            if (!isAdmin)
            {
                query = query.Where(t => t.UserId == userId);

                query = sortBy == "asc"
                    ? query.OrderBy(t => t.PurchaseDate)
                    : query.OrderByDescending(t => t.PurchaseDate);
            }
            else
            {
                // Admin bisa filter by userEmail
                if (!string.IsNullOrEmpty(userEmail))
                    query = query.Where(t => t.User.Email.Contains(userEmail));

                // Admin selalu sort by PurchaseDate desc
                query = query.OrderByDescending(t => t.PurchaseDate);
            }

            var totalTransactions = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalTransactions / (double)size);

            var transactions = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var responseData = transactions.Select(t => new
            {
                transactionId = t.Id,
                userEmail = isAdmin ? t.User.Email : null,
                courseId = t.CourseId,
                courseTitle = t.Course.Title,
                purchaseDate = t.PurchaseDate,
                amount = t.Amount,
                couponCode = t.CouponCode ?? "",
                paidAmount = t.PaidAmount
            });

            return Ok(new
            {
                data = responseData,
                pagination = new
                {
                    page,
                    size,
                    totalPages
                }
            });
        }
    }

}

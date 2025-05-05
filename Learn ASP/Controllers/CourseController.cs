using Learn_ASP.Data;
using Learn_ASP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Learn_ASP.Controllers
{
    [ApiController]
    [Route("gsa-api/v1/courses/")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] string? title, [FromQuery] string? sort = "desc", [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            if (page <= 0 || size <= 0)
                return UnprocessableEntity(new { message = "Validation error: 'page' must be a positive integer." });

            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(title))
                query = query.Where(c => c.Title.Contains(title));

            query = sort == "asc" ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)size);

            var courses = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Price
                })
                .ToListAsync();

            return Ok(new
            {
                data = courses,
                pagination = new
                {
                    page,
                    size,
                    totalPages
                }
            });
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourseDetails(int courseId)
        {
            if (courseId <= 0)
                return UnprocessableEntity(new { message = "Validation error: courseId must be numeric." });

            var course = await _context.Courses
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound(new { message = "Course not found." });

            return Ok(new
            {
                data = new
                {
                    course.Id,
                    course.Title,
                    course.Description,
                    Price = course.Price,
                    Duration = $"{course.Duration} minutes",
                    Modules = course.Modules.Select(m => m.Title).ToList()
                }
            });
        }

        [HttpPost("{courseId}/purchase")]
        [Authorize]
        public async Task<IActionResult> PurchaseCourse(int courseId, [FromBody] PurchaseRequest request)
        {
            if (courseId <= 0)
                return UnprocessableEntity(new { message = "Validation error: courseId must be numeric." });

            var validMethods = new[] { "debit_card", "credit_card", "paypal" };
            if (!validMethods.Contains(request.PaymentMethod.ToLower()))
            {
                return UnprocessableEntity(new { message = "Validation error: payment method not accepted." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Forbid(new { message = "Please login/register to purchase the course" });

            int userId = int.Parse(userIdClaim.Value);

            // Ambil Course
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                return NotFound(new { message = "Course not found." });

            decimal discount = 0;
            int? couponId = null;

            // Cek Coupon (optional)
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode);

                if (coupon == null || coupon.ExpiryDate < DateTime.UtcNow || coupon.Quota <= 0)
                    return UnprocessableEntity(new { message = "Validation error: coupon code has expired or quota exceeded." });

                discount = (coupon.DiscountPct / 100m) * course.Price;
                coupon.Quota -= 1;  // Kurangi kuota coupon
                couponId = coupon.Id;
            }

            // Hitung harga akhir
            decimal paidAmount = course.Price - discount;

            // Simpan ke tabel Purchase
            var purchase = new Purchase
            {
                User_Id = userId,
                Course_Id = course.Id,
                Coupon_Id = couponId,
                Price_Paid = paidAmount,
                Payment_Method = request.PaymentMethod.ToLower(),
                Purchased_At = DateTime.UtcNow
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Course purchased successfully.",
                data = new
                {
                    purchaseId = purchase.Id,
                    courseId = course.Id,
                    userId = userId,
                    purchaseDate = purchase.Purchased_At.ToString("o"),
                    paymentMethod = request.PaymentMethod.ToLower(),
                    originalPrice = course.Price,
                    discountApplied = discount,
                    paidAmount
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CourseRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            var course = new Course
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Duration = request.Duration,
                Modules = request.Modules.Select(m => new Module
                {
                    Title = m.Title,
                    Content = m.Content
                }).ToList()
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Course created successfully", data = course });
        }

        [HttpPut]
        [Route("{courseId}")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] CourseRequest request)
        {
            var course = await _context.Courses
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound(new { message = "Course not found." });

            course.Title = request.Title;
            course.Description = request.Description;
            course.Price = request.Price;
            course.Duration = request.Duration;

            course.Modules.Clear();
            foreach (var m in request.Modules)
            {
                course.Modules.Add(new Module
                {
                    Title = m.Title,
                    Content = m.Content
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Course updated successfully", data = course });
        }

        private IActionResult Forbid(object value)
        {
            throw new NotImplementedException();
        }
    }
}

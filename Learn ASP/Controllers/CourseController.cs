using Learn_ASP.Data;
using Learn_ASP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learn_ASP.Controllers
{
    [ApiController]
    [Route("gsa-api/v1")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses([FromQuery] string title, [FromQuery] string sort = "desc", [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            if (page <= 0 || size <= 0)
                return UnprocessableEntity(new { message = "Validation error: 'page' must be a positive integer." });

            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(title))
                query = query.Where(c => c.Title.Contains(title));

            query = sort == "asc" ? query.OrderBy(c => c.Created_At) : query.OrderByDescending(c => c.Created_At);

            var totalCourses = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCourses / (double)size);

            var courses = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            return Ok(new
            {
                data = courses.Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Price
                }),
                pagination = new
                {
                    page,
                    size,
                    totalPages
                }
            });
        }

        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetCourseDetail(int courseId)
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
                    course.Price,
                    course.Duration,
                    modules = course.Modules.Select(m => m.Title)
                }
            });
        }
    }
}

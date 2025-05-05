using Learn_ASP.Data;
using Learn_ASP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learn_ASP.Controllers
{
    [ApiController]
    [Route("gsa-api/v1/coupons")]
    public class CouponController : ControllerBase
    {

        private readonly AppDbContext _context;

        public CouponController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoupons()
        {
            var coupons = await _context.Coupons.ToListAsync();

            return Ok(new
            {
                data = coupons.Select(c => new
                {
                    c.Id,
                    c.Code,
                    c.Discount_Pct,
                    c.Quota,
                    c.Expiry_Date
                })
            });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> AddCoupon([FromBody] CreateCouponRequest request)
        {
            if (string.IsNullOrEmpty(request.CouponCode) || request.CouponCode.Length < 5)
                return UnprocessableEntity(new { message = "Validation error: coupon code must be at least 5 characters." });

            if (request.DiscountValue <= 0)
                return UnprocessableEntity(new { message = "Validation error: discount value must be positive." });

            if (request.Quota <= 0)
                return UnprocessableEntity(new { message = "Validation error: quota must be positive." });

            if (request.ExpiryDate <= DateTime.UtcNow)
                return UnprocessableEntity(new { message = "Validation error: expiry date must be in the future." });

            var coupon = new Coupon
            {
                Code = request.CouponCode,
                Discount_Pct = request.DiscountValue,
                Quota = request.Quota,
                Expiry_Date = request.ExpiryDate
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Coupon added successfully.",
                data = new
                {
                    coupon.Id,
                    coupon.Code,
                    coupon.Discount_Pct,
                    coupon.Quota,
                    coupon.Expiry_Date
                }
            });
        }

        [HttpPut("coupons/{couponId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateCoupon(int couponId, [FromBody] Coupon request)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null)
                return NotFound(new { message = "Coupon not found." });

            if (request.Discount_Pct <= 0 || request.Quota <= 0)
                return UnprocessableEntity(new { message = "Validation error: quota and discount must be positive." });

            if (request.Expiry_Date <= DateTime.UtcNow)
                return UnprocessableEntity(new { message = "Validation error: expiry date must be in the future." });

            var exists = await _context.Coupons.AnyAsync(c => c.Code == request.Code && c.Id != couponId);
            if (exists)
                return UnprocessableEntity(new { message = "Validation error: coupon code must be unique." });

            coupon.Code = request.Code;
            coupon.Discount_Pct = request.Discount_Pct;
            coupon.Expiry_Date = request.Expiry_Date;
            coupon.Quota = request.Quota;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Coupon updated successfully.",
                data = new
                {
                    couponId = coupon.Id,
                    couponCode = coupon.Code,
                    discountValue = coupon.Discount_Pct,
                    expiryDate = coupon.Expiry_Date,
                    quota = coupon.Quota
                }
            });
        }
    }
}

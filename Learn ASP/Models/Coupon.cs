namespace Learn_ASP.Models
{
    public class Coupon
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; } = null!;
        public decimal DiscountValue { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quota { get; set; }
    }
}

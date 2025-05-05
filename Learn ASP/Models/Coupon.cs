using System.ComponentModel.DataAnnotations;

namespace Learn_ASP.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        [Required]
        public string Code { get; set; } = null!;
        [Required]
        public decimal DiscountPct { get; set; }
        [Required]
        public int Quota { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        public List<Purchase> Purchases { get; set; } = new();
    }
}

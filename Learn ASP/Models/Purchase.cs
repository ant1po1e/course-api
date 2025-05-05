using System.ComponentModel.DataAnnotations;

namespace Learn_ASP.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        [Required]
        public int User_Id { get; set; }
        [Required]
        public int Course_Id { get; set; }
        public int? Coupon_Id { get; set; }
        [Required]
        public decimal Price_Paid { get; set; }
        [Required]
        public string Payment_Method { get; set; } = null!;
        public DateTime Purchased_At { get; set; }

        [Required]
        public User User { get; set; } = null!;
        [Required]
        public Course Course { get; set; } = null!;
        public Coupon? Coupon { get; set; }
    }
}

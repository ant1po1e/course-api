namespace Learn_ASP.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountApplied { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; } = null!;
    }
}

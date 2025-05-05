namespace Learn_ASP
{
    public class DTO
    {
        public class PurchaseRequest
        {
            public string PaymentMethod { get; set; } = null!;
            public string? CouponCode { get; set; }
        }
    }
}

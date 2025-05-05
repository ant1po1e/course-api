namespace Learn_ASP.Models
{
    public class PurchaseRequest
    {
        public string PaymentMethod { get; set; }
        public string CouponCode { get; set; }
    }

    public class ModuleRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class CourseRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public List<ModuleRequest> Modules { get; set; }
    }

    public class CreateCourseRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; } // in minutes
        public List<ModuleRequest> Modules { get; set; }
    }

    public class UpdateCourseRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public List<ModuleRequest> Modules { get; set; } 
        public bool Active { get; set; }
    }

    public class CreateCouponRequest
    {
        public string CouponCode { get; set; }
        public decimal DiscountValue { get; set; }
        public int Quota { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

}

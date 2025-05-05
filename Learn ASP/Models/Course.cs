using Learn_ASP.Models;
using System.ComponentModel.DataAnnotations;

namespace Learn_ASP.Models
{
    public class Course
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Duration { get; set; }  // minutes
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Required]
        public List<Module> Modules { get; set; } = new();
        [Required]
        public List<Purchase> Purchases { get; set; } = new();
    }

}

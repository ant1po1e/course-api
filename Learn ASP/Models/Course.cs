using Learn_ASP.Models;

namespace Learn_ASP.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Duration { get; set; } 
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        public ICollection<Module> Modules { get; set; } = new List<Module>();
    }

}

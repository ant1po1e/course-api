using System.ComponentModel.DataAnnotations;

namespace Learn_ASP.Models
{
    public class Module
    {
        public int Id { get; set; }
        [Required]
        public int Course_Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Content { get; set; } = null!;

        public Course Course { get; set; } = null!;
    }
}

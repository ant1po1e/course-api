using System.ComponentModel.DataAnnotations;

namespace Learn_ASP.Models
{
    public class User
    {
        public int Id { get; set; }


        [Required]
        public string Name { get; set; } = null!;
        
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password_Hash { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "student";
    }
}

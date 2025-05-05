namespace Learn_ASP.Models
{
    public class Module
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;

        public int Course_Id { get; set; }
        public Course Course { get; set; } = null!;
    }

}

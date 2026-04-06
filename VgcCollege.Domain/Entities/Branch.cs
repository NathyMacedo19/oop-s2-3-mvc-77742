namespace VgcCollege.Domain.Entities
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Navigation
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
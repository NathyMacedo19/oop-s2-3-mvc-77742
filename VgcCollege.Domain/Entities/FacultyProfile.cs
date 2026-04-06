namespace VgcCollege.Domain.Entities
{
    public class FacultyProfile
    {
        public int Id { get; set; }
        public string IdentityUserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Navigation
        public ICollection<FacultyCourseAssignment> CourseAssignments { get; set; } = new List<FacultyCourseAssignment>();
    }
}
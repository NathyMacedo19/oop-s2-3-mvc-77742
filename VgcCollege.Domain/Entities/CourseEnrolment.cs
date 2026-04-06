namespace VgcCollege.Domain.Entities
{
    public class CourseEnrolment
    {
        public int Id { get; set; }
        public int StudentProfileId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Withdrawn, Completed

        // Navigation
        public StudentProfile StudentProfile { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
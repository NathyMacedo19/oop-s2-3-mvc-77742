using VgcCollege.Domain.Entities;

namespace VgcCollege.Web.ViewModels
{
    public class StudentWithAttendanceViewModel
    {
        public StudentProfile? Student { get; set; }
        public int AttendancePercent { get; set; }
        public int EnrolmentId { get; set; }
    }
}
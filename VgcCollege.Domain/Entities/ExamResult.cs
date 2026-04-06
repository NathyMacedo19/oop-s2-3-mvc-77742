namespace VgcCollege.Domain.Entities
{
    public class ExamResult
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int StudentProfileId { get; set; }
        public int Score { get; set; }
        public string Grade { get; set; } = string.Empty;

        // Navigation
        public Exam Exam { get; set; } = null!;
        public StudentProfile StudentProfile { get; set; } = null!;
    }
}
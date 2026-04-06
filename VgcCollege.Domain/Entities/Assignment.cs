using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Entities
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Assignment title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Assignment Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maximum score is required")]
        [Range(1, 100, ErrorMessage = "Maximum score must be between 1 and 100")]
        [Display(Name = "Maximum Score")]
        public int MaxScore { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        // Navigation
        public Course Course { get; set; } = null!;
        public ICollection<AssignmentResult> Results { get; set; } = new List<AssignmentResult>();
    }
}
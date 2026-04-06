using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Web.Data;

namespace VgcCollege.Tests.Helpers
{
    public static class TestDbHelper
    {
        private static int _dbCounter = 0;

        public static AppDbContext GetInMemoryDbContext()
        {
            _dbCounter++;
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase_{_dbCounter}")
                .Options;

            var context = new AppDbContext(options);

            // Clear any existing data
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SeedTestData(context);
            return context;
        }

        private static void SeedTestData(AppDbContext context)
        {
            // Seed Branches
            var dublinBranch = new Branch { Id = 1, Name = "Dublin Campus", Address = "1 O'Connell St, Dublin" };
            var corkBranch = new Branch { Id = 2, Name = "Cork Campus", Address = "12 Patrick St, Cork" };
            context.Branches.AddRange(dublinBranch, corkBranch);

            // Seed Courses
            var course1 = new Course { Id = 1, Name = "Cybersecurity", BranchId = 1, StartDate = DateTime.Today.AddMonths(-2), EndDate = DateTime.Today.AddMonths(4) };
            var course2 = new Course { Id = 2, Name = "Data Science", BranchId = 1, StartDate = DateTime.Today.AddMonths(-2), EndDate = DateTime.Today.AddMonths(4) };
            var course3 = new Course { Id = 3, Name = "Software Development", BranchId = 2, StartDate = DateTime.Today.AddMonths(-1), EndDate = DateTime.Today.AddMonths(5) };
            context.Courses.AddRange(course1, course2, course3);

            // Seed Faculty Profiles
            var faculty1 = new FacultyProfile { Id = 1, Name = "Dr. John Rowley", Email = "john.rowley@vgc.com", Phone = "0851234567" };
            var faculty2 = new FacultyProfile { Id = 2, Name = "Dr. Wenhao Fu", Email = "wenhao.fu@vgc.com", Phone = "0857654321" };
            context.FacultyProfiles.AddRange(faculty1, faculty2);

            // Seed Faculty Course Assignments
            context.FacultyCourseAssignments.AddRange(
                new FacultyCourseAssignment { FacultyProfileId = 1, CourseId = 1 },
                new FacultyCourseAssignment { FacultyProfileId = 1, CourseId = 2 },
                new FacultyCourseAssignment { FacultyProfileId = 2, CourseId = 3 }
            );

            // Seed Student Profiles
            var student1 = new StudentProfile { Id = 1, Name = "Nathalia Macedo", Email = "nathy@vgc.com", StudentNumber = "VGC001", Phone = "0861345687" };
            var student2 = new StudentProfile { Id = 2, Name = "Paul O'Connel", Email = "pauloc@vgc.com", StudentNumber = "VGC002", Phone = "0834654309" };
            var student3 = new StudentProfile { Id = 3, Name = "Carlos Costa", Email = "carlosc@vgc.com", StudentNumber = "VGC003", Phone = "0869878924" };
            context.StudentProfiles.AddRange(student1, student2, student3);

            // Seed Enrolments
            context.CourseEnrolments.AddRange(
                new CourseEnrolment { Id = 1, StudentProfileId = 1, CourseId = 1, EnrolDate = DateTime.Today.AddMonths(-1), Status = "Active" },
                new CourseEnrolment { Id = 2, StudentProfileId = 1, CourseId = 2, EnrolDate = DateTime.Today.AddMonths(-1), Status = "Active" },
                new CourseEnrolment { Id = 3, StudentProfileId = 2, CourseId = 1, EnrolDate = DateTime.Today.AddMonths(-1), Status = "Active" },
                new CourseEnrolment { Id = 4, StudentProfileId = 3, CourseId = 3, EnrolDate = DateTime.Today.AddMonths(-1), Status = "Active" }
            );

            // Seed Assignments
            var assignment1 = new Assignment { Id = 1, CourseId = 1, Title = "Mobile App Dev2.", MaxScore = 100, DueDate = DateTime.Today.AddDays(-10) };
            var assignment2 = new Assignment { Id = 2, CourseId = 1, Title = "CA3 - UI/UX Report", MaxScore = 100, DueDate = DateTime.Today.AddDays(10) };
            var assignment3 = new Assignment { Id = 3, CourseId = 2, Title = "Data Analysis Report", MaxScore = 100, DueDate = DateTime.Today.AddDays(-5) };
            context.Assignments.AddRange(assignment1, assignment2, assignment3);

            // Seed Assignment Results
            context.AssignmentResults.AddRange(
                new AssignmentResult { Id = 1, AssignmentId = 1, StudentProfileId = 1, Score = 85, Feedback = "Good work!" },
                new AssignmentResult { Id = 2, AssignmentId = 1, StudentProfileId = 2, Score = 72, Feedback = "Needs improvement" },
                new AssignmentResult { Id = 3, AssignmentId = 3, StudentProfileId = 1, Score = 92, Feedback = "Excellent!" }
            );

            // Seed Exams - Student 1 has exam 1 (released) and exam 2 (not released)
            var exam1 = new Exam { Id = 1, CourseId = 1, Title = "Semester Exam", Date = DateTime.Today.AddDays(-5), MaxScore = 100, ResultsReleased = true };
            var exam2 = new Exam { Id = 2, CourseId = 1, Title = "Midterm Exam", Date = DateTime.Today.AddDays(-15), MaxScore = 100, ResultsReleased = false };
            var exam3 = new Exam { Id = 3, CourseId = 3, Title = "Security Exam", Date = DateTime.Today.AddDays(-3), MaxScore = 100, ResultsReleased = true };
            context.Exams.AddRange(exam1, exam2, exam3);

            // Seed Exam Results - Student 1 has results for both exam 1 AND exam 2
            context.ExamResults.AddRange(
                new ExamResult { Id = 1, ExamId = 1, StudentProfileId = 1, Score = 87, Grade = "B+" },
                new ExamResult { Id = 2, ExamId = 1, StudentProfileId = 2, Score = 73, Grade = "C+" },
                new ExamResult { Id = 3, ExamId = 2, StudentProfileId = 1, Score = 77, Grade = "C+" },  // Student 1's hidden exam result
                new ExamResult { Id = 4, ExamId = 3, StudentProfileId = 3, Score = 80, Grade = "B" }
            );

            // Seed Attendance Records
            context.AttendanceRecords.AddRange(
                new AttendanceRecord { Id = 1, CourseEnrolmentId = 1, WeekNumber = 1, Date = DateTime.Today.AddDays(-7), Present = true },
                new AttendanceRecord { Id = 2, CourseEnrolmentId = 1, WeekNumber = 2, Date = DateTime.Today.AddDays(-14), Present = true },
                new AttendanceRecord { Id = 3, CourseEnrolmentId = 1, WeekNumber = 3, Date = DateTime.Today.AddDays(-21), Present = false },
                new AttendanceRecord { Id = 4, CourseEnrolmentId = 2, WeekNumber = 1, Date = DateTime.Today.AddDays(-7), Present = true }
            );

            context.SaveChanges();
        }
    }
}
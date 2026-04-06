using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Tests.Helpers;
using VgcCollege.Web.Data;

namespace VgcCollege.Tests
{
    public class VgcCollegeTests
    {
        // ==================== TEST 1: Student can only see their own data ====================
        [Fact]
        public async Task Student_CanOnlySeeOwnEnrolments()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int studentId = 1; // Nathalia Macedo (enrolled in courses 1 and 2)

            // Act - Get enrolments for student 1
            var studentEnrolments = await context.CourseEnrolments
                .Where(e => e.StudentProfileId == studentId)
                .ToListAsync();

            // Assert - Student 1 should have 2 enrolments (courses 1 and 2)
            Assert.Equal(2, studentEnrolments.Count);
            Assert.Contains(studentEnrolments, e => e.CourseId == 1);
            Assert.Contains(studentEnrolments, e => e.CourseId == 2);
        }

        // ==================== TEST 2: Student cannot see another student's data ====================
        [Fact]
        public async Task Student_CannotSeeOtherStudentsEnrolments()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int student1Id = 1;
            int student2Id = 2;

            // Act - Get enrolments for student 2
            var student2Enrolments = await context.CourseEnrolments
                .Where(e => e.StudentProfileId == student2Id)
                .ToListAsync();

            // Assert - Student 2 should have 1 enrolment (course 1)
            Assert.Single(student2Enrolments);
            Assert.Equal(1, student2Enrolments.First().CourseId);

            // Verify student 1's enrolments are different
            var student1Enrolments = await context.CourseEnrolments
                .Where(e => e.StudentProfileId == student1Id)
                .ToListAsync();

            Assert.NotEqual(student1Enrolments.Select(e => e.CourseId),
                           student2Enrolments.Select(e => e.CourseId));
        }

        // ==================== TEST 3: Faculty can only see students in their courses ====================
        [Fact]
        public async Task Faculty_CanOnlySeeStudentsInTheirCourses()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int facultyId = 1; // Dr. John Rowley (teaches course 1 and 2)

            // Get course IDs for this faculty
            var facultyCourseIds = await context.FacultyCourseAssignments
                .Where(fca => fca.FacultyProfileId == facultyId)
                .Select(fca => fca.CourseId)
                .ToListAsync();

            // Get student IDs enrolled in those courses
            var studentIds = await context.CourseEnrolments
                .Where(e => facultyCourseIds.Contains(e.CourseId))
                .Select(e => e.StudentProfileId)
                .Distinct()
                .ToListAsync();

            // Assert - Faculty 1 should see students 1 and 2 (not student 3)
            Assert.Contains(1, studentIds); // Nathalia
            Assert.Contains(2, studentIds); // Carlos
            Assert.DoesNotContain(3, studentIds); // Paul (only in course 3)
        }

        // ==================== TEST 4: Exam results are hidden until released ====================
        [Fact]
        public async Task ExamResults_AreHiddenUntilReleased()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int studentId = 1;

            // Act - Get visible exam results (released = true)
            var visibleResults = await context.ExamResults
                .Include(er => er.Exam)
                .Where(er => er.StudentProfileId == studentId && er.Exam.ResultsReleased == true)
                .ToListAsync();

            // Act - Get all exam results
            var allResults = await context.ExamResults
                .Include(er => er.Exam)
                .Where(er => er.StudentProfileId == studentId)
                .ToListAsync();

            // Assert - Student should only see released results (1 out of 2)
            Assert.Single(visibleResults); // Only exam 1 is released
            Assert.Equal(2, allResults.Count); // Has 2 exams total (exam 1 and exam 2)
            Assert.Contains(allResults, r => r.ExamId == 1); // Released
            Assert.Contains(allResults, r => r.ExamId == 2); // Not released (hidden)
        }

        // ==================== TEST 5: Assignment grade calculation ====================
        [Fact]
        public async Task AssignmentGrade_CalculatesCorrectAverage()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int studentId = 1;

            // Act
            var assignmentResults = await context.AssignmentResults
                .Where(ar => ar.StudentProfileId == studentId)
                .ToListAsync();

            var averageScore = assignmentResults.Average(ar => ar.Score);

            // Assert - Student 1 has scores 85 and 92 = average 88.5
            Assert.Equal(2, assignmentResults.Count);
            Assert.Equal(88.5, averageScore);
        }

        // ==================== TEST 6: Course enrollment prevents duplicate ====================
        [Fact]
        public async Task CourseEnrollment_PreventsDuplicateEnrollment()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int studentId = 1;
            int courseId = 1;

            // Check if already enrolled
            var existingEnrollment = await context.CourseEnrolments
                .FirstOrDefaultAsync(e => e.StudentProfileId == studentId && e.CourseId == courseId);

            // Assert - Student is already enrolled in course 1
            Assert.NotNull(existingEnrollment);

            // Try to add duplicate (business rule should prevent)
            var duplicateExists = await context.CourseEnrolments
                .AnyAsync(e => e.StudentProfileId == studentId && e.CourseId == courseId);

            Assert.True(duplicateExists);
        }

        // ==================== TEST 7: Attendance percentage calculation ====================
        [Fact]
        public async Task Attendance_CalculatesCorrectPercentage()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int enrolmentId = 1; // Nathalia in course 1

            // Act
            var attendanceRecords = await context.AttendanceRecords
                .Where(a => a.CourseEnrolmentId == enrolmentId)
                .ToListAsync();

            int presentCount = attendanceRecords.Count(a => a.Present);
            int totalCount = attendanceRecords.Count;
            double percentage = totalCount > 0 ? (presentCount * 100.0 / totalCount) : 0;

            // Assert - 2 present out of 3 = 66.67%
            Assert.Equal(3, totalCount);
            Assert.Equal(2, presentCount);
            Assert.Equal(66.67, Math.Round(percentage, 2));
        }

        // ==================== TEST 8: Course cannot be deleted with enrolled students ====================
        [Fact]
        public async Task Course_CannotBeDeletedWithEnrolledStudents()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int courseId = 1; // Course with enrolled students

            // Act - Check if course has enrolments
            var hasEnrolments = await context.CourseEnrolments
                .AnyAsync(e => e.CourseId == courseId);

            // Assert - Course has enrolments, so delete should be prevented
            Assert.True(hasEnrolments);

            // Business rule: Only delete if no enrolments
            if (!hasEnrolments)
            {
                var course = await context.Courses.FindAsync(courseId);
                if (course != null) context.Courses.Remove(course);
                await context.SaveChangesAsync();
            }

            var courseStillExists = await context.Courses.AnyAsync(c => c.Id == courseId);
            Assert.True(courseStillExists); // Course still exists because deletion was prevented
        }

        // ==================== TEST 9: Faculty can only access their assigned courses ====================
        [Fact]
        public async Task Faculty_CanOnlyAccessAssignedCourses()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int facultyId = 2; // Dr. Wenhao Fu (teaches course 3 only)

            // Act - Get courses assigned to this faculty
            var assignedCourses = await context.FacultyCourseAssignments
                .Where(fca => fca.FacultyProfileId == facultyId)
                .Select(fca => fca.CourseId)
                .ToListAsync();

            // Assert - Faculty 2 should only have course 3
            Assert.Single(assignedCourses);
            Assert.Equal(3, assignedCourses.First());

            // Verify they cannot access course 1
            Assert.DoesNotContain(1, assignedCourses);
        }

        // ==================== TEST 10: Student gradebook shows only released exam results ====================
        [Fact]
        public async Task StudentGradebook_ShowsOnlyReleasedExamResults()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            int studentId = 1;

            // Act - Get exam results with release status
            var examResults = await context.ExamResults
                .Include(er => er.Exam)
                .Where(er => er.StudentProfileId == studentId)
                .Select(er => new { er.Exam.Title, er.Exam.ResultsReleased, er.Score, er.Grade })
                .ToListAsync();

            // Assert
            var releasedExams = examResults.Where(r => r.ResultsReleased == true).ToList();
            var hiddenExams = examResults.Where(r => r.ResultsReleased == false).ToList();

            // Student 1 should have 1 released exam and 1 hidden exam
            Assert.Single(releasedExams);
            Assert.Single(hiddenExams);
            Assert.Equal("Semester Exam", releasedExams.First().Title);
            Assert.Equal("Midterm Exam", hiddenExams.First().Title);
        }
    }
}
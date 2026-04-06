using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Web.Data;
using VgcCollege.Web.ViewModels;

namespace VgcCollege.Web.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<FacultyController> _logger;

        public FacultyController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<FacultyController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ==================== DASHBOARD ====================
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

            if (faculty == null)
            {
                _logger.LogWarning("Faculty profile not found for user {UserId}", user.Id);
                return RedirectToAction("AccessDenied", "Home");
            }

            // Get courses this faculty teaches
            var myCourses = await _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                    .ThenInclude(c => c.Branch)
                .Where(fca => fca.FacultyProfileId == faculty.Id)
                .Select(fca => fca.Course)
                .ToListAsync();

            // Get total students across all their courses
            var courseIds = myCourses.Select(c => c.Id).ToList();
            var studentIds = await _context.CourseEnrolments
                .Where(e => courseIds.Contains(e.CourseId) && e.Status == "Active")
                .Select(e => e.StudentProfileId)
                .Distinct()
                .ToListAsync();

            // Get pending assignments that need grading (score = 0 means not graded)
            var pendingGrading = await _context.AssignmentResults
                .Include(ar => ar.Assignment)
                .Where(ar => courseIds.Contains(ar.Assignment.CourseId) && ar.Score == 0)
                .CountAsync();

            ViewBag.Faculty = faculty;
            ViewBag.MyCourses = myCourses;
            ViewBag.TotalCourses = myCourses.Count;
            ViewBag.TotalStudents = studentIds.Count;
            ViewBag.PendingGrading = pendingGrading;

            return View();
        }

        // ==================== MY STUDENTS ====================
        public async Task<IActionResult> MyStudents(int? courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

            if (faculty == null) return RedirectToAction("AccessDenied", "Home");

            // Get courses this faculty teaches
            var courses = await _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                    .ThenInclude(c => c.Branch)
                .Where(fca => fca.FacultyProfileId == faculty.Id)
                .Select(fca => fca.Course)
                .ToListAsync();

            List<StudentWithAttendanceViewModel> studentsWithAttendance = new List<StudentWithAttendanceViewModel>();
            Course selectedCourse = null;

            if (courseId.HasValue && courseId.Value > 0)
            {
                selectedCourse = courses.FirstOrDefault(c => c.Id == courseId);
                if (selectedCourse != null)
                {
                    var students = await _context.CourseEnrolments
                        .Include(e => e.StudentProfile)
                        .Include(e => e.AttendanceRecords)
                        .Where(e => e.CourseId == courseId.Value && e.Status == "Active")
                        .ToListAsync();

                    foreach (var enrolment in students)
                    {
                        var presentCount = enrolment.AttendanceRecords.Count(a => a.Present);
                        var totalCount = enrolment.AttendanceRecords.Count;
                        var attendancePercent = totalCount > 0 ? (presentCount * 100 / totalCount) : 0;

                        studentsWithAttendance.Add(new StudentWithAttendanceViewModel
                        {
                            Student = enrolment.StudentProfile,
                            AttendancePercent = attendancePercent,
                            EnrolmentId = enrolment.Id
                        });
                    }
                }
            }

            ViewBag.Courses = courses;
            ViewBag.SelectedCourse = selectedCourse;
            ViewBag.StudentsWithAttendance = studentsWithAttendance;
            ViewBag.Faculty = faculty;

            return View();
        }

        // ==================== STUDENT DETAILS ====================
        public async Task<IActionResult> StudentDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

            if (faculty == null) return RedirectToAction("AccessDenied", "Home");

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            // Verify this student is enrolled in a course this faculty teaches
            var courseIds = await _context.FacultyCourseAssignments
                .Where(fca => fca.FacultyProfileId == faculty.Id)
                .Select(fca => fca.CourseId)
                .ToListAsync();

            var isInMyCourse = await _context.CourseEnrolments
                .AnyAsync(e => e.StudentProfileId == id &&
                              courseIds.Contains(e.CourseId));

            if (!isInMyCourse)
            {
                _logger.LogWarning("Faculty {FacultyId} attempted to access student {StudentId} not in their courses", faculty.Id, id);
                return RedirectToAction("AccessDenied", "Home");
            }

            // Get student's enrolments
            var enrolments = await _context.CourseEnrolments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Where(e => e.StudentProfileId == id && e.Status == "Active")
                .ToListAsync();

            // Get assignment results
            var assignmentResults = await _context.AssignmentResults
                .Include(ar => ar.Assignment)
                    .ThenInclude(a => a.Course)
                .Where(ar => ar.StudentProfileId == id)
                .ToListAsync();

            // Get exam results
            var examResults = await _context.ExamResults
                .Include(er => er.Exam)
                    .ThenInclude(e => e.Course)
                .Where(er => er.StudentProfileId == id)
                .ToListAsync();

            // Get attendance records
            var attendance = await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                .Where(a => a.CourseEnrolment.StudentProfileId == id)
                .ToListAsync();

            ViewBag.Enrolments = enrolments;
            ViewBag.AssignmentResults = assignmentResults;
            ViewBag.ExamResults = examResults;
            ViewBag.Attendance = attendance;

            return View(student);
        }

        // ==================== ASSIGNMENTS (Gradebook) ====================
        public async Task<IActionResult> Assignments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

            if (faculty == null) return RedirectToAction("AccessDenied", "Home");

            var courseIds = await _context.FacultyCourseAssignments
                .Where(fca => fca.FacultyProfileId == faculty.Id)
                .Select(fca => fca.CourseId)
                .ToListAsync();

            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Branch)
                .Where(a => courseIds.Contains(a.CourseId))
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();

            return View(assignments);
        }

        // ==================== ENTER GRADES ====================
        public async Task<IActionResult> EnterGrades(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

            if (faculty == null) return RedirectToAction("AccessDenied", "Home");

            // Verify faculty teaches this course
            var teachesCourse = await _context.FacultyCourseAssignments
                .AnyAsync(fca => fca.FacultyProfileId == faculty.Id && fca.CourseId == courseId);

            if (!teachesCourse)
            {
                _logger.LogWarning("Faculty {FacultyId} attempted to enter grades for course {CourseId} they don't teach", faculty.Id, courseId);
                return RedirectToAction("AccessDenied", "Home");
            }

            var course = await _context.Courses
                .Include(c => c.Assignments)
                .Include(c => c.Exams)
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            var students = await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Where(e => e.CourseId == courseId && e.Status == "Active")
                .Select(e => e.StudentProfile)
                .ToListAsync();

            // Load assignment results and exam results for each student
            var assignmentResults = await _context.AssignmentResults
                .Where(ar => course.Assignments.Select(a => a.Id).Contains(ar.AssignmentId))
                .ToListAsync();

            var examResults = await _context.ExamResults
                .Where(er => course.Exams.Select(e => e.Id).Contains(er.ExamId))
                .ToListAsync();

            ViewBag.Course = course;
            ViewBag.Students = students;
            ViewBag.AssignmentResults = assignmentResults;
            ViewBag.ExamResults = examResults;

            return View();
        }

        // ==================== SAVE ASSIGNMENT GRADE ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAssignmentGrade(int assignmentId, int studentId, int score, string feedback)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var faculty = await _context.FacultyProfiles
                    .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

                if (faculty == null) return Unauthorized();

                var assignment = await _context.Assignments
                    .Include(a => a.Course)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId);

                if (assignment == null) return NotFound();

                // Verify faculty teaches this course
                var teachesCourse = await _context.FacultyCourseAssignments
                    .AnyAsync(fca => fca.FacultyProfileId == faculty.Id && fca.CourseId == assignment.CourseId);

                if (!teachesCourse) return Unauthorized();

                var result = await _context.AssignmentResults
                    .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.StudentProfileId == studentId);

                if (result == null)
                {
                    result = new AssignmentResult
                    {
                        AssignmentId = assignmentId,
                        StudentProfileId = studentId,
                        Score = score,
                        Feedback = feedback ?? string.Empty
                    };
                    _context.AssignmentResults.Add(result);
                    _logger.LogInformation("Assignment grade added: Assignment {AssignmentId}, Student {StudentId}, Score {Score} by {User}",
                        assignmentId, studentId, score, User.Identity?.Name);
                }
                else
                {
                    result.Score = score;
                    result.Feedback = feedback ?? string.Empty;
                    _context.Update(result);
                    _logger.LogInformation("Assignment grade updated: Assignment {AssignmentId}, Student {StudentId}, Score {Score} by {User}",
                        assignmentId, studentId, score, User.Identity?.Name);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving assignment grade for Assignment {AssignmentId}, Student {StudentId}",
                    assignmentId, studentId);
                return Json(new { success = false, error = ex.Message });
            }
        }

        // ==================== SAVE EXAM GRADE ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveExamGrade(int examId, int studentId, int score, string grade)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var faculty = await _context.FacultyProfiles
                    .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

                if (faculty == null) return Unauthorized();

                var exam = await _context.Exams
                    .Include(e => e.Course)
                    .FirstOrDefaultAsync(e => e.Id == examId);

                if (exam == null) return NotFound();

                // Verify faculty teaches this course
                var teachesCourse = await _context.FacultyCourseAssignments
                    .AnyAsync(fca => fca.FacultyProfileId == faculty.Id && fca.CourseId == exam.CourseId);

                if (!teachesCourse) return Unauthorized();

                var result = await _context.ExamResults
                    .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentProfileId == studentId);

                if (result == null)
                {
                    result = new ExamResult
                    {
                        ExamId = examId,
                        StudentProfileId = studentId,
                        Score = score,
                        Grade = grade ?? string.Empty
                    };
                    _context.ExamResults.Add(result);
                    _logger.LogInformation("Exam grade added: Exam {ExamId}, Student {StudentId}, Score {Score} by {User}",
                        examId, studentId, score, User.Identity?.Name);
                }
                else
                {
                    result.Score = score;
                    result.Grade = grade ?? string.Empty;
                    _context.Update(result);
                    _logger.LogInformation("Exam grade updated: Exam {ExamId}, Student {StudentId}, Score {Score} by {User}",
                        examId, studentId, score, User.Identity?.Name);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving exam grade for Exam {ExamId}, Student {StudentId}",
                    examId, studentId);
                return Json(new { success = false, error = ex.Message });
            }
        }

        // ==================== MARK ATTENDANCE ====================
        public async Task<IActionResult> MarkAttendance(int id, int week = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

            if (faculty == null) return RedirectToAction("AccessDenied", "Home");

            var course = await _context.Courses
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            // Verify faculty teaches this course
            var teachesCourse = await _context.FacultyCourseAssignments
                .AnyAsync(fca => fca.FacultyProfileId == faculty.Id && fca.CourseId == id);

            if (!teachesCourse) return RedirectToAction("AccessDenied", "Home");

            // Get students enrolled in this course with their attendance records
            var enrolments = await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.AttendanceRecords)
                .Where(e => e.CourseId == id && e.Status == "Active")
                .ToListAsync();

            ViewBag.Enrolments = enrolments;
            ViewBag.CurrentWeek = week;
            ViewBag.Course = course;

            return View();
        }

        // ==================== SAVE ATTENDANCE ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(int courseId, int weekNumber, IFormCollection form)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var faculty = await _context.FacultyProfiles
                    .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

                if (faculty == null) return Unauthorized();

                // Verify faculty teaches this course
                var teachesCourse = await _context.FacultyCourseAssignments
                    .AnyAsync(fca => fca.FacultyProfileId == faculty.Id && fca.CourseId == courseId);

                if (!teachesCourse) return Unauthorized();

                // Get all students enrolled in this course
                var enrolments = await _context.CourseEnrolments
                    .Where(e => e.CourseId == courseId && e.Status == "Active")
                    .ToListAsync();

                foreach (var enrolment in enrolments)
                {
                    var isPresent = form[$"attendance_{enrolment.StudentProfileId}"] == "true";

                    // Check if attendance record already exists
                    var existingRecord = await _context.AttendanceRecords
                        .FirstOrDefaultAsync(a => a.CourseEnrolmentId == enrolment.Id && a.WeekNumber == weekNumber);

                    if (existingRecord != null)
                    {
                        existingRecord.Present = isPresent;
                        _context.Update(existingRecord);
                    }
                    else
                    {
                        var newRecord = new AttendanceRecord
                        {
                            CourseEnrolmentId = enrolment.Id,
                            WeekNumber = weekNumber,
                            Date = DateTime.Today,
                            Present = isPresent
                        };
                        _context.AttendanceRecords.Add(newRecord);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Attendance saved for course {CourseId}, week {WeekNumber} by {User}",
                    courseId, weekNumber, User.Identity?.Name);

                TempData["Success"] = $"Attendance for Week {weekNumber} saved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving attendance for course {CourseId}, week {WeekNumber}", courseId, weekNumber);
                TempData["Error"] = "An error occurred while saving attendance.";
            }

            return RedirectToAction(nameof(MarkAttendance), new { id = courseId, week = weekNumber });
        }
    
    // ==================== CREATE ASSIGNMENT ====================
// GET: Create Assignment
public async Task<IActionResult> CreateAssignment()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

            if (faculty == null) return RedirectToAction("AccessDenied", "Home");

            // Get courses this faculty teaches
            var myCourses = await _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                .Where(fca => fca.FacultyProfileId == faculty.Id)
                .Select(fca => fca.Course)
                .ToListAsync();

            ViewBag.MyCourses = new SelectList(myCourses, "Id", "Name");

            return View();
        }

        // POST: Create Assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(Assignment assignment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user.Id);

            if (faculty == null) return RedirectToAction("AccessDenied", "Home");

            // Verify faculty teaches this course
            var teachesCourse = await _context.FacultyCourseAssignments
                .AnyAsync(fca => fca.FacultyProfileId == faculty.Id && fca.CourseId == assignment.CourseId);

            if (!teachesCourse)
            {
                TempData["Error"] = "You can only create assignments for courses you teach.";
                return RedirectToAction("Assignments");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Assignments.Add(assignment);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Assignment created: {Title} for course {CourseId} by {User}",
                        assignment.Title, assignment.CourseId, User.Identity?.Name);
                    TempData["Success"] = $"Assignment '{assignment.Title}' created successfully.";
                    return RedirectToAction("Assignments");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating assignment");
                    ModelState.AddModelError("", "An error occurred while creating the assignment.");
                }
            }

            var myCourses = await _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                .Where(fca => fca.FacultyProfileId == faculty.Id)
                .Select(fca => fca.Course)
                .ToListAsync();
            ViewBag.MyCourses = new SelectList(myCourses, "Id", "Name", assignment.CourseId);

            return View(assignment);
        }
    }

}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<StudentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Student Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Find the StudentProfile linked to this Identity user
            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);

            if (student == null)
            {
                _logger.LogWarning("Student profile not found for user {UserId}", user.Id);
                return RedirectToAction("AccessDenied", "Home");
            }

            // Get student's enrolments with courses
            var enrolments = await _context.CourseEnrolments
                .Include(e => e.Course)
                .ThenInclude(c => c.Branch)
                .Where(e => e.StudentProfileId == student.Id && e.Status == "Active")
                .ToListAsync();

            // Get assignment results
            var assignmentResults = await _context.AssignmentResults
                .Include(ar => ar.Assignment)
                .ThenInclude(a => a.Course)
                .Where(ar => ar.StudentProfileId == student.Id)
                .ToListAsync();

            // Get exam results (only released ones)
            var examResults = await _context.ExamResults
                .Include(er => er.Exam)
                .ThenInclude(e => e.Course)
                .Where(er => er.StudentProfileId == student.Id && er.Exam.ResultsReleased == true)
                .ToListAsync();

            ViewBag.Student = student;
            ViewBag.Enrolments = enrolments;
            ViewBag.AssignmentResults = assignmentResults;
            ViewBag.ExamResults = examResults;

            return View();
        }

        // GET: Student Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);

            if (student == null) return RedirectToAction("AccessDenied", "Home");

            return View(student);
        }

        // GET: Student Courses
        public async Task<IActionResult> Courses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);

            if (student == null) return RedirectToAction("AccessDenied", "Home");

            var enrolments = await _context.CourseEnrolments
                .Include(e => e.Course)
                .ThenInclude(c => c.Branch)
                .Include(e => e.AttendanceRecords)
                .Where(e => e.StudentProfileId == student.Id)
                .ToListAsync();

            return View(enrolments);
        }
        // GET: Student Attendance
        public async Task<IActionResult> Attendance()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);

            if (student == null) return RedirectToAction("AccessDenied", "Home");

            // Get student's enrolments with attendance records
            var enrolments = await _context.CourseEnrolments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Include(e => e.AttendanceRecords)
                .Where(e => e.StudentProfileId == student.Id)
                .ToListAsync();

            ViewBag.Enrolments = enrolments;

            return View();
        }

        // GET: Student Results
        public async Task<IActionResult> Results()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);

            if (student == null) return RedirectToAction("AccessDenied", "Home");

            // Assignment results
            var assignmentResults = await _context.AssignmentResults
                .Include(ar => ar.Assignment)
                .ThenInclude(a => a.Course)
                .Where(ar => ar.StudentProfileId == student.Id)
                .ToListAsync();

            // Exam results (only released ones)
            var examResults = await _context.ExamResults
                .Include(er => er.Exam)
                .ThenInclude(e => e.Course)
                .Where(er => er.StudentProfileId == student.Id && er.Exam.ResultsReleased == true)
                .ToListAsync();

            ViewBag.AssignmentResults = assignmentResults;
            ViewBag.ExamResults = examResults;

            return View();
        }
    }
}
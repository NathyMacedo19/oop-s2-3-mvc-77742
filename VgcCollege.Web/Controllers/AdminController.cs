using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // ==================== DASHBOARD ====================
        public async Task<IActionResult> Index()
        {
            var branchCount = await _context.Branches.CountAsync();
            var courseCount = await _context.Courses.CountAsync();
            var studentCount = await _context.StudentProfiles.CountAsync();
            var facultyCount = await _context.FacultyProfiles.CountAsync();

            ViewBag.BranchCount = branchCount;
            ViewBag.CourseCount = courseCount;
            ViewBag.StudentCount = studentCount;
            ViewBag.FacultyCount = facultyCount;

            // Recent Courses (last 3 courses added)
            ViewBag.RecentCourses = await _context.Courses
                .Include(c => c.Branch)
                .OrderByDescending(c => c.StartDate)
                .Take(3)
                .ToListAsync();

            // Recent Enrolments (last 5 enrolments)
            ViewBag.RecentEnrolments = await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                .ThenInclude(c => c.Branch)
                .OrderByDescending(e => e.EnrolDate)
                .Take(5)
                .ToListAsync();

            // Recently Released Exams
            ViewBag.RecentExamReleases = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Results)
                .Where(e => e.ResultsReleased == true)
                .OrderByDescending(e => e.Date)
                .Take(3)
                .ToListAsync();

            // Additional Stats for Footer
            ViewBag.ExamReleaseCount = await _context.Exams.CountAsync(e => e.ResultsReleased == true);
            ViewBag.PendingExamCount = await _context.Exams.CountAsync(e => e.ResultsReleased == false);
            ViewBag.ActiveEnrolments = await _context.CourseEnrolments.CountAsync(e => e.Status == "Active");
            ViewBag.TotalAssignments = await _context.Assignments.CountAsync();

            return View();
        }

        // ==================== BRANCHES ====================
        public async Task<IActionResult> Branches()
        {
            var branches = await _context.Branches
                .Include(b => b.Courses)
                .ToListAsync();
            return View(branches);
        }

        public async Task<IActionResult> BranchDetails(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Courses)
                    .ThenInclude(c => c.Enrolments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null) return NotFound();
            return View(branch);
        }

        public IActionResult CreateBranch()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBranch(Branch branch)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Branches.Add(branch);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Branch created: {BranchName} by {User}", branch.Name, User.Identity?.Name);
                    return RedirectToAction(nameof(Branches));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating branch {BranchName}", branch.Name);
                    ModelState.AddModelError("", "An error occurred while creating the branch.");
                }
            }
            return View(branch);
        }

        public async Task<IActionResult> EditBranch(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Courses)
                    .ThenInclude(c => c.Enrolments)
                .Include(b => b.Courses)
                    .ThenInclude(c => c.FacultyAssignments)
                        .ThenInclude(fa => fa.FacultyProfile)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null) return NotFound();

            // Get all faculty for the dropdown
            ViewBag.FacultyList = new SelectList(_context.FacultyProfiles, "Id", "Name");

            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBranch(int id, Branch branch)
        {
            if (id != branch.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(branch);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Branch updated: {BranchName} by {User}", branch.Name, User.Identity?.Name);
                    return RedirectToAction(nameof(Branches));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating branch {BranchId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the branch.");
                }
            }
            return View(branch);
        }

        public async Task<IActionResult> DeleteBranch(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Courses)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (branch == null) return NotFound();
            return View(branch);
        }

        [HttpPost, ActionName("DeleteBranch")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBranchConfirmed(int id)
        {
            try
            {
                var branch = await _context.Branches.FindAsync(id);
                if (branch != null)
                {
                    _context.Branches.Remove(branch);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Branch deleted: {BranchName} by {User}", branch.Name, User.Identity?.Name);
                }
                return RedirectToAction(nameof(Branches));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch {BranchId}", id);
                TempData["Error"] = "Cannot delete branch that has courses. Remove courses first.";
                return RedirectToAction(nameof(Branches));
            }
        }

        // ==================== COURSES ====================
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                .ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> CourseDetails(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                    .ThenInclude(e => e.StudentProfile)
                .Include(c => c.FacultyAssignments)
                    .ThenInclude(fa => fa.FacultyProfile)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var enrolledStudents = course.Enrolments?
                .Where(e => e.Status == "Active")
                .Select(e => e.StudentProfile)
                .ToList() ?? new List<StudentProfile>();

            var facultyTeachers = course.FacultyAssignments?
                .Select(fa => fa.FacultyProfile)
                .ToList() ?? new List<FacultyProfile>();

            ViewBag.EnrolledStudents = enrolledStudents;
            ViewBag.FacultyTeachers = facultyTeachers;

            return View(course);
        }

        public IActionResult CreateCourse()
        {
            ViewBag.Branches = new SelectList(_context.Branches, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Course created: {CourseName} by {User}", course.Name, User.Identity?.Name);
                    return RedirectToAction(nameof(Courses));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating course {CourseName}", course.Name);
                    ModelState.AddModelError("", "An error occurred while creating the course.");
                }
            }
            ViewBag.Branches = new SelectList(_context.Branches, "Id", "Name", course.BranchId);
            return View(course);
        }

        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            ViewBag.Branches = new SelectList(_context.Branches, "Id", "Name", course.BranchId);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course course)
        {
            if (id != course.Id) return NotFound();

            // Remove unique name validation - allow same name in different branches
            ModelState.Remove("Branch");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Course updated: {CourseName} by {User}", course.Name, User.Identity?.Name);
                    TempData["Success"] = "Course updated successfully.";
                    return RedirectToAction(nameof(CourseDetails), new { id = course.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating course {CourseId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the course.");
                }
            }
            ViewBag.Branches = new SelectList(_context.Branches, "Id", "Name", course.BranchId);
            return View(course);
        }

        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("DeleteCourse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourseConfirmed(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course != null)
                {
                    _context.Courses.Remove(course);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Course deleted: {CourseName} by {User}", course.Name, User.Identity?.Name);
                }
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId}", id);
                TempData["Error"] = "Cannot delete course that has enrolled students.";
                return RedirectToAction(nameof(Courses));
            }
        }

        // ==================== STUDENTS ====================
        public async Task<IActionResult> Students()
        {
            var students = await _context.StudentProfiles.ToListAsync();
            return View(students);
        }

        public async Task<IActionResult> StudentDetails(int id)
        {
            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            var enrolments = await _context.CourseEnrolments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Where(e => e.StudentProfileId == id)
                .ToListAsync();

            var assignmentResults = await _context.AssignmentResults
                .Include(ar => ar.Assignment)
                    .ThenInclude(a => a.Course)
                .Where(ar => ar.StudentProfileId == id)
                .ToListAsync();

            var examResults = await _context.ExamResults
                .Include(er => er.Exam)
                    .ThenInclude(e => e.Course)
                .Where(er => er.StudentProfileId == id)
                .ToListAsync();

            ViewBag.Enrolments = enrolments;
            ViewBag.AssignmentResults = assignmentResults;
            ViewBag.ExamResults = examResults;

            return View(student);
        }

        public IActionResult CreateStudent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentProfile student, string password, string email)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new IdentityUser { UserName = email, Email = email };
                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Student");
                        student.IdentityUserId = user.Id;
                        student.Email = email;
                        _context.StudentProfiles.Add(student);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Student created: {StudentName} ({StudentNumber}) by {User}",
                            student.Name, student.StudentNumber, User.Identity?.Name);
                        return RedirectToAction(nameof(Students));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating student {StudentName}", student.Name);
                    ModelState.AddModelError("", "An error occurred while creating the student.");
                }
            }
            return View(student);
        }

        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _context.StudentProfiles.FindAsync(id);
            if (student == null) return NotFound();

            // Get enrolled courses for this student
            var enrolledCourses = await _context.CourseEnrolments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Where(e => e.StudentProfileId == id)
                .ToListAsync();

            // Get all available courses (not already enrolled)
            var enrolledCourseIds = enrolledCourses.Select(e => e.CourseId).ToList();
            var availableCourses = await _context.Courses
                .Include(c => c.Branch)
                .Where(c => !enrolledCourseIds.Contains(c.Id))
                .ToListAsync();

            // Get counts for academic summary
            var assignmentCount = await _context.AssignmentResults
                .CountAsync(ar => ar.StudentProfileId == id);
            var examCount = await _context.ExamResults
                .CountAsync(er => er.StudentProfileId == id);

            ViewBag.EnrolledCourses = enrolledCourses;
            ViewBag.AvailableCourses = availableCourses;
            ViewBag.AssignmentCount = assignmentCount;
            ViewBag.ExamCount = examCount;

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int id, StudentProfile student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Student updated: {StudentName} by {User}", student.Name, User.Identity?.Name);
                    TempData["Success"] = "Student information updated successfully.";
                    return RedirectToAction(nameof(EditStudent), new { id = student.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating student {StudentId}", id);
                    ModelState.AddModelError("", "Error updating student.");
                }
            }

            // Reload data for view on error
            var enrolledCourses = await _context.CourseEnrolments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Where(e => e.StudentProfileId == id)
                .ToListAsync();

            var enrolledCourseIds = enrolledCourses.Select(e => e.CourseId).ToList();
            var availableCourses = await _context.Courses
                .Include(c => c.Branch)
                .Where(c => !enrolledCourseIds.Contains(c.Id))
                .ToListAsync();

            ViewBag.EnrolledCourses = enrolledCourses;
            ViewBag.AvailableCourses = availableCourses;

            return View(student);
        }

        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.StudentProfiles.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentConfirmed(int id)
        {
            try
            {
                var student = await _context.StudentProfiles.FindAsync(id);
                if (student != null)
                {
                    var user = await _userManager.FindByIdAsync(student.IdentityUserId);
                    if (user != null) await _userManager.DeleteAsync(user);

                    _context.StudentProfiles.Remove(student);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Student deleted: ID {StudentId} by {User}", id, User.Identity?.Name);
                }
                return RedirectToAction(nameof(Students));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {StudentId}", id);
                TempData["Error"] = "Cannot delete student with active enrolments.";
                return RedirectToAction(nameof(Students));
            }
        }

        // ==================== STUDENT ENROLLMENT MANAGEMENT ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourseEnrollment(int studentId, int courseId)
        {
            try
            {
                var student = await _context.StudentProfiles.FindAsync(studentId);
                if (student == null) return NotFound();

                var course = await _context.Courses.FindAsync(courseId);
                if (course == null) return NotFound();

                var existing = await _context.CourseEnrolments
                    .FirstOrDefaultAsync(e => e.StudentProfileId == studentId && e.CourseId == courseId);

                if (existing != null)
                {
                    TempData["Error"] = "Student is already enrolled in this course.";
                    return RedirectToAction(nameof(EditStudent), new { id = studentId });
                }

                var enrolment = new CourseEnrolment
                {
                    StudentProfileId = studentId,
                    CourseId = courseId,
                    EnrolDate = DateTime.Today,
                    Status = "Active"
                };

                _context.CourseEnrolments.Add(enrolment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Student {StudentName} enrolled in course {CourseName} by {User}",
                    student.Name, course.Name, User.Identity?.Name);

                TempData["Success"] = $"Student enrolled in {course.Name} successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling student {StudentId} in course {CourseId}", studentId, courseId);
                TempData["Error"] = "An error occurred while enrolling the student.";
            }

            return RedirectToAction(nameof(EditStudent), new { id = studentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCourseEnrollment(int enrolmentId, int studentId)
        {
            try
            {
                var enrolment = await _context.CourseEnrolments
                    .Include(e => e.Course)
                    .FirstOrDefaultAsync(e => e.Id == enrolmentId);

                if (enrolment != null)
                {
                    _context.CourseEnrolments.Remove(enrolment);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Student removed from course {CourseName} by {User}",
                        enrolment.Course?.Name, User.Identity?.Name);

                    TempData["Success"] = "Student removed from course successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing enrollment {EnrolmentId}", enrolmentId);
                TempData["Error"] = "An error occurred while removing the enrollment.";
            }

            return RedirectToAction(nameof(EditStudent), new { id = studentId });
        }

        // ==================== FACULTY ====================
        public async Task<IActionResult> Faculty()
        {
            var faculty = await _context.FacultyProfiles
                .Include(f => f.CourseAssignments)
                    .ThenInclude(fca => fca.Course)
                .ToListAsync();
            return View(faculty);
        }

        public async Task<IActionResult> FacultyDetails(int id)
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null) return NotFound();

            var assignedCourses = await _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                    .ThenInclude(c => c.Branch)
                .Include(fca => fca.Course)
                    .ThenInclude(c => c.Enrolments)
                .Where(fca => fca.FacultyProfileId == id)
                .Select(fca => fca.Course)
                .ToListAsync();

            ViewBag.AssignedCourses = assignedCourses;

            return View(faculty);
        }

        public IActionResult CreateFaculty()
        {
            ViewBag.Courses = new SelectList(_context.Courses, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFaculty(FacultyProfile faculty, string password, string email, List<int>? selectedCourses)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new IdentityUser { UserName = email, Email = email };
                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Faculty");
                        faculty.IdentityUserId = user.Id;
                        faculty.Email = email;
                        _context.FacultyProfiles.Add(faculty);
                        await _context.SaveChangesAsync();

                        if (selectedCourses != null && selectedCourses.Any())
                        {
                            foreach (var courseId in selectedCourses)
                            {
                                _context.FacultyCourseAssignments.Add(new FacultyCourseAssignment
                                {
                                    FacultyProfileId = faculty.Id,
                                    CourseId = courseId
                                });
                            }
                            await _context.SaveChangesAsync();
                        }

                        _logger.LogInformation("Faculty created: {FacultyName} by {User}",
                            faculty.Name, User.Identity?.Name);
                        return RedirectToAction(nameof(Faculty));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating faculty {FacultyName}", faculty.Name);
                    ModelState.AddModelError("", "An error occurred while creating the faculty member.");
                }
            }
            ViewBag.Courses = new SelectList(_context.Courses, "Id", "Name");
            return View(faculty);
        }

        public async Task<IActionResult> EditFaculty(int id)
        {
            var faculty = await _context.FacultyProfiles.FindAsync(id);
            if (faculty == null) return NotFound();

            // Get assigned courses for this faculty
            var assignedCourses = await _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                    .ThenInclude(c => c.Branch)
                .Include(fca => fca.Course)
                    .ThenInclude(c => c.Enrolments)
                .Where(fca => fca.FacultyProfileId == id)
                .Select(fca => fca.Course)
                .ToListAsync();

            // Get all available courses (not already assigned)
            var assignedCourseIds = assignedCourses.Select(c => c.Id).ToList();
            var availableCourses = await _context.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                .Where(c => !assignedCourseIds.Contains(c.Id))
                .ToListAsync();

            // Get total students across assigned courses
            var totalStudents = assignedCourses.Sum(c => c.Enrolments?.Count ?? 0);

            ViewBag.AssignedCourses = assignedCourses;
            ViewBag.AvailableCourses = availableCourses;
            ViewBag.TotalStudents = totalStudents;

            return View(faculty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFaculty(int id, FacultyProfile faculty, List<int>? selectedCourses)
        {
            if (id != faculty.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faculty);

                    // Update course assignments
                    var existingAssignments = await _context.FacultyCourseAssignments
                        .Where(fca => fca.FacultyProfileId == id)
                        .ToListAsync();
                    _context.FacultyCourseAssignments.RemoveRange(existingAssignments);

                    if (selectedCourses != null && selectedCourses.Any())
                    {
                        foreach (var courseId in selectedCourses)
                        {
                            _context.FacultyCourseAssignments.Add(new FacultyCourseAssignment
                            {
                                FacultyProfileId = faculty.Id,
                                CourseId = courseId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Faculty updated: {FacultyName} by {User}", faculty.Name, User.Identity?.Name);
                    TempData["Success"] = "Faculty information updated successfully.";
                    return RedirectToAction(nameof(EditFaculty), new { id = faculty.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating faculty {FacultyId}", id);
                    ModelState.AddModelError("", "Error updating faculty.");
                }
            }

            // Reload data for view on error
            var assignedCourses = await _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                    .ThenInclude(c => c.Branch)
                .Where(fca => fca.FacultyProfileId == id)
                .Select(fca => fca.Course)
                .ToListAsync();

            var assignedCourseIds = assignedCourses.Select(c => c.Id).ToList();
            var availableCourses = await _context.Courses
                .Include(c => c.Branch)
                .Where(c => !assignedCourseIds.Contains(c.Id))
                .ToListAsync();

            ViewBag.AssignedCourses = assignedCourses;
            ViewBag.AvailableCourses = availableCourses;

            return View(faculty);
        }

        public async Task<IActionResult> DeleteFaculty(int id)
        {
            var faculty = await _context.FacultyProfiles
                .Include(f => f.CourseAssignments)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (faculty == null) return NotFound();
            return View(faculty);
        }

        [HttpPost, ActionName("DeleteFaculty")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFacultyConfirmed(int id)
        {
            try
            {
                var faculty = await _context.FacultyProfiles.FindAsync(id);
                if (faculty != null)
                {
                    var user = await _userManager.FindByIdAsync(faculty.IdentityUserId);
                    if (user != null) await _userManager.DeleteAsync(user);

                    _context.FacultyProfiles.Remove(faculty);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Faculty deleted: ID {FacultyId} by {User}", id, User.Identity?.Name);
                }
                return RedirectToAction(nameof(Faculty));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting faculty {FacultyId}", id);
                TempData["Error"] = "Cannot delete faculty with active course assignments.";
                return RedirectToAction(nameof(Faculty));
            }
        }

        // ==================== FACULTY COURSE ASSIGNMENT MANAGEMENT ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFacultyCourse(int facultyId, int courseId)
        {
            try
            {
                var faculty = await _context.FacultyProfiles.FindAsync(facultyId);
                if (faculty == null) return NotFound();

                var course = await _context.Courses.FindAsync(courseId);
                if (course == null) return NotFound();

                var existing = await _context.FacultyCourseAssignments
                    .FirstOrDefaultAsync(a => a.FacultyProfileId == facultyId && a.CourseId == courseId);

                if (existing != null)
                {
                    TempData["Error"] = "Faculty member is already assigned to this course.";
                    return RedirectToAction(nameof(EditFaculty), new { id = facultyId });
                }

                var assignment = new FacultyCourseAssignment
                {
                    FacultyProfileId = facultyId,
                    CourseId = courseId
                };

                _context.FacultyCourseAssignments.Add(assignment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Faculty {FacultyName} assigned to course {CourseName} by {User}",
                    faculty.Name, course.Name, User.Identity?.Name);

                TempData["Success"] = $"{faculty.Name} assigned to {course.Name} successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning faculty {FacultyId} to course {CourseId}", facultyId, courseId);
                TempData["Error"] = "An error occurred while assigning the course.";
            }

            return RedirectToAction(nameof(EditFaculty), new { id = facultyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFacultyCourse(int facultyId, int courseId)
        {
            try
            {
                var assignment = await _context.FacultyCourseAssignments
                    .Include(a => a.Course)
                    .FirstOrDefaultAsync(a => a.FacultyProfileId == facultyId && a.CourseId == courseId);

                if (assignment != null)
                {
                    _context.FacultyCourseAssignments.Remove(assignment);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Faculty removed from course {CourseName} by {User}",
                        assignment.Course?.Name, User.Identity?.Name);

                    TempData["Success"] = "Faculty removed from course successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing faculty assignment for Faculty {FacultyId}, Course {CourseId}", facultyId, courseId);
                TempData["Error"] = "An error occurred while removing the assignment.";
            }

            return RedirectToAction(nameof(EditFaculty), new { id = facultyId });
        }

        // ==================== EXAM RESULTS RELEASE ====================
        public async Task<IActionResult> ExamResults()
        {
            var exams = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Results)
                .ToListAsync();
            return View(exams);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleExamRelease(int examId)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(examId);
                if (exam != null)
                {
                    exam.ResultsReleased = !exam.ResultsReleased;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Exam {ExamId} results release toggled to {Status} by {User}",
                        examId, exam.ResultsReleased, User.Identity?.Name);
                }
                return RedirectToAction(nameof(ExamResults));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling exam release for {ExamId}", examId);
                TempData["Error"] = "An error occurred while toggling exam results.";
                return RedirectToAction(nameof(ExamResults));
            }
        }

        // ==================== COURSE MANAGEMENT FROM BRANCH ====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourseFromBranch(int branchId, string courseName, DateTime startDate, DateTime endDate, List<int> selectedFaculty)
        {
            try
            {
                var branch = await _context.Branches.FindAsync(branchId);
                if (branch == null) return NotFound();

                if (string.IsNullOrWhiteSpace(courseName))
                {
                    TempData["Error"] = "Course name is required.";
                    return RedirectToAction(nameof(EditBranch), new { id = branchId });
                }

                // Validate faculty assignment
                if (selectedFaculty == null || !selectedFaculty.Any())
                {
                    TempData["Error"] = "At least one faculty member must be assigned to this course.";
                    ViewBag.FacultyList = new SelectList(_context.FacultyProfiles, "Id", "Name");
                    return RedirectToAction(nameof(EditBranch), new { id = branchId });
                }

                var course = new Course
                {
                    Name = courseName.Trim(),
                    BranchId = branchId,
                    StartDate = startDate,
                    EndDate = endDate
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                // Assign faculty to the course
                foreach (var facultyId in selectedFaculty)
                {
                    var assignment = new FacultyCourseAssignment
                    {
                        FacultyProfileId = facultyId,
                        CourseId = course.Id
                    };
                    _context.FacultyCourseAssignments.Add(assignment);
                }
                await _context.SaveChangesAsync();

                var facultyNames = string.Join(", ", selectedFaculty.Select(async id =>
                {
                    var faculty = await _context.FacultyProfiles.FindAsync(id);
                    return faculty?.Name ?? "Unknown";
                }).Select(t => t.Result));

                _logger.LogInformation("Course {CourseName} created at branch {BranchName} with faculty {FacultyNames} by {User}",
                    course.Name, branch.Name, facultyNames, User.Identity?.Name);

                TempData["Success"] = $"Course '{course.Name}' created successfully at {branch.Name} with {selectedFaculty.Count} faculty assigned.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course at branch {BranchId}", branchId);
                TempData["Error"] = "An error occurred while creating the course.";
            }

            return RedirectToAction(nameof(EditBranch), new { id = branchId });
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;

namespace VgcCollege.Web.Data
{
    public static class SeedData
    {
        public static async Task InitialiseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Seed Roles
            string[] roles = { "Admin", "Faculty", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Users
            var adminUser = await CreateUser(userManager, "admin@vgc.com", "Nathy19*", "Admin");
            var facultyUser1 = await CreateUser(userManager, "wenhao.fu@vgc.com", "Nathy19*", "Faculty");
            var facultyUser2 = await CreateUser(userManager, "rod.haanappel@vgc.com", "Nathy19*", "Faculty");
            var facultyUser3 = await CreateUser(userManager, "john.rowley@vgc.com", "Nathy19*", "Faculty");
            var studentUser1 = await CreateUser(userManager, "nathy@vgc.com", "Nathy19*", "Student");
            var studentUser2 = await CreateUser(userManager, "helder@vgc.com", "Nathy19*", "Student");
            var studentUser3 = await CreateUser(userManager, "matheus@vgc.com", "Nathy19**", "Student");
            var studentUser4 = await CreateUser(userManager, "wesley.oliveira@vgc.com", "Nathy19*", "Student");
            var studentUser5 = await CreateUser(userManager, "larissa.sousa@vgc.com", "Nathy19*", "Student");
            var studentUser6 = await CreateUser(userManager, "carlos.camargo@vgc.com", "Nathy19*", "Student");
            var studentUser7 = await CreateUser(userManager, "amanda@vgc.com", "Nathy19*", "Student");
            var studentUser8 = await CreateUser(userManager, "caio.pereira@vgc.com", "Nathy19*", "Student");

            // Seed Branches
            if (!context.Branches.Any())
            {
                context.Branches.AddRange(
                    new Branch { Name = "Dublin Campus", Address = "1 O'Connell St, Dublin" },
                    new Branch { Name = "Cork Campus", Address = "12 Patrick St, Cork" },
                    new Branch { Name = "Galway Campus", Address = "5 Shop St, Galway" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Courses
            if (!context.Courses.Any())
            {
                context.Courses.AddRange(
                    new Course { Name = "Programming Fundamentals", BranchId = 1, StartDate = DateTime.Now.AddMonths(-3), EndDate = DateTime.Now.AddMonths(9) },
                    new Course { Name = "Database Systems", BranchId = 1, StartDate = DateTime.Now.AddMonths(-3), EndDate = DateTime.Now.AddMonths(9) },
                    new Course { Name = "Operating Systems", BranchId = 2, StartDate = DateTime.Now.AddMonths(-2), EndDate = DateTime.Now.AddMonths(10) },
                    new Course { Name = "Artificial Intelligence", BranchId = 2, StartDate = DateTime.Now.AddMonths(-2), EndDate = DateTime.Now.AddMonths(10) },
                    new Course { Name = "Cloud Computing", BranchId = 3, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(11) },
                    new Course { Name = "Cybersecurity", BranchId = 3, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(11) }
                );
                await context.SaveChangesAsync();
            }

            // Seed Faculty Profiles
            if (!context.FacultyProfiles.Any())
            {
                context.FacultyProfiles.AddRange(
                    new FacultyProfile { IdentityUserId = facultyUser1!.Id, Name = "Dr. Wenhao Fu", Email = "wenhao.fu@vgc.com", Phone = "0851112233" },
                    new FacultyProfile { IdentityUserId = facultyUser2!.Id, Name = "Dr. Rod Haanappel", Email = "rod.haanappel@vgc.com", Phone = "0854445566" },
                    new FacultyProfile { IdentityUserId = facultyUser3!.Id, Name = "Dr. John Rowley", Email = "john.rowley@vgc.com", Phone = "0857778899" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Faculty Course Assignments
            if (!context.FacultyCourseAssignments.Any())
            {
                context.FacultyCourseAssignments.AddRange(
                    new FacultyCourseAssignment { FacultyProfileId = 1, CourseId = 1 },
                    new FacultyCourseAssignment { FacultyProfileId = 1, CourseId = 2 },
                    new FacultyCourseAssignment { FacultyProfileId = 2, CourseId = 3 },
                    new FacultyCourseAssignment { FacultyProfileId = 2, CourseId = 4 },
                    new FacultyCourseAssignment { FacultyProfileId = 3, CourseId = 5 },
                    new FacultyCourseAssignment { FacultyProfileId = 3, CourseId = 6 }
                );
                await context.SaveChangesAsync();
            }

            // Seed Student Profiles
            if (!context.StudentProfiles.Any())
            {
                context.StudentProfiles.AddRange(
                    new StudentProfile { IdentityUserId = studentUser1!.Id, Name = "Nathalia Macedo", Email = "nathy@vgc.com", Phone = "0861234567", Address = "10 Main St, Dublin", DateOfBirth = new DateTime(2000, 11, 5), StudentNumber = "VGC001" },
                    new StudentProfile { IdentityUserId = studentUser2!.Id, Name = "Amanda Gabriela", Email = "amada@vgc.com", Phone = "0867654321", Address = "22 High St, Cork", DateOfBirth = new DateTime(1999, 8, 20), StudentNumber = "VGC002" },
                    new StudentProfile { IdentityUserId = studentUser3!.Id, Name = "Helder Oliveira", Email = "helder@vgc.com", Phone = "0869876543", Address = "5 West St, Galway", DateOfBirth = new DateTime(2000, 3, 10), StudentNumber = "VGC003" },
                    new StudentProfile { IdentityUserId = studentUser4!.Id, Name = "Wesley Oliveira", Email = "wesley.oliveira@vgc.com", Phone = "0861122334", Address = "15 Park Ave, Dublin", DateOfBirth = new DateTime(2001, 6, 15), StudentNumber = "VGC004" },
                    new StudentProfile { IdentityUserId = studentUser5!.Id, Name = "Larissa Sousa", Email = "larissa.sousa@vgc.com", Phone = "0865566778", Address = "8 Church St, Cork", DateOfBirth = new DateTime(2002, 2, 22), StudentNumber = "VGC005" },
                    new StudentProfile { IdentityUserId = studentUser6!.Id, Name = "Carlos Camargo", Email = "carlos.camargo@vgc.com", Phone = "0869988776", Address = "42 Bridge St, Galway", DateOfBirth = new DateTime(2000, 9, 8), StudentNumber = "VGC006" },
                    new StudentProfile { IdentityUserId = studentUser7!.Id, Name = "Luara Froes", Email = "luara@vgc.com", Phone = "0864433221", Address = "7 College Rd, Dublin", DateOfBirth = new DateTime(2001, 12, 3), StudentNumber = "VGC007" },
                    new StudentProfile { IdentityUserId = studentUser8!.Id, Name = "Caio Pereira", Email = "caio.pereira@vgc.com", Phone = "0868877665", Address = "23 Harbour St, Cork", DateOfBirth = new DateTime(2002, 5, 17), StudentNumber = "VGC008" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Enrolments — every student in at least 3-4 courses
            if (!context.CourseEnrolments.Any())
            {
                context.CourseEnrolments.AddRange(
                    // Luara (1) — Programming, Database, AI, Cloud
                    new CourseEnrolment { StudentProfileId = 1, CourseId = 1, EnrolDate = DateTime.Now.AddMonths(-3), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 1, CourseId = 2, EnrolDate = DateTime.Now.AddMonths(-3), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 1, CourseId = 3, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 1, CourseId = 4, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 1, CourseId = 5, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },

                    // Bob (2) — OS, AI, Cybersecurity, Cloud
                    new CourseEnrolment { StudentProfileId = 2, CourseId = 3, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 2, CourseId = 4, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 2, CourseId = 6, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 2, CourseId = 5, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },

                    // Helder (3) — Cloud, Cybersecurity, Programming, OS
                    new CourseEnrolment { StudentProfileId = 3, CourseId = 5, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 3, CourseId = 6, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 3, CourseId = 1, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 3, CourseId = 3, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },

                    // Wesley (4) — Programming, AI, Database, Cybersecurity
                    new CourseEnrolment { StudentProfileId = 4, CourseId = 1, EnrolDate = DateTime.Now.AddMonths(-3), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 4, CourseId = 4, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 4, CourseId = 2, EnrolDate = DateTime.Now.AddMonths(-3), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 4, CourseId = 6, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },

                    // Larissa (5) — Database, Cloud, Programming, AI
                    new CourseEnrolment { StudentProfileId = 5, CourseId = 2, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 5, CourseId = 5, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 5, CourseId = 1, EnrolDate = DateTime.Now.AddMonths(-3), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 5, CourseId = 4, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },

                    // Carlos (6) — OS, Cybersecurity, Cloud, Database
                    new CourseEnrolment { StudentProfileId = 6, CourseId = 3, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 6, CourseId = 6, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 6, CourseId = 5, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 6, CourseId = 2, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },

                    // Manar (7) — Programming, Database, AI, OS, Cloud
                    new CourseEnrolment { StudentProfileId = 7, CourseId = 1, EnrolDate = DateTime.Now.AddMonths(-3), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 7, CourseId = 2, EnrolDate = DateTime.Now.AddMonths(-3), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 7, CourseId = 4, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 7, CourseId = 3, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 7, CourseId = 5, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },

                    // Caio (8) — OS, Cloud, Cybersecurity, Programming
                    new CourseEnrolment { StudentProfileId = 8, CourseId = 3, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 8, CourseId = 5, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 8, CourseId = 6, EnrolDate = DateTime.Now.AddMonths(-1), Status = "Active" },
                    new CourseEnrolment { StudentProfileId = 8, CourseId = 1, EnrolDate = DateTime.Now.AddMonths(-2), Status = "Active" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Attendance Records — 8 weeks per enrolment
            if (!context.AttendanceRecords.Any())
            {
                var attendance = new List<AttendanceRecord>();
                var random = new Random(42); // fixed seed for consistency

                var totalEnrolments = await context.CourseEnrolments.CountAsync();

                for (int enrolmentId = 1; enrolmentId <= totalEnrolments; enrolmentId++)
                {
                    for (int week = 1; week <= 8; week++)
                    {
                        attendance.Add(new AttendanceRecord
                        {
                            CourseEnrolmentId = enrolmentId,
                            WeekNumber = week,
                            Date = DateTime.Now.AddDays(-(56 - (week * 7))),
                            Present = random.Next(0, 10) > 2 // ~70-80% attendance
                        });
                    }
                }
                context.AttendanceRecords.AddRange(attendance);
                await context.SaveChangesAsync();
            }

            // Seed Assignments
            if (!context.Assignments.Any())
            {
                context.Assignments.AddRange(
                    new Assignment { CourseId = 1, Title = "Console App Project", MaxScore = 100, DueDate = DateTime.Now.AddDays(-30) },
                    new Assignment { CourseId = 1, Title = "OOP Final Project", MaxScore = 100, DueDate = DateTime.Now.AddDays(-10) },
                    new Assignment { CourseId = 2, Title = "SQL Query Assignment", MaxScore = 100, DueDate = DateTime.Now.AddDays(-25) },
                    new Assignment { CourseId = 2, Title = "Database Design Project", MaxScore = 100, DueDate = DateTime.Now.AddDays(-5) },
                    new Assignment { CourseId = 3, Title = "Process Scheduling Simulator", MaxScore = 100, DueDate = DateTime.Now.AddDays(-20) },
                    new Assignment { CourseId = 3, Title = "Memory Management Report", MaxScore = 100, DueDate = DateTime.Now.AddDays(-8) },
                    new Assignment { CourseId = 4, Title = "Search Algorithms Implementation", MaxScore = 100, DueDate = DateTime.Now.AddDays(-15) },
                    new Assignment { CourseId = 4, Title = "Neural Network Project", MaxScore = 100, DueDate = DateTime.Now.AddDays(-3) },
                    new Assignment { CourseId = 5, Title = "AWS Architecture Design", MaxScore = 100, DueDate = DateTime.Now.AddDays(-18) },
                    new Assignment { CourseId = 5, Title = "Serverless Application", MaxScore = 100, DueDate = DateTime.Now.AddDays(-2) },
                    new Assignment { CourseId = 6, Title = "Security Audit Report", MaxScore = 100, DueDate = DateTime.Now.AddDays(-12) },
                    new Assignment { CourseId = 6, Title = "Penetration Testing Lab", MaxScore = 100, DueDate = DateTime.Now.AddDays(-1) }
                );
                await context.SaveChangesAsync();
            }

            // Seed Assignment Results — results for every enrolled student
            if (!context.AssignmentResults.Any())
            {
                context.AssignmentResults.AddRange(
                    // Luara (1) — enrolled in courses 1, 2, 4, 5
                    new AssignmentResult { AssignmentId = 1, StudentProfileId = 1, Score = 92, Feedback = "Excellent work! Great understanding of C# fundamentals." },
                    new AssignmentResult { AssignmentId = 2, StudentProfileId = 1, Score = 88, Feedback = "Very solid OOP design. Consider adding more comments." },
                    new AssignmentResult { AssignmentId = 3, StudentProfileId = 1, Score = 95, Feedback = "Outstanding SQL queries. Perfect optimization!" },
                    new AssignmentResult { AssignmentId = 4, StudentProfileId = 1, Score = 91, Feedback = "Excellent database schema design." },
                    new AssignmentResult { AssignmentId = 5, StudentProfileId = 1, Score = 97, Feedback = "Fantastic process scheduling simulator!" },
                    new AssignmentResult { AssignmentId = 6, StudentProfileId = 1, Score = 95, Feedback = "Exceptional memory management report. Very detailed and well explained." },
                    new AssignmentResult { AssignmentId = 7, StudentProfileId = 1, Score = 92, Feedback = "Very good implementation of search algorithms. Consider adding more test cases." },
                    new AssignmentResult { AssignmentId = 9, StudentProfileId = 1, Score = 93, Feedback = "Great cloud architecture!" },

                    // Bob (2) — enrolled in courses 3, 4, 5, 6
                    new AssignmentResult { AssignmentId = 5, StudentProfileId = 2, Score = 72, Feedback = "Good effort, but process scheduling needs improvement." },
                    new AssignmentResult { AssignmentId = 6, StudentProfileId = 2, Score = 68, Feedback = "Memory management report needs more detail." },
                    new AssignmentResult { AssignmentId = 7, StudentProfileId = 2, Score = 65, Feedback = "Search algorithms implemented but optimization needed." },
                    new AssignmentResult { AssignmentId = 9, StudentProfileId = 2, Score = 70, Feedback = "Decent AWS architecture." },
                    new AssignmentResult { AssignmentId = 11, StudentProfileId = 2, Score = 74, Feedback = "Good security audit attempt." },

                    // Helder (3) — enrolled in courses 1, 3, 5, 6
                    new AssignmentResult { AssignmentId = 1, StudentProfileId = 3, Score = 78, Feedback = "Good console app but missing error handling." },
                    new AssignmentResult { AssignmentId = 5, StudentProfileId = 3, Score = 82, Feedback = "Solid process scheduling work." },
                    new AssignmentResult { AssignmentId = 9, StudentProfileId = 3, Score = 85, Feedback = "Well designed AWS architecture." },
                    new AssignmentResult { AssignmentId = 10, StudentProfileId = 3, Score = 79, Feedback = "Serverless app mostly works." },
                    new AssignmentResult { AssignmentId = 11, StudentProfileId = 3, Score = 45, Feedback = "Security audit lacks depth. Please review OWASP guidelines." },
                    new AssignmentResult { AssignmentId = 12, StudentProfileId = 3, Score = 62, Feedback = "Penetration testing needs improvement." },

                    // Wesley (4) — enrolled in courses 1, 2, 4, 6
                    new AssignmentResult { AssignmentId = 1, StudentProfileId = 4, Score = 96, Feedback = "Exceptional! Best in class." },
                    new AssignmentResult { AssignmentId = 2, StudentProfileId = 4, Score = 94, Feedback = "Outstanding OOP implementation." },
                    new AssignmentResult { AssignmentId = 3, StudentProfileId = 4, Score = 92, Feedback = "Excellent SQL work!" },
                    new AssignmentResult { AssignmentId = 7, StudentProfileId = 4, Score = 91, Feedback = "Great AI algorithms." },
                    new AssignmentResult { AssignmentId = 8, StudentProfileId = 4, Score = 94, Feedback = "Neural network implementation is impressive!" },
                    new AssignmentResult { AssignmentId = 11, StudentProfileId = 4, Score = 88, Feedback = "Very thorough security audit." },

                    // Larissa (5) — enrolled in courses 1, 2, 4, 5
                    new AssignmentResult { AssignmentId = 1, StudentProfileId = 5, Score = 84, Feedback = "Good programming foundations." },
                    new AssignmentResult { AssignmentId = 3, StudentProfileId = 5, Score = 90, Feedback = "Excellent SQL queries!" },
                    new AssignmentResult { AssignmentId = 4, StudentProfileId = 5, Score = 91, Feedback = "Beautiful database design. Normalization is perfect." },
                    new AssignmentResult { AssignmentId = 7, StudentProfileId = 5, Score = 86, Feedback = "Good AI implementation." },
                    new AssignmentResult { AssignmentId = 9, StudentProfileId = 5, Score = 88, Feedback = "Solid cloud architecture." },
                    new AssignmentResult { AssignmentId = 10, StudentProfileId = 5, Score = 89, Feedback = "Serverless app works flawlessly." },

                    // Carlos (6) — enrolled in courses 2, 3, 5, 6
                    new AssignmentResult { AssignmentId = 3, StudentProfileId = 6, Score = 71, Feedback = "SQL needs more work on joins." },
                    new AssignmentResult { AssignmentId = 5, StudentProfileId = 6, Score = 75, Feedback = "Good process scheduling." },
                    new AssignmentResult { AssignmentId = 6, StudentProfileId = 6, Score = 74, Feedback = "Memory management report is decent." },
                    new AssignmentResult { AssignmentId = 9, StudentProfileId = 6, Score = 69, Feedback = "Cloud architecture needs improvement." },
                    new AssignmentResult { AssignmentId = 11, StudentProfileId = 6, Score = 80, Feedback = "Good security audit." },
                    new AssignmentResult { AssignmentId = 12, StudentProfileId = 6, Score = 78, Feedback = "Good penetration testing attempt." },

                    // Manar (7) — enrolled in courses 1, 2, 3, 4, 5
                    new AssignmentResult { AssignmentId = 1, StudentProfileId = 7, Score = 98, Feedback = "Outstanding! Perfect execution." },
                    new AssignmentResult { AssignmentId = 2, StudentProfileId = 7, Score = 97, Feedback = "Exceptional OOP design." },
                    new AssignmentResult { AssignmentId = 3, StudentProfileId = 7, Score = 96, Feedback = "SQL mastery demonstrated!" },
                    new AssignmentResult { AssignmentId = 4, StudentProfileId = 7, Score = 95, Feedback = "Perfect database schema." },
                    new AssignmentResult { AssignmentId = 5, StudentProfileId = 7, Score = 93, Feedback = "Excellent OS work." },
                    new AssignmentResult { AssignmentId = 7, StudentProfileId = 7, Score = 97, Feedback = "AI algorithms implemented brilliantly." },
                    new AssignmentResult { AssignmentId = 8, StudentProfileId = 7, Score = 98, Feedback = "Best neural network in class!" },
                    new AssignmentResult { AssignmentId = 9, StudentProfileId = 7, Score = 96, Feedback = "Exceptional cloud architecture." },

                    // Caio (8) — enrolled in courses 1, 3, 5, 6
                    new AssignmentResult { AssignmentId = 1, StudentProfileId = 8, Score = 76, Feedback = "Good console app effort." },
                    new AssignmentResult { AssignmentId = 5, StudentProfileId = 8, Score = 82, Feedback = "Good work on the scheduler." },
                    new AssignmentResult { AssignmentId = 6, StudentProfileId = 8, Score = 79, Feedback = "Decent memory management report." },
                    new AssignmentResult { AssignmentId = 9, StudentProfileId = 8, Score = 71, Feedback = "Cloud architecture is basic but correct." },
                    new AssignmentResult { AssignmentId = 10, StudentProfileId = 8, Score = 55, Feedback = "Serverless app needs revision. Function not triggering correctly." },
                    new AssignmentResult { AssignmentId = 11, StudentProfileId = 8, Score = 68, Feedback = "Security audit needs more detail." }
                );
                await context.SaveChangesAsync();
            }

            // Seed Exams
            if (!context.Exams.Any())
            {
                context.Exams.AddRange(
                    new Exam { CourseId = 1, Title = "Programming Final Exam", Date = DateTime.Now.AddDays(-18), MaxScore = 100, ResultsReleased = true },
                    new Exam { CourseId = 2, Title = "Database Midterm", Date = DateTime.Now.AddDays(-22), MaxScore = 100, ResultsReleased = true },
                    new Exam { CourseId = 3, Title = "OS Final Exam", Date = DateTime.Now.AddDays(-12), MaxScore = 100, ResultsReleased = false },
                    new Exam { CourseId = 4, Title = "AI Comprehensive Exam", Date = DateTime.Now.AddDays(-8), MaxScore = 100, ResultsReleased = false },
                    new Exam { CourseId = 5, Title = "Cloud Certification Prep", Date = DateTime.Now.AddDays(-5), MaxScore = 100, ResultsReleased = false },
                    new Exam { CourseId = 6, Title = "Security Exam", Date = DateTime.Now.AddDays(-3), MaxScore = 100, ResultsReleased = false }
                );
                await context.SaveChangesAsync();
            }

            // Seed Exam Results — all students who are enrolled
            if (!context.ExamResults.Any())
            {
                context.ExamResults.AddRange(
                    // Exam 1 — Programming (Released) — Nathalia, Helder, Wesley, Larissa, Luara, Caio enrolled
                    new ExamResult { ExamId = 1, StudentProfileId = 1, Score = 89, Grade = "B+" },
                    new ExamResult { ExamId = 1, StudentProfileId = 3, Score = 75, Grade = "C+" },
                    new ExamResult { ExamId = 1, StudentProfileId = 4, Score = 95, Grade = "A" },
                    new ExamResult { ExamId = 1, StudentProfileId = 5, Score = 81, Grade = "B" },
                    new ExamResult { ExamId = 1, StudentProfileId = 7, Score = 97, Grade = "A+" },
                    new ExamResult { ExamId = 1, StudentProfileId = 8, Score = 73, Grade = "C+" },

                    // Exam 2 — Database (Released) — Nathalia, Wesley, Larissa, Carlos, Amanda enrolled
                    new ExamResult { ExamId = 2, StudentProfileId = 1, Score = 92, Grade = "A-" },
                    new ExamResult { ExamId = 2, StudentProfileId = 4, Score = 90, Grade = "A-" },
                    new ExamResult { ExamId = 2, StudentProfileId = 5, Score = 88, Grade = "B+" },
                    new ExamResult { ExamId = 2, StudentProfileId = 6, Score = 70, Grade = "C+" },
                    new ExamResult { ExamId = 2, StudentProfileId = 7, Score = 94, Grade = "A" },

                    // Exam 3 — OS (Hidden) — Amanda, Helder, Carlos, Luara, Caio enrolled
                    new ExamResult { ExamId = 3, StudentProfileId = 2, Score = 71, Grade = "C+" },
                    new ExamResult { ExamId = 3, StudentProfileId = 3, Score = 79, Grade = "B-" },
                    new ExamResult { ExamId = 3, StudentProfileId = 6, Score = 68, Grade = "C" },
                    new ExamResult { ExamId = 3, StudentProfileId = 7, Score = 91, Grade = "A-" },
                    new ExamResult { ExamId = 3, StudentProfileId = 8, Score = 77, Grade = "B-" },

                    // Exam 4 — AI (Hidden) — Luara, Nathalia, Wesley, Larissa, Amanda enrolled
                    new ExamResult { ExamId = 4, StudentProfileId = 1, Score = 84, Grade = "B" },
                    new ExamResult { ExamId = 4, StudentProfileId = 2, Score = 65, Grade = "C" },
                    new ExamResult { ExamId = 4, StudentProfileId = 4, Score = 88, Grade = "B+" },
                    new ExamResult { ExamId = 4, StudentProfileId = 5, Score = 83, Grade = "B" },
                    new ExamResult { ExamId = 4, StudentProfileId = 7, Score = 96, Grade = "A" },

                    // Exam 5 — Cloud (Hidden) — Luara, Nathalia, Helder, Larissa, Carlos, Amanda, Caio enrolled
                    new ExamResult { ExamId = 5, StudentProfileId = 1, Score = 86, Grade = "B+" },
                    new ExamResult { ExamId = 5, StudentProfileId = 2, Score = 70, Grade = "C+" },
                    new ExamResult { ExamId = 5, StudentProfileId = 3, Score = 82, Grade = "B" },
                    new ExamResult { ExamId = 5, StudentProfileId = 5, Score = 91, Grade = "A-" },
                    new ExamResult { ExamId = 5, StudentProfileId = 6, Score = 67, Grade = "C" },
                    new ExamResult { ExamId = 5, StudentProfileId = 7, Score = 93, Grade = "A" },
                    new ExamResult { ExamId = 5, StudentProfileId = 8, Score = 54, Grade = "F" },

                    // Exam 6 — Cybersecurity (Hidden) — Nathalia, Helder, Wesley, Carlos, Caio enrolled
                    new ExamResult { ExamId = 6, StudentProfileId = 2, Score = 76, Grade = "C+" },
                    new ExamResult { ExamId = 6, StudentProfileId = 3, Score = 71, Grade = "C+" },
                    new ExamResult { ExamId = 6, StudentProfileId = 4, Score = 85, Grade = "B+" },
                    new ExamResult { ExamId = 6, StudentProfileId = 6, Score = 82, Grade = "B" },
                    new ExamResult { ExamId = 6, StudentProfileId = 8, Score = 48, Grade = "F" }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task<IdentityUser?> CreateUser(
            UserManager<IdentityUser> userManager,
            string email, string password, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) return existing;

            var user = new IdentityUser { UserName = email, Email = email };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                return user;
            }
            return null;
        }
    }
}
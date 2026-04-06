VGC COLLEGE MANAGEMENT SYSTEM - README.md
===========================================

VGC College Management System

A comprehensive academic management system for Acme Global College (VGC), a multi-branch college in Ireland. Built with ASP.NET Core MVC, Entity Framework Core, and SQLite.

OVERVIEW

This system replaces paper-based processes with a modern web application that supports:
- Student registration and enrolment
- Attendance tracking
- Assignment and exam gradebook management
- Role-based access control (Admin, Faculty, Student)
- Exam results release control

TECHNOLOGIES USED

- Backend: ASP.NET Core MVC 8.0
- Database: SQLite with Entity Framework Core
- Authentication: ASP.NET Core Identity
- Logging: Serilog with Seq sink
- Frontend: Bootstrap 5, Bootstrap Icons, Chart.js
- Testing: xUnit with InMemory database
- CI/CD: GitHub Actions

PREREQUISITES

- .NET 8.0 SDK
- SQLite (optional - included via NuGet)
- Seq (optional - for structured logging UI)

SETUP INSTRUCTIONS

1. Clone the Repository

git clone https://github.com/Luarafroes/oop-s2-3-mvc-78337.git
cd oop-s2-3-mvc-78337

2. Restore Dependencies

dotnet restore

3. Apply Database Migrations

cd src/VgcCollege.Web
dotnet ef database update

4. Run the Application

dotnet run

The application will be available at https://localhost:5001 or http://localhost:5000

5. Run Tests

dotnet test

SEEDED DEMO ACCOUNTS

Role          | Email                      | Password
--------------|----------------------------|---------------
Admin         | admin@vgc.com              | Letmein01*
Faculty       | john.rowley@vgc.com        | Letmein01*
Student       | luara@vgc.com              | Letmein01*

Additional Faculty Accounts:

Name                 | Email                         | Password
---------------------|-------------------------------|---------------
Dr. Wenhao Fu        | wenhao.fu@vgc.com             | Letmein01*
Dr. Rod Haanappel    | rod.haanappel@vgc.com         | Letmein01*

Additional Student Accounts:

Name                | Email                         | Password
--------------------|-------------------------------|---------------
Bob Walsh           | bob@vgc.com                   | Letmein01*
Helder Oliveira     | helder@vgc.com                | Letmein01*
Wesley Oliveira     | wesley.oliveira@vgc.com       | Letmein01*
Larissa Sousa       | larissa.sousa@vgc.com         | Letmein01*
Carlos Camargo      | carlos.camargo@vgc.com        | Letmein01*
Manar Ahkim         | manar.ahkim@vgc.com           | Letmein01*
Caio Pereira        | caio.pereira@vgc.com          | Letmein01*

FEATURES BY ROLE

ADMIN:
- Manage branches (Dublin, Cork, Galway campuses)
- Manage courses across all branches
- Manage student profiles and enrolments
- Manage faculty profiles and course assignments
- Release/hide exam results

FACULTY:
- View dashboard with course statistics
- View students enrolled in their courses
- Mark weekly attendance
- Create assignments
- Enter assignment and exam grades

STUDENT:
- View personal dashboard
- View enrolled courses with attendance rates
- View assignment results with feedback
- View exam results (only when released by admin)
- Update personal profile

DATABASE SCHEMA

The system includes 11 entities:

Entity                  | Description
------------------------|--------------------------------------------------
Branch                  | College campuses (Dublin, Cork, Galway)
Course                  | Academic courses offered at branches
StudentProfile          | Student information linked to Identity
FacultyProfile          | Faculty information linked to Identity
FacultyCourseAssignment | Many-to-many: Faculty to Courses
CourseEnrolment         | Students enrolled in courses
AttendanceRecord        | Weekly attendance tracking
Assignment              | Course assignments
AssignmentResult        | Student grades for assignments
Exam                    | Course exams
ExamResult              | Student grades for exams (with release control)

TESTING

Test Coverage (10 tests):

Test 1: Student can only see their own enrolments
Test 2: Student cannot see another student's data
Test 3: Faculty can only see students in their courses
Test 4: Exam results are hidden until released
Test 5: Assignment grade calculation
Test 6: Course enrollment prevents duplicates
Test 7: Attendance percentage calculation
Test 8: Course cannot be deleted with enrolled students
Test 9: Faculty can only access assigned courses
Test 10: Student gradebook shows only released exams

Run Tests:

dotnet test

DESIGN DECISIONS

1. SQLite over SQL Server: Chosen for simplicity and zero configuration for local development.

2. Separate Domain Project: Clean architecture with Domain, Web, and Tests projects for separation of concerns.

3. InMemory Database for Tests: Ensures tests are isolated, fast, and don't affect development data.

4. Serilog with Seq: Structured logging enables powerful log querying (e.g., filtering by User, Level, or custom properties).

5. Bootstrap 5 + Icons: Professional, responsive UI without complex custom CSS.

6. Role-Based Authorization: Server-side enforcement using [Authorize(Roles = "...")] attributes.

PROJECT STRUCTURE

oop-s2-3-mvc-78337/
├── src/
│   ├── VgcCollege.Domain/          # Entities
│   └── VgcCollege.Web/             # MVC Application
│       ├── Controllers/
│       ├── Views/
│       ├── Data/
│       ├── ViewModels/
│       └── Program.cs
├── tests/
│   └── VgcCollege.Tests/           # xUnit Tests
│       ├── Helpers/
│       └── VgcCollegeTests.cs
├── .github/workflows/
│   └── ci.yml                      # GitHub Actions CI
└── README.md

LOGGING WITH SEQ

The application uses Serilog for structured logging. To view logs in Seq:

1. Install Seq from datalust.co/seq
2. Run Seq locally (default: http://localhost:5341)
3. Run the application
4. Open http://localhost:5341 to search and filter logs

Example queries:
- Level = 'Warning' - Show all warnings
- User = 'admin@vgc.com' - Show admin actions
- @Level = 'Error' - Show errors

GITHUB ACTIONS CI

The CI workflow runs on every push to main:
- Restores dependencies
- Builds in Release configuration
- Runs all xUnit tests
- Fails if build or tests fail

CONTACT

For questions about this project, please contact your module instructor.

ACKNOWLEDGMENTS

- Acme Global College for the project requirements
- Bootstrap for the UI framework
- Serilog team for structured logging
- Chart.js for data visualization
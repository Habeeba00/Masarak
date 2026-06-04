using System.Security.Claims;
using Masarak.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masarak.Controllers
{
    // ─── Admin Controller ─────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult Dashboard() =>
            Ok(new { message = "Admin dashboard — restricted to Admins only.", user = GetUserInfo() });

        [HttpGet("users")]
        public IActionResult GetAllUsers() =>
            Ok(new { message = "User list — Admin only.", user = GetUserInfo() });

        [HttpDelete("users/{id}")]
        public IActionResult DeleteUser(int id) =>
            Ok(new { message = $"User {id} deleted by Admin.", user = GetUserInfo() });

        private object GetUserInfo() => new
        {
            UserId = User.FindFirstValue("userid"),
            Email = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email),
            Role = User.FindFirstValue("role")
        };
    }

    // ─── Teacher Controller ───────────────────────────────────────────────────────
    [ApiController]
    [Route("api/teacher")]
    [Authorize(Policy = AppPolicies.TeacherOnly)]
    [Produces("application/json")]
    public class TeacherController : ControllerBase
    {
        [HttpGet("classes")]
        public IActionResult GetMyClasses() =>
            Ok(new { message = "My classes — restricted to Teachers.", user = GetUserInfo() });

        [HttpPost("sessions")]
        public IActionResult CreateSession() =>
            Ok(new { message = "Session created — Teacher access.", user = GetUserInfo() });

        [HttpGet("assignments")]
        public IActionResult GetAssignments() =>
            Ok(new { message = "Assignments — Teacher access.", user = GetUserInfo() });

        // Admins and Teachers can both grade
        [HttpPost("grade/{submissionId}")]
        [Authorize(Policy = AppPolicies.AdminOrTeacher)]
        public IActionResult GradeSubmission(int submissionId, [FromBody] decimal score) =>
            Ok(new { message = $"Submission {submissionId} graded with {score}.", user = GetUserInfo() });

        private object GetUserInfo() => new
        {
            UserId = User.FindFirstValue("userid"),
            Email = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email),
            Role = User.FindFirstValue("role")
        };
    }

    // ─── Student Controller ───────────────────────────────────────────────────────
    [ApiController]
    [Route("api/student")]
    [Authorize(Policy = AppPolicies.StudentOnly)]
    [Produces("application/json")]
    public class StudentController : ControllerBase
    {
        [HttpGet("courses")]
        public IActionResult GetMyCourses() =>
            Ok(new { message = "My enrolled courses — Student only.", user = GetUserInfo() });

        [HttpGet("grades")]
        public IActionResult GetMyGrades() =>
            Ok(new { message = "My grades — Student only.", user = GetUserInfo() });

        [HttpPost("assignments/{id}/submit")]
        public IActionResult SubmitAssignment(int id) =>
            Ok(new { message = $"Assignment {id} submitted — Student only.", user = GetUserInfo() });

        [HttpGet("exams")]
        public IActionResult GetUpcomingExams() =>
            Ok(new { message = "Upcoming exams — Student only.", user = GetUserInfo() });

        private object GetUserInfo() => new
        {
            UserId = User.FindFirstValue("userid"),
            Email = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email),
            Role = User.FindFirstValue("role")
        };
    }

    // ─── Parent Controller ────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/parent")]
    [Authorize(Policy = AppPolicies.ParentOnly)]
    [Produces("application/json")]
    public class ParentController : ControllerBase
    {
        [HttpGet("children")]
        public IActionResult GetChildren() =>
            Ok(new { message = "My children — Parent only.", user = GetUserInfo() });

        [HttpGet("children/{childId}/grades")]
        public IActionResult GetChildGrades(int childId) =>
            Ok(new { message = $"Child {childId} grades — Parent only.", user = GetUserInfo() });

        [HttpGet("children/{childId}/attendance")]
        public IActionResult GetChildAttendance(int childId) =>
            Ok(new { message = $"Child {childId} attendance — Parent only.", user = GetUserInfo() });

        private object GetUserInfo() => new
        {
            UserId = User.FindFirstValue("userid"),
            Email = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email),
            Role = User.FindFirstValue("role")
        };
    }

    // ─── Mixed Access Controller ──────────────────────────────────────────────────
    [ApiController]
    [Route("api/shared")]
    [Authorize]
    [Produces("application/json")]
    public class SharedController : ControllerBase
    {
        // Students and Parents can both view a student's progress
        [HttpGet("progress/{studentId}")]
        [Authorize(Policy = AppPolicies.StudentOrParent)]
        public IActionResult GetProgress(int studentId) =>
            Ok(new { message = $"Progress for student {studentId} — Student or Parent.", user = GetUserInfo() });

        // Admins and Teachers can view analytics
        [HttpGet("analytics")]
        [Authorize(Policy = AppPolicies.AdminOrTeacher)]
        public IActionResult GetAnalytics() =>
            Ok(new { message = "Analytics dashboard — Admin or Teacher.", user = GetUserInfo() });

        // Any authenticated user can get their notifications
        [HttpGet("notifications")]
        public IActionResult GetNotifications() =>
            Ok(new { message = "Notifications — any authenticated user.", user = GetUserInfo() });

        private object GetUserInfo() => new
        {
            UserId = User.FindFirstValue("userid"),
            Email = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email),
            Role = User.FindFirstValue("role")
        };
    }
}

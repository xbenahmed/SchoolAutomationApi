using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolAutomationApi.Data;
using SchoolAutomationApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Added missing namespace
using Microsoft.Extensions.Logging;

namespace SchoolAutomationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")] // Restrict to teachers
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AttendanceController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanAttendance([FromBody] ScanRequest request)
        {
            try
            {
                // Get the current teacher from the authenticated user
                var email = User.FindFirst(ClaimTypes.Email)?.Value
                           ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("No email claim found for the authenticated teacher.");
                    return Unauthorized("Teacher email not found in token.");
                }

                var teacher = await _userManager.FindByEmailAsync(email);
                if (teacher == null)
                {
                    _logger.LogWarning("Teacher with email {Email} not found.", email);
                    return NotFound("Teacher not found.");
                }

                // Validate the scanned student ID
                if (request.StudentId <= 0)
                {
                    _logger.LogWarning("Invalid student ID: {StudentId}", request.StudentId);
                    return BadRequest("Invalid student ID.");
                }

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentId == request.StudentId);
                if (student == null)
                {
                    _logger.LogWarning("Student with ID {StudentId} not found.", request.StudentId);
                    return NotFound("Student not found.");
                }

                // Record attendance
                var attendance = new Attendance
                {
                    StudentId = request.StudentId,
                    TeacherId = teacher.Id,
                    AttendanceDate = DateTime.UtcNow,
                    IsPresent = true // Default to present; could be toggled
                };
                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Attendance recorded for student {StudentId} by teacher {TeacherId}.", request.StudentId, teacher.Id);
                return Ok(new { message = "Attendance recorded successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while recording attendance for student ID {StudentId}.", request?.StudentId);
                return StatusCode(500, "An error occurred while recording attendance.");
            }
        }
    }

    public class ScanRequest
    {
        public int StudentId { get; set; }
    }
}
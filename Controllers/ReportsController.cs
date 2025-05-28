using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAutomationApi.Data;
using Microsoft.EntityFrameworkCore;

namespace SchoolAutomationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{type}")]
        [Authorize(Roles = "Administrator,Teacher,Parent")]
        public async Task<IActionResult> GenerateReport(string type, [FromQuery] int? studentId, [FromQuery] int? courseId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            switch (type.ToLower())
            {
                case "attendance":
                    var attendanceQuery = _context.Attendances
                        .Include(a => a.Student)
                        .Include(a => a.Course)
                        .AsQueryable();

                    if (studentId.HasValue)
                        attendanceQuery = attendanceQuery.Where(a => a.StudentId == studentId.Value);

                    if (courseId.HasValue)
                        attendanceQuery = attendanceQuery.Where(a => a.CourseId == courseId.Value);

                    if (startDate.HasValue)
                        attendanceQuery = attendanceQuery.Where(a => a.Date >= startDate.Value);

                    if (endDate.HasValue)
                        attendanceQuery = attendanceQuery.Where(a => a.Date <= endDate.Value);

                    var attendance = await attendanceQuery
                        .Select(a => new
                        {
                            studentId = a.StudentId,
                            studentName = a.Student!.Name,
                            courseId = a.CourseId,
                            courseName = a.Course!.CourseName,
                            date = a.Date,
                            status = a.Status
                        })
                        .ToListAsync();

                    return Ok(attendance);

                case "grades":
                    var gradesQuery = _context.Grades
                        .Include(g => g.Student)
                        .Include(g => g.Course)
                        .AsQueryable();

                    if (studentId.HasValue)
                        gradesQuery = gradesQuery.Where(g => g.StudentId == studentId.Value);

                    if (courseId.HasValue)
                        gradesQuery = gradesQuery.Where(g => g.CourseId == courseId.Value);

                    var grades = await gradesQuery
                        .Select(g => new
                        {
                            studentId = g.StudentId,
                            studentName = g.Student!.Name,
                            courseId = g.CourseId,
                            courseName = g.Course!.CourseName,
                            gradeValue = g.GradeValue
                        })
                        .ToListAsync();

                    return Ok(grades);

                default:
                    return BadRequest("Invalid report type. Use 'attendance' or 'grades'.");
            }
        }
    }
}
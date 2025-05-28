using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAutomationApi.Data;
using SchoolAutomationApi.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolAutomationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GradesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddGrade([FromBody] GradeCreateDto gradeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == gradeDto.StudentId);

            if (student == null)
                return BadRequest("Student not found.");

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == gradeDto.CourseId);

            if (course == null)
                return BadRequest("Course not found.");

            var existingGrade = await _context.Grades
                .AnyAsync(g => g.StudentId == gradeDto.StudentId && g.CourseId == gradeDto.CourseId);

            if (existingGrade)
                return BadRequest("Grade already exists for this student and course.");

            var grade = new Grade
            {
                StudentId = gradeDto.StudentId,
                CourseId = gradeDto.CourseId,
                GradeValue = gradeDto.GradeValue
            };

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGrade), new { id = grade.GradeId }, new { id = grade.GradeId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrade(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Course)
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
                return NotFound();

            return Ok(new
            {
                id = grade.GradeId,
                studentId = grade.StudentId,
                studentName = grade.Student?.Name,
                courseId = grade.CourseId,
                courseName = grade.Course?.CourseName,
                gradeValue = grade.GradeValue
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] GradeUpdateDto gradeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grade = await _context.Grades
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
                return NotFound();

            grade.GradeValue = gradeDto.GradeValue;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.Grades
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
                return NotFound();

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class GradeCreateDto
    {
        public required int StudentId { get; set; }
        public required int CourseId { get; set; }
        public required string GradeValue { get; set; }
    }

    public class GradeUpdateDto
    {
        public required string GradeValue { get; set; }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolAutomationApi.Data;
using SchoolAutomationApi.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolAutomationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class TeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public TeachersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeacher([FromBody] TeacherCreateDto teacherDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate email
            if (await _userManager.FindByEmailAsync(teacherDto.ContactInfo) != null)
                return BadRequest("Email is already in use.");

            var defaultPassword = _configuration["DefaultPassword"] ?? "Default@123";
            var user = new ApplicationUser
            {
                Email = teacherDto.ContactInfo,
                UserName = teacherDto.ContactInfo,
                FullName = teacherDto.Name,
                ProfileInfo = $"Teacher - {teacherDto.Name}"
            };
            var result = await _userManager.CreateAsync(user, defaultPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var teacher = new Teacher
            {
                UserId = user.Id,
                Subject = teacherDto.Subject
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(user, "Teacher");

            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, new { id = teacher.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
                return NotFound();

            return Ok(new
            {
                id = teacher.Id,
                name = teacher.User?.FullName,
                subject = teacher.Subject,
                contactInfo = teacher.User?.Email
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] TeacherUpdateDto teacherDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
                return NotFound();

            // Check for duplicate email (excluding current user)
            var existingUser = await _userManager.FindByEmailAsync(teacherDto.ContactInfo);
            if (existingUser != null && existingUser.Id != teacher.UserId)
                return BadRequest("Email is already in use.");

            teacher.Subject = teacherDto.Subject;
            if (teacher.User != null)
            {
                teacher.User.FullName = teacherDto.Name;
                teacher.User.Email = teacherDto.ContactInfo;
                teacher.User.UserName = teacherDto.ContactInfo;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
                return NotFound();

            if (teacher.User != null)
            {
                await _userManager.DeleteAsync(teacher.User);
            }

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class TeacherCreateDto
    {
        public required string Name { get; set; }
        public required string ContactInfo { get; set; }
        public required string Subject { get; set; }
    }

    public class TeacherUpdateDto
    {
        public required string Name { get; set; }
        public required string ContactInfo { get; set; }
        public required string Subject { get; set; }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolAutomationApi.Data;
using SchoolAutomationApi.Models;
using QRCoder;
using Microsoft.EntityFrameworkCore;

namespace SchoolAutomationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto studentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate ParentId if provided
            if (studentDto.ParentId.HasValue)
            {
                var parentExists = await _context.Parents.AnyAsync(p => p.Id == studentDto.ParentId.Value);
                if (!parentExists)
                    return BadRequest($"Parent with ID {studentDto.ParentId.Value} not found.");
            }

            var defaultPassword = _configuration["DefaultPassword"] ?? "Default@123";
            var user = new ApplicationUser
            {
                Email = studentDto.ContactInfo,
                UserName = studentDto.ContactInfo,
                FullName = studentDto.Name,
                ProfileInfo = $"Student - {studentDto.Name}"
            };
            var result = await _userManager.CreateAsync(user, defaultPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign Student role
            await _userManager.AddToRoleAsync(user, "Student");

            var student = new Student
            {
                UserId = user.Id,
                Name = studentDto.Name,
                GradeLevel = studentDto.GradeLevel,
                ContactInfo = studentDto.ContactInfo,
                QRCode = GenerateQRCode(user.Id)
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            if (studentDto.ParentId.HasValue)
            {
                var parentRelation = new ParentsStudentRelation
                {
                    ParentId = studentDto.ParentId.Value,
                    StudentId = student.StudentId
                };
                _context.ParentsStudentRelations.Add(parentRelation);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetStudent), new { id = student.StudentId }, new { id = student.StudentId, qrCode = student.QRCode });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);
            if (student == null)
                return NotFound();

            return Ok(new
            {
                id = student.StudentId,
                name = student.Name,
                gradeLevel = student.GradeLevel,
                contactInfo = student.ContactInfo,
                qrCode = student.QRCode,
                email = student.User?.Email
            });
        }

        private string GenerateQRCode(string data)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }

    public class StudentCreateDto
    {
        public required string Name { get; set; }
        public required string GradeLevel { get; set; }
        public required string ContactInfo { get; set; }
        public int? ParentId { get; set; }
    }
}
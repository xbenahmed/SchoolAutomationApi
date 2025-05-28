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
    public class ParentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ParentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateParent([FromBody] ParentCreateDto parentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userManager.FindByEmailAsync(parentDto.ContactInfo) != null)
                return BadRequest("Email is already in use.");

            var defaultPassword = _configuration["DefaultPassword"] ?? "Default@123";
            var user = new ApplicationUser
            {
                Email = parentDto.ContactInfo,
                UserName = parentDto.ContactInfo,
                FullName = parentDto.Name,
                ProfileInfo = $"Parent - {parentDto.Name}"
            };
            var result = await _userManager.CreateAsync(user, defaultPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var parent = new Parent
            {
                UserId = user.Id
            };

            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(user, "Parent");

            if (parentDto.StudentIds != null && parentDto.StudentIds.Any())
            {
                foreach (var studentId in parentDto.StudentIds)
                {
                    var studentExists = await _context.Students.AnyAsync(s => s.StudentId == studentId);
                    if (!studentExists)
                        return BadRequest($"Student with ID {studentId} not found.");

                    var relation = new ParentsStudentRelation
                    {
                        ParentId = parent.Id,
                        StudentId = studentId
                    };
                    _context.ParentsStudentRelations.Add(relation);
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetParent), new { id = parent.Id }, new { id = parent.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParent(int id)
        {
            var parent = await _context.Parents
                .Include(p => p.User)
                .Include(p => p.ParentStudentRelations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parent == null)
                return NotFound();

            var students = parent.ParentStudentRelations != null
                ? await _context.ParentsStudentRelations
                    .Where(psr => psr.ParentId == parent.Id)
                    .Select(psr => new StudentRelationDto
                    {
                        Id = psr.Student != null ? psr.Student.StudentId : (int?)null,
                        Name = psr.Student != null ? psr.Student.Name : null
                    })
                    .ToListAsync()
                : new List<StudentRelationDto>();

            return Ok(new
            {
                id = parent.Id,
                name = parent.User?.FullName,
                contactInfo = parent.User?.Email,
                students
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParent(int id, [FromBody] ParentUpdateDto parentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parent = await _context.Parents
                .Include(p => p.User)
                .Include(p => p.ParentStudentRelations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parent == null)
                return NotFound();

            if (parent.User != null)
            {
                var existingUser = await _userManager.FindByEmailAsync(parentDto.ContactInfo);
                if (existingUser != null && existingUser.Id != parent.UserId)
                    return BadRequest("Email is already in use.");

                parent.User.FullName = parentDto.Name;
                parent.User.Email = parentDto.ContactInfo;
                parent.User.UserName = parentDto.ContactInfo;
            }

            if (parentDto.StudentIds != null)
            {
                var existingRelations = parent.ParentStudentRelations ?? new List<ParentsStudentRelation>();
                _context.ParentsStudentRelations.RemoveRange(existingRelations);

                foreach (var studentId in parentDto.StudentIds)
                {
                    var studentExists = await _context.Students.AnyAsync(s => s.StudentId == studentId);
                    if (!studentExists)
                        return BadRequest($"Student with ID {studentId} not found.");

                    var relation = new ParentsStudentRelation
                    {
                        ParentId = parent.Id,
                        StudentId = studentId
                    };
                    _context.ParentsStudentRelations.Add(relation);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParent(int id)
        {
            var parent = await _context.Parents
                .Include(p => p.User)
                .Include(p => p.ParentStudentRelations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parent == null)
                return NotFound();

            if (parent.ParentStudentRelations != null)
                _context.ParentsStudentRelations.RemoveRange(parent.ParentStudentRelations);

            if (parent.User != null)
                await _userManager.DeleteAsync(parent.User);

            _context.Parents.Remove(parent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class ParentCreateDto
    {
        public required string Name { get; set; }
        public required string ContactInfo { get; set; }
        public List<int>? StudentIds { get; set; }
    }

    public class ParentUpdateDto
    {
        public required string Name { get; set; }
        public required string ContactInfo { get; set; }
        public List<int>? StudentIds { get; set; }
    }

    public class StudentRelationDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
}
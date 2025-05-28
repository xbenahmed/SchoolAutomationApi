using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAutomationApi.Data;
using Microsoft.EntityFrameworkCore;

namespace SchoolAutomationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class BackupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public BackupsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            var backupPath = _configuration["Backup:Path"];
            if (string.IsNullOrEmpty(backupPath))
                return StatusCode(500, "Backup path not configured.");

            if (!Directory.Exists(backupPath))
                return StatusCode(500, "Backup path does not exist or is inaccessible.");

            var backupFileName = $"webschool_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
            var fullPath = Path.Combine(backupPath, backupFileName);

            try
            {
                var sql = $"BACKUP DATABASE [webschool] TO DISK = '{fullPath}' WITH INIT";
                await _context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create backup: {ex.Message}");
            }

            return Ok(new { backupFile = fullPath });
        }
    }
}
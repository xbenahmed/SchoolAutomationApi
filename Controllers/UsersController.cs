using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolAutomationApi.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace SchoolAutomationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<ApplicationUser> userManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Extract email from claims (fallback to the raw claim type if needed)
                var email = User.FindFirst(ClaimTypes.Email)?.Value
                           ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("No email claim found in the token for the authenticated user.");
                    return Unauthorized("User email not found in token.");
                }

                // Fetch the user from the database using email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("User with email {Email} not found in the database.", email);
                    return NotFound("User not found in the database.");
                }

                // Get the user's roles
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                // Build the response
                var userResponse = new
                {
                    email = user.Email,
                    fullName = user.FullName,
                    role
                };

                _logger.LogInformation("Successfully retrieved user details for {Email}.", email);
                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user details for email in token.");
                return StatusCode(500, "An error occurred while retrieving user details.");
            }
        }
    }
}
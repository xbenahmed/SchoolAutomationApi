using Microsoft.AspNetCore.Identity;

namespace SchoolAutomationApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string ProfileInfo { get; set; } = null!;
        public string FullName { get; set; } = null!;

        // Role is managed via IdentityUserRoles, accessible via UserManager
    }
}
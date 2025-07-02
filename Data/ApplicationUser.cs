using Microsoft.AspNetCore.Identity;

namespace FormsApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsBlocked { get; set; }
        public string Role { get; set; } = "User";
    }
} 
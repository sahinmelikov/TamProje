using Microsoft.AspNetCore.Identity;

namespace QrSystem.Models.Auth
{
    public class AppRole : IdentityRole
    {
        public bool IsActivated { get; set; }
    }
}

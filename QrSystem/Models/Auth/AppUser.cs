using Microsoft.AspNetCore.Identity;

namespace QrSystem.Models.Auth
{
    public class AppUser : IdentityUser
    {
        public string Fullname { get; set; }
        public bool IsActivated { get; set; }
        public int? RestorantId { get; set; }
        public Restorant Restorant { get; set; }
      
    }
}

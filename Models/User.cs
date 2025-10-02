using Microsoft.AspNetCore.Identity;

namespace FintechStatsPlatform.Models
{
    public class User : AbstractEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string[] AccountIds { get; set; }
    }
}

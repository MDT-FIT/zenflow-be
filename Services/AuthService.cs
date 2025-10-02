using FintechStatsPlatform.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace FintechStatsPlatform.Services
{
    public class AuthService
    {
        public void signIn(string username, string email, string password) { }

        public void logIn(string email, string password) { }

        public void logOut() { }

        public User getCurrentUser()
        {
            return new User();
        }
    }
}

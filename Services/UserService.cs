using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace FintechStatsPlatform.Services
{
    public class UserService
    {
        private readonly FintechContext _context;
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient, FintechContext context)
        {
            _context = context;
            _httpClient = httpClient;
        }


        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => email.Equals(u.Email));
        }
        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> UpdateUserAsync(string id, User user)
        {
            if (id != user.Id) return false;

            _context.Entry(user).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return false;
                throw;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(u => u.Id == id);
        }
    }
}

using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace FintechStatsPlatform.Services
{
    public class UserService
    {
        private readonly FintechContext _context;

        public UserService(HttpClient httpClient, FintechContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

            if (user == null)
                throw new Exception("User not found");

            return user;
        }
        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync().ConfigureAwait(false);
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id).ConfigureAwait(false);
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            if (user == null) return;
            if (id != user.Id) return;

            _context.Entry(user).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync().ConfigureAwait(false);;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return;
                throw;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return user;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id).ConfigureAwait(false);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(u => u.Id == id);
        }
    }
}

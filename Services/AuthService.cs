using FintechStatsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FintechStatsPlatform.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _secretKey;
        private readonly FintechContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        public string SecretKey { get { return _secretKey; } }


        public AuthService(string clientId, string clientSecret, string secretKey, FintechContext context)
        {
            _httpClient = new HttpClient();
            _clientId = clientId;
            _clientSecret = clientSecret;
            _secretKey = secretKey;
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }
        public string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<User> RegisterUserAsync(string username, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new Exception("Користувач із таким email вже існує");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                Email = email,
                PasswordHash = HashPassword(null, password), // хешуємо пароль
                AccountIds = new List<string>()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<string> LogInAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            var verified = VerifyPassword(user, password);
            if (!verified) return null;

            return GenerateJwtToken(user);
        }
            
        public void LogOut(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://zenflow/log-out");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
        }


        public User GetCurrentUser(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://zenflow/me");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<User>(json);
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

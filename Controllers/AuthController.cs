using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using FintechStatsPlatform.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace FintechStatsPlatform.Controllers
{
    [ApiController]
    [Route("api/zenflow/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private PasswordHasher<string> passwordHasher;
        public AuthController(AuthService authService) 
        { 
          _authService = authService;
            passwordHasher = new PasswordHasher<string>();
        }

        [HttpPost("sign-up")]
        public IActionResult SignUp([FromBody] SignUpRequest request)
        {
            // Перевірка простих умов
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email і пароль обов'язкові");

            // Створимо "фейкового" користувача
            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
            };

            if (user == null)
                return Unauthorized("Невірний email або пароль");

            user.PasswordHash = passwordHasher.HashPassword(null, request.Password);

            // Створюємо JWT токен так само, як у LogIn
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_authService.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            return Ok(new { user, access_token = accessToken });
        }

        [HttpPost("log-in")]
        public IActionResult LogIn([FromBody] LogInRequest request) 
        {
            string hashedPassword = passwordHasher.HashPassword(null, "123456");
            var user = (request.Email == "test@test.com" && PasswordVerificationResult.Success == passwordHasher.VerifyHashedPassword(null,hashedPassword,request.Password))
            ? new User{Email = request.Email }
            : null;


            if (user == null)
                return Unauthorized("Невірний email або пароль");

            // Створюємо JWT токен
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_authService.SecretKey); // з .env або appsettings.json
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            return Ok(new { access_token = accessToken });
        }

        [HttpPost("log-out")]
        public IActionResult LogOut()
        {
            // У реальному застосунку тут би видаляли токен з бази або сесії
            return Ok(new { message = "Вихід успішний" });
        }

    }
}

using FintechStatsPlatform.DTO;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly UsersService _usersService;
        private PasswordHasher<string> passwordHasher;
        public AuthController(AuthService authService, UsersService usersService) 
        { 
            _authService = authService;
            _usersService = usersService;
            passwordHasher = new PasswordHasher<string>();
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email і пароль обов'язкові");

            try
            {
                var user = await _authService.RegisterUserAsync(request.Username, request.Email, request.Password);
                var accessToken = _authService.GenerateJwtToken(user); // окремий метод у сервісі для JWT

                return Ok(new { user, access_token = accessToken });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("log-in")]
        public async Task<IActionResult> LogIn([FromBody] LogInRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Введіть логін і пароль");

            var accessToken = await _authService.LogInAsync(request.Email, request.Password);

            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Невірний email або пароль");

            return Ok(new { access_token = accessToken });
        }

        [HttpPost("log-out")]
        public IActionResult LogOut([FromBody] string accessToken)
        {
            _authService.LogOut(accessToken);
            return Ok(new { message = "Вихід успішний" });
        }

    }
}

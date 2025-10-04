using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using FintechStatsPlatform.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FintechStatsPlatform.Controllers
{
    [ApiController]
    [Route("api/zenflow/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Реєстрація нового користувача через Auth0
        /// POST /api/zenflow/auth/sign-up
        /// </summary>
        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Email і пароль обов'язкові" });
            }

            if (string.IsNullOrEmpty(request.Username))
            {
                return BadRequest(new { error = "Username обов'язковий" });
            }

            try
            {
                _logger.LogInformation("Sign up attempt for email: {Email}", request.Email);

                // Реєструємо користувача в Auth0
                var auth0User = await _authService.SignUpAsync(
                    request.Username,
                    request.Email,
                    request.Password
                );

                // Конвертуємо в нашу модель User
                var user = _authService.ConvertToUser(auth0User);

                // TODO: Зберегти користувача в БД
                // await _userRepository.CreateAsync(user);

                // Автоматично логінимо після реєстрації
                var tokenResponse = await _authService.LogInAsync(request.Email, request.Password);

                _logger.LogInformation("User successfully registered: {Email}", request.Email);

                return Ok(new
                {
                    user = new
                    {
                        email = user.Email,
                        username = user.Username,
                        id = auth0User.Sub
                    },
                    access_token = tokenResponse.AccessToken,
                    id_token = tokenResponse.IdToken,
                    refresh_token = tokenResponse.RefreshToken,
                    expires_in = tokenResponse.ExpiresIn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sign up failed for email: {Email}", request.Email);

                return BadRequest(new
                {
                    error = "Помилка реєстрації",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Вхід користувача через Auth0
        /// POST /api/zenflow/auth/log-in
        /// </summary>
        [HttpPost("log-in")]
        public async Task<IActionResult> LogIn([FromBody] LogInRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Email і пароль обов'язкові" });
            }

            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                // Автентифікуємо через Auth0
                var tokenResponse = await _authService.LogInAsync(request.Email, request.Password);

                // Отримуємо інформацію про користувача
                var userInfo = await _authService.GetUserInfoAsync(tokenResponse.AccessToken);

                // TODO: Перевірити чи існує користувач в БД, якщо ні - створити
                // var user = await _userRepository.GetByEmailAsync(userInfo.Email);
                // if (user == null) {
                //     user = _authService.ConvertToUser(userInfo);
                //     await _userRepository.CreateAsync(user);
                // }

                _logger.LogInformation("User successfully logged in: {Email}", request.Email);

                return Ok(new
                {
                    access_token = tokenResponse.AccessToken,
                    id_token = tokenResponse.IdToken,
                    refresh_token = tokenResponse.RefreshToken,
                    expires_in = tokenResponse.ExpiresIn,
                    user = new
                    {
                        id = userInfo.Sub,
                        email = userInfo.Email,
                        username = userInfo.Nickname ?? userInfo.Name,
                        email_verified = userInfo.EmailVerified
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", request.Email);

                return Unauthorized(new
                {
                    error = "Невірний email або пароль",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Оновлення access token через refresh token
        /// POST /api/zenflow/auth/refresh
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { error = "Refresh token обов'язковий" });
            }

            try
            {
                var tokenResponse = await _authService.RefreshTokenAsync(refreshToken);

                return Ok(new
                {
                    access_token = tokenResponse.AccessToken,
                    expires_in = tokenResponse.ExpiresIn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return Unauthorized(new { error = "Невалідний refresh token" });
            }
        }

        /// <summary>
        /// Вихід з системи
        /// POST /api/zenflow/auth/log-out
        /// </summary>
        [HttpPost("log-out")]
        public IActionResult LogOut()
        {
            // Для token-based auth logout виконується на клієнті
            // Токен видаляється з localStorage/sessionStorage
            _authService.LogOut();

            return Ok(new { message = "Вихід успішний" });
        }
    }
}

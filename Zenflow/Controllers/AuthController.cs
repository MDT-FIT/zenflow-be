using FintechStatsPlatform.DTO;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;



namespace FintechStatsPlatform.Controllers
{
    [ApiController]
    [Route("api/zenflow/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _jwtTokenKey = Environment.GetEnvironmentVariable("AUTH_JWT_TOKEN") ?? "jwt_token";
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly FintechContext _context;
        public AuthController(AuthService authService, ILogger<AuthController> logger, UserService usersService, FintechContext context)
        {
            _authService = authService;
            _logger = logger;
            _context = context;
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

                var auth0User = await _authService.SignUpAsync(
                    request.Username,
                    request.Email,
                    request.Password
                );

                var tokenResponse = await _authService.LogInAsync(request.Email, request.Password);

                var handler = new JwtSecurityTokenHandler();
                var idToken = handler.ReadJwtToken(tokenResponse.IdToken);

                var user = _authService.ConvertToUser(auth0User, idToken.Payload.Sub);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User successfully registered: {Email}", request.Email);


                HttpContext.Response.Cookies.Append(_jwtTokenKey, tokenResponse.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });


                return Ok(new
                {
                    user,
                    access_token = tokenResponse.AccessToken,
                    id_token = tokenResponse.IdToken,
                    refresh_token = tokenResponse.RefreshToken,
                    expires_in = tokenResponse.ExpiresIn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sign up failed for email: {Email}", request.Email);
                Console.WriteLine(ex.Message);
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

                var tokenResponse = await _authService.LogInAsync(request.Email, request.Password);

                var userInfo = await _authService.GetUserInfoAsync(tokenResponse.AccessToken);

                var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userInfo.Sub);

                if (user == null)
                {
                    user = _authService.ConvertToUser(userInfo);
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("User successfully logged in: {Email}", request.Email);


                HttpContext.Response.Cookies.Append(_jwtTokenKey, tokenResponse.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
                return Unauthorized(new { error = "Невалідний refresh token" });
            }
        }

        /// <summary>
        /// Вихід з системи
        /// POST /api/zenflow/auth/log-out
        /// </summary>
        [HttpPost("log-out")]
        public IActionResult LogOut([FromBody] string accessToken)
        {
            HttpContext.Response.Cookies.Delete(_jwtTokenKey);

            _authService.LogOut();

            return Ok(new { message = "Вихід успішний" });
        }
    }
}

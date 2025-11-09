using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Zenflow.DTO;
using Zenflow.Helpers;
using Zenflow.Models;
using Zenflow.Services;
using static Zenflow.Helpers.ExceptionTypes;

namespace Zenflow.Controllers
{
    [ApiController]
    [Route("api/zenflow/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly FintechContext _context;

        public AuthController(
            AuthService authService,
            ILogger<AuthController> logger,
            UserService usersService,
            FintechContext context
        )
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

                Auth0UserInfo auth0User = await _authService
                    .SignUpAsync(request.Username, request.Email, request.Password)
                    .ConfigureAwait(false);

                Auth0TokenResponse tokenResponse = await _authService
                    .LogInAsync(request.Email, request.Password)
                    .ConfigureAwait(false);

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken idToken = handler.ReadJwtToken(tokenResponse.IdToken);

                User user = _authService.ConvertToUser(auth0User, idToken.Payload.Sub);
                _context.Users.Add(user);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                _logger.LogInformation("User successfully registered: {Email}", request.Email);

                HttpContext.Response.Cookies.Append(
                    EnvConfig.AuthJwt,
                    tokenResponse.AccessToken ?? "",
                    CookieConfig.Default()
                );


                HttpContext.Response.Cookies.Append(
                     EnvConfig.AuthRefreshJwt,
                    tokenResponse.RefreshToken ?? "",
                    CookieConfig.Default()
                );

                return Ok(
                    new
                    {
                        user,
                        access_token = tokenResponse.AccessToken,
                        id_token = tokenResponse.IdToken,
                        refresh_token = tokenResponse.RefreshToken,
                        expires_in = tokenResponse.ExpiresIn,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sign up failed for email: {Email}", request.Email);
                Console.WriteLine(ex.Message);
                return BadRequest(new { error = "Помилка реєстрації", message = ex.Message });
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

                Auth0TokenResponse tokenResponse = await _authService
                    .LogInAsync(request.Email, request.Password)
                    .ConfigureAwait(false);

                Auth0UserInfo userInfo = await _authService
                    .GetUserInfoAsync(tokenResponse.AccessToken ?? "")
                    .ConfigureAwait(false);

                User? user = await _context
                    .Users.FirstOrDefaultAsync(user => user.Id == userInfo.Sub)
                    .ConfigureAwait(false);

                if (user == null)
                {
                    user = _authService.ConvertToUser(userInfo);
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }

                _logger.LogInformation("User successfully logged in: {Email}", request.Email);

                HttpContext.Response.Cookies.Append(
                    EnvConfig.AuthJwt,
                    tokenResponse.AccessToken ?? "",
                    CookieConfig.Default()
                );

                HttpContext.Response.Cookies.Append(
              EnvConfig.AuthRefreshJwt,
              tokenResponse.RefreshToken ?? "",
              CookieConfig.Default()
          );


                return Ok(
                    new
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
                            email_verified = userInfo.EmailVerified,
                        },
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", request.Email);
                Console.WriteLine(ex.Message);
                return Unauthorized(
                    new { error = "Невірний email або пароль", message = ex.Message }
                );
            }
        }

        /// <summary>
        /// Вхід користувача через Auth0
        /// POST /api/zenflow/auth/get-user
        /// </summary>
        [HttpGet("get-user")]
        public async Task<IActionResult> GetUser()
        {

            string token = HttpContext.Request.Cookies[EnvConfig.AuthJwt] ?? "";
            string refreshToken = HttpContext.Request.Cookies[EnvConfig.AuthRefreshJwt] ?? "";

            bool isTokenValid = AuthService.ValidateToken(token);

            try
            {
                string currentToken = token;

                if (isTokenValid == false)
                {
                    Auth0TokenResponse tokenResponse = await _authService
                   .RefreshTokenAsync(refreshToken)
                   .ConfigureAwait(false);

                    HttpContext.Response.Cookies.Append(
               EnvConfig.AuthJwt,
               tokenResponse.AccessToken ?? "",
               CookieConfig.Default()
           );

                    currentToken = tokenResponse.AccessToken ?? "";
                }


                _logger.LogInformation("Attempt to get UserInfo");

                Auth0UserInfo userInfo = await _authService
                    .GetUserInfoAsync(currentToken)
                    .ConfigureAwait(false);

                User? user = await _context
                    .Users.FirstOrDefaultAsync(user => user.Id == userInfo.Sub)
                    .ConfigureAwait(false);

                if (userInfo == null)
                {
                    throw new UserNotFoundException();
                }

                if (user == null)
                {
                    user = _authService.ConvertToUser(userInfo);
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }

                List<string?> accountIds = await _context
                 .BankAccounts.Where(account => account.UserId == user.Id)
                 .Select(account => account.Id).ToListAsync();

                user.AccountIds = accountIds.Where(account => account != null).Select(account => account!).ToList();
                User userAggregated = new User(id: user.Id ?? "", username: userInfo.Nickname ?? "", email: userInfo.Email ?? "", accountIds: user.AccountIds);

                await _context.SaveChangesAsync().ConfigureAwait(false);

                return Ok(
                    new
                    {
                        user = userAggregated
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current user");
                Console.WriteLine(ex.Message);
                return Unauthorized(
                    new { error = "Unable to get current user", message = ex.Message }
                );
            }
        }

        /// <summary>
        /// Оновлення access token через refresh token
        /// POST /api/zenflow/auth/refresh
        /// </summary>
        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { error = "Refresh token обов'язковий" });
            }

            try
            {
                Auth0TokenResponse tokenResponse = await _authService
                    .RefreshTokenAsync(refreshToken)
                    .ConfigureAwait(false);

                HttpContext.Response.Cookies.Append(
                  EnvConfig.AuthJwt,
                  tokenResponse.AccessToken ?? "",
                  CookieConfig.Default()
              );

                HttpContext.Response.Cookies.Append(
                  EnvConfig.AuthRefreshJwt,
                  tokenResponse.RefreshToken ?? "",
                  CookieConfig.Default()
              );


                return Ok(
                );
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
        public IActionResult LogOut()
        {
            HttpContext.Response.Cookies.Delete(EnvConfig.AuthJwt, CookieConfig.Default());
            HttpContext.Response.Cookies.Delete(EnvConfig.AuthRefreshJwt, CookieConfig.Default());

            return Ok(new { message = "Вихід успішний" });
        }
    }
}

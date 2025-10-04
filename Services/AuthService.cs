using FintechStatsPlatform.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FintechStatsPlatform.Services
{
    // Response моделі від Auth0
    public class Auth0TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class Auth0UserInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("picture")]
        public string Picture { get; set; }
    }

    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _domain;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _audience;
        private readonly string _connectionName;
        private readonly string _secretKey;

        public string SecretKey => _secretKey;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _domain = _configuration["Auth0:Domain"] ?? throw new ArgumentNullException("Auth0:Domain");
            _clientId = _configuration["Auth0:ClientId"] ?? throw new ArgumentNullException("Auth0:ClientId");
            _clientSecret = _configuration["Auth0:ClientSecret"] ?? throw new ArgumentNullException("Auth0:ClientSecret");
            _audience = _configuration["Auth0:Audience"] ?? throw new ArgumentNullException("Auth0:Audience");
            _connectionName = _configuration["Auth0:ConnectionName"] ?? "Username-Password-Authentication";
            _secretKey = _configuration["Jwt:SecretKey"] ?? "your-secret-key-min-32-chars-long!";
        }

        /// <summary>
        /// Sign in with oAuth
        /// </summary>
        public async Task<Auth0UserInfo> SignUpAsync(string username, string email, string password)
        {
            var requestBody = new
            {
                client_id = _clientId,
                email = email,
                password = password,
                connection = _connectionName,
                name = username,
                username = username
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"https://{_domain}/dbconnections/signup",
                content
            );

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Auth0 signup failed: {responseContent}");
            }

            // Auth0 повертає створеного користувача
            var signupResponse = JsonSerializer.Deserialize<Auth0UserInfo>(responseContent);

            // Після реєстрації автоматично логінимо для отримання токенів
            await LogInAsync(email, password);

            return signupResponse ?? throw new Exception("Failed to deserialize signup response");
        }

        /// <summary>
        /// Логін через Auth0 (Resource Owner Password Grant)
        /// Повертає токен який можна використати для доступу до API
        /// </summary>
        public async Task<Auth0TokenResponse> LogInAsync(string email, string password)
        {
            var requestBody = new
            {
                grant_type = "http://auth0.com/oauth/grant-type/password-realm",
                username = email,
                password = password,
                client_id = _clientId,
                client_secret = _clientSecret,
                audience = _audience,
                scope = "openid profile email offline_access",
                realm = _connectionName
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"https://{_domain}/oauth/token",
                content
            );

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Auth0 authentication failed: {responseContent}");
            }

            return JsonSerializer.Deserialize<Auth0TokenResponse>(responseContent)
                ?? throw new Exception("Failed to deserialize Auth0 response");
        }

        /// <summary>
        /// Логін через Auth0 (спрощена версія, повертає тільки токен для сумісності)
        /// </summary>
        public async Task<string> LogIn(string email, string password)
        {
            var tokenResponse = await LogInAsync(email, password);
            return tokenResponse.AccessToken;
        }

        /// <summary>
        /// Отримати інформацію про користувача
        /// </summary>
        public async Task<Auth0UserInfo> GetUserInfoAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{_domain}/userinfo");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve user info: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Auth0UserInfo>(content)
                ?? throw new Exception("Failed to deserialize user info");
        }

        /// <summary>
        /// Отримати поточного користувача (для сумісності зі старим кодом)
        /// </summary>
        public async Task<User> GetCurrentUser(string accessToken)
        {
            var userInfo = await GetUserInfoAsync(accessToken);
            return ConvertToUser(userInfo);
        }

        /// <summary>
        /// Оновити токен через refresh token
        /// </summary>
        public async Task<Auth0TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var requestBody = new
            {
                grant_type = "refresh_token",
                client_id = _clientId,
                client_secret = _clientSecret,
                refresh_token = refreshToken
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"https://{_domain}/oauth/token",
                content
            );

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to refresh token: {responseContent}");
            }

            return JsonSerializer.Deserialize<Auth0TokenResponse>(responseContent)
                ?? throw new Exception("Failed to deserialize refresh token response");
        }

        /// <summary>
        /// Logout (на клієнті видаляємо токен)
        /// </summary>
        public void LogOut(string accessToken = null)
        {
            // Для token-based auth logout відбувається на клієнті
            // Токен просто видаляється з localStorage
        }

        /// <summary>
        /// Конвертувати Auth0UserInfo в модель User
        /// </summary>
        public User ConvertToUser(Auth0UserInfo userInfo)
        {
            return new User
            {
                Email = userInfo.Email,
                Username = userInfo.Nickname ?? userInfo.Name ?? userInfo.Email.Split('@')[0],
                // PasswordHash не зберігаємо, бо Auth0 управляє паролями
            };
        }

        /// <summary>
        /// Реєстрація (спрощена версія для сумісності)
        /// </summary>
        public string SignUp(string username, string email, string password)
        {
            var task = SignUpAsync(username, email, password);
            task.Wait();
            var userInfo = task.Result;

            // Повертаємо JSON з інформацією про користувача
            return JsonSerializer.Serialize(userInfo);
        }
    }
}

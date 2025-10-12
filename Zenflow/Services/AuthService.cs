using FintechStatsPlatform.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zenflow.Env;
using static FintechStatsPlatform.Exceptions.ExceptionTypes;

namespace FintechStatsPlatform.Services
{
    public class Auth0TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }

    public class Auth0UserInfo
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }

    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Sign in with oAuth
        /// </summary>
        public async Task<Auth0UserInfo> SignUpAsync(string username, string email, string password)
        {
            var requestBody = new
            {
                client_id = EnvConfig.AuthClientId,
                email = email,
                password = password,
                connection = EnvConfig.AuthConnection,
                name = username,
                username = username
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                EnvConfig.AuthConnectUri,
                content
            ).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Auth0Exception($"Signup failed: {responseContent}");

            var signupResponse = JsonSerializer.Deserialize<Auth0UserInfo>(responseContent)
                ?? throw new Auth0DeserializationException("signup response");

            await LogInAsync(email, password).ConfigureAwait(false);

            return signupResponse;
        }

        /// <summary>
        /// Логін через Auth0 (Resource Owner Password Grant)
        /// Повертає токен який можна використати для доступу до API
        /// </summary>
        public async Task<Auth0TokenResponse> LogInAsync(string email, string password)
        {
            var requestBody = new
            {
                grant_type = EnvConfig.AuthGrantType,
                username = email,
                password = password,
                client_id = EnvConfig.AuthClientId,
                client_secret = EnvConfig.AuthClientSecret,
                audience = EnvConfig.AuthAudience,
                scope = "openid profile email offline_access",
                realm = EnvConfig.AuthConnection
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                EnvConfig.AuthTokenUri,
                content
            ).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Auth0Exception($"Authentication failed: {responseContent}");

            var tokenResponse = JsonSerializer.Deserialize<Auth0TokenResponse>(responseContent)
                ?? throw new JsonParsingException("Failed to deserialize Auth0 response");

            return tokenResponse;
        }

        /// <summary>
        /// Логін через Auth0 (спрощена версія, повертає тільки токен для сумісності)
        /// </summary>
        public async Task<string> LogIn(string email, string password)
        {
            var tokenResponse = await LogInAsync(email, password).ConfigureAwait(false);
            return tokenResponse.AccessToken ?? "";
        }

        /// <summary>
        /// Отримати інформацію про користувача
        /// </summary>
        public async Task<Auth0UserInfo> GetUserInfoAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, EnvConfig.AuthUserInfoUri);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Auth0Exception($"Failed to retrieve user info: {content}");

            return JsonSerializer.Deserialize<Auth0UserInfo>(content)
                ?? throw new Auth0DeserializationException("user info");
        }

        /// <summary>
        /// Отримати поточного користувача (для сумісності зі старим кодом)
        /// </summary>
        public async Task<User> GetCurrentUser(string accessToken)
        {
            var userInfo = await GetUserInfoAsync(accessToken).ConfigureAwait(false);
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
                client_id = EnvConfig.AuthClientId,
                client_secret = EnvConfig.AuthClientSecret,
                refresh_token = refreshToken
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                EnvConfig.AuthTokenUri,
                content
            ).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Auth0Exception($"Failed to refresh token: {responseContent}");

            return JsonSerializer.Deserialize<Auth0TokenResponse>(responseContent)
                ?? throw new Auth0DeserializationException("refresh token response");
        }

        /// <summary>
        /// Logout (на клієнті видаляємо токен)
        /// </summary>
        public void LogOut(string? accessToken = null)
        {
            // Для token-based auth logout відбувається на клієнті
            // Токен просто видаляється з localStorage
        }

        /// <summary>
        /// Конвертувати Auth0UserInfo в модель User
        /// </summary>
        public User ConvertToUser(Auth0UserInfo userInfo, string forceId = "")
        {
            return new User(id: forceId ?? userInfo.Sub ?? "");
        }

        /// <summary>
        /// Реєстрація (спрощена версія для сумісності)
        /// </summary>
        public string SignUp(string username, string email, string password)
        {
            var task = SignUpAsync(username, email, password);
            task.Wait();
            var userInfo = task.Result;

            return JsonSerializer.Serialize(userInfo);
        }

        public async Task<string> GetUserTokenAsync(string email, string password)
        {
            var requestBody = new
            {
                grant_type = "http://auth0.com/oauth/grant-type/password-realm",
                username = email,
                password = password,
                client_id = EnvConfig.AuthClientId,
                client_secret = EnvConfig.AuthClientSecret,
                audience = EnvConfig.AuthAudience,
                scope = "openid profile email balances:read offline_access",
                realm = EnvConfig.AuthConnection,
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(EnvConfig.AuthTokenUri, content).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Auth0Exception($"Failed to get user token: {responseContent}");

            var tokenResponse = JsonSerializer.Deserialize<Auth0TokenResponse>(responseContent)
            ?? throw new Auth0DeserializationException("token response");

            return tokenResponse.AccessToken;
        }

    }
}

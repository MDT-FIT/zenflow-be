using FintechStatsPlatform.Models;
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
        public string SecretKey { get { return _secretKey; } }


        public AuthService(string clientId, string clientSecret, string secretKey)
        {
            _httpClient = new HttpClient();
            _clientId = clientId;
            _clientSecret = clientSecret;
            _secretKey = secretKey;
        }

        public string SignUp(string username, string email, string password)
        {
            var body = new
            {
                username = username,
                email = email,
                password = password
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync("https://zenflow/sign-up", content).Result;
            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync().Result;
        }


        public string LogIn(string email, string password)
        {
            var body = new
            {
                email = email,
                password = password
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync("https://zenflow/log-in", content).Result;
            response.EnsureSuccessStatusCode();

            var responseJson = response.Content.ReadAsStringAsync().Result;

            // наприклад, витягнемо токен
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("access_token").GetString();
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

    }
}

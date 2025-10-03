using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace FintechStatsPlatform.Services
{
    public class BankService
    {

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;


        public BankService(string clientId, string clientSecret, IMemoryCache cache)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _httpClient = new HttpClient();
            _cache = cache;
        }


        public  List<Transaction> listTransactions(TransactionFilter filter) 
        {
            return new List<Transaction>();
        }

        public List<BankConfig> listBankConfigs(string userdId)
        {
            return new List<BankConfig>();
        }

        public Balance getBalance(BalanceFilter filter)
        { 
            return new Balance();
        }

        public  void connectMono() { }

        public void connectOtherBank(string userdId, BankConfig config)
        {
            
        }
        public string GetTinkAccessToken()
        {
            // спробуємо взяти з кешу
            if (_cache.TryGetValue("TinkAccessToken", out string token))
            {
                return token;
            }

            // якщо немає у кеші → робимо запит
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "authorization:grant,user:create"),
        });

            var response = _httpClient.PostAsync("https://api.tink.com/api/v1/oauth/token", content).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;

            using var doc = JsonDocument.Parse(json);
            token = doc.RootElement.GetProperty("access_token").GetString();
            int expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32(); // сек

            // кладемо в кеш з TTL (трошки менше, ніж реальний)
            _cache.Set("TinkAccessToken", token, TimeSpan.FromSeconds(expiresIn - 60));

            return token;
        }



    }
}

using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;

namespace FintechStatsPlatform.Services
{
    public class BankService
    {
        private string tinkLink = "https://link.tink.com/1.0/account-check/?client_id=d33f61e7a07f42baa2a18292a2d2ac61&redirect_uri=https%3A%2F%2Fconsole.tink.com%2Fcallback&market=PL&locale=en_US";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly BankConfig _tinkConfig;



        public BankService(string clientId, string clientSecret, IMemoryCache cache)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _httpClient = new HttpClient();
            _cache = cache;
            _tinkConfig = new BankConfig(tinkLink);
        }

        public  List<Transaction> listTransactions(TransactionFilter filter) 
        {
            return new List<Transaction>();
        }

        public List<BankConfig> listBankConfigs(string userId)
        {
            
            // Додати перевірку чи у юзера підключен банк
            string id = "id"; // get from DB
            if (id == userId)
            {
                //hardcode
                var user = new User();
                user.Id = userId;

                //
                List<BankConfig> bankConfigs = new List<BankConfig> {_tinkConfig};
                List<BankConfig> filterdConfigs = bankConfigs.Where(config => user.isBankConnected(config.BankName)).ToList();
                return filterdConfigs;
            }
            return new List<BankConfig>();
        }

        public Balance getBalance(BalanceFilter filter)
        { 
            return new Balance();
        }

        public  void connectMono() { }

        public async void connectOtherBank(string userId, string accountVerificationId)
        {
            string token = GetTinkAccessToken("account-verification-reports:read");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response =  await _httpClient.GetAsync($"api/v1/account-verification-reports/{userId}");
                var json = response.Content.ReadAsStringAsync().Result;

                var doc = JsonDocument.Parse(json);
                var accounts = doc.RootElement.GetProperty("accounts");

                List<Account> userAccountsList = new List<Account>();
                foreach (var tinkAccounts in accounts.EnumerateArray())
                { 
                    string id = tinkAccounts.GetProperty("id").GetString();
                    string fullBankId = User.bankNamesKeyValuePairs[Enumirators.BankName.OTHER].Concat(id).ToString();
                    userAccountsList.Add(new Account(userId, fullBankId));
                }
                // зберігаємо в базі всі банківські акаунти юзера
            }

        }
        public string GetTinkAccessToken(string scope)
        {

            // якщо немає у кеші → робимо запит
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", scope),
            });

            var response = _httpClient.PostAsync("https://api.tink.com/api/v1/oauth/token", content).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;

            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            int expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32(); // сек

            return token;
        }
    }
}

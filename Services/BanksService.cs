using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FintechStatsPlatform.Services
{
    public class BanksService
    {
        private string tinkLink = "https://link.tink.com/1.0/account-check/?client_id=d33f61e7a07f42baa2a18292a2d2ac61&redirect_uri=https%3A%2F%2Fconsole.tink.com%2Fcallback&market=PL&locale=en_US";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly BankConfig _tinkConfig;
        private readonly FintechContext _context;



        public BanksService(string clientId, string clientSecret, IMemoryCache cache, FintechContext context)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _httpClient = new HttpClient();
            _cache = cache;
            _tinkConfig = new BankConfig(apiLink: tinkLink);
            _context = context;
        }

        public  List<Transaction> listTransactions(TransactionFilter filter) 
        {
            return new List<Transaction>();
        }

        public List<BankConfig> listBankConfigs(string userId)
        {

            // Додати перевірку чи у юзера підключен банк
            var user = _context.Users
                       .Include(u => u.Accounts)
                       .FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return new List<BankConfig>();
            else
            {
                var allBankConfigs = new List<BankConfig>
                {
                    _tinkConfig
                };
                List<BankConfig> filterdConfigs = allBankConfigs.Where(config => user.isBankConnected(config.BankName)).ToList();
                return filterdConfigs;
            }
        }

        public async Task<BalanceResponse> GetBalanceAsync(string accountId, string userAccessToken)
        {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://api.tink.com/api/v1/accounts/{accountId}/balances"
                );

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Tink API returned {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BalanceResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public  void connectMono() { }

        public async Task connectOtherBankAsync(string userId, string code)
        {
            string token = GetTinkAccessToken(code);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://api.tink.com/api/v1/accounts/list");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var accountsJson = doc.RootElement.GetProperty("accounts");

            List<BankAccount> userAccountsList = new List<BankAccount>();

            foreach (var tinkAccount in accountsJson.EnumerateArray())
            {
                string id = tinkAccount.GetProperty("id").GetString();
                string fullBankId = User.BankNameMap[Enumirators.BankName.OTHER] + id;

                userAccountsList.Add(new BankAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    BankId = fullBankId,
                    Balance = 0
                });
            }

            await _context.BankAccounts.AddRangeAsync(userAccountsList);
            await _context.SaveChangesAsync();
        }

        public string GetTinkAccessToken(string code)
        {
            string scope = "accounts:read,balances:read,transactions:read";

            // якщо немає у кеші → робимо запит
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string> ("code", code),
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

        public async Task<List<Bank>> GetBanksAsync()
        {
            return await _context.Banks.ToListAsync();
        }

        public async Task<Bank> GetBankByIdAsync(string id)
        {
            return await _context.Banks.FindAsync(id);
        }

        public async Task<Bank> AddBankAsync(Bank bank)
        {
            _context.Banks.Add(bank);
            await _context.SaveChangesAsync();
            return bank;
        }

        public async Task<bool> UpdateBankAsync(string id, Bank bank)
        {
            if (id != bank.Id) return false;

            _context.Entry(bank).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BankExistsAsync(id)) return false;
                throw;
            }
        }

        public async Task<bool> DeleteBankAsync(string id)
        {
            var bank = await _context.Banks.FindAsync(id);
            if (bank == null) return false;

            _context.Banks.Remove(bank);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> BankExistsAsync(string id)
        {
            return await _context.Banks.AnyAsync(b => b.Id == id);
        }
    }
}

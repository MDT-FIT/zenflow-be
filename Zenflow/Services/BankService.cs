using FintechStatsPlatform.Enumirators;
using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FintechStatsPlatform.Services
{
    public class BankService
    {
        private readonly string BaseApiLink = Environment.GetEnvironmentVariable("TINK_API_LINK") ?? "";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private readonly FintechContext _context;

        public BankService(HttpClient httpClient, FintechContext context)
        {
            _clientId = Environment.GetEnvironmentVariable("TINK_CLIENT_ID") ?? "";
            _clientSecret = Environment.GetEnvironmentVariable("TINK_CLIENT_SECRET") ?? "";
            _httpClient = new HttpClient();
            _httpClient = httpClient;
            _context = context;
        }

        public List<Transaction> ListTransactions(TransactionFilter filter)
        {
            throw new NotImplementedException("Has been not implemented yet");
        }

        public List<BankConfig> ListBankConfigs(string userId)
        {

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return new List<BankConfig>();
            else
            {

                var allBankConfigs = _context.Banks.ToList();

                List<BankConfig> filterdConfigs = allBankConfigs.Where(config => !user.IsBankConnected(config.Name)).ToList();
                return filterdConfigs;
            }
        }

        public async Task<BalanceResponse> GetBalanceAsync(string accountId, string userAccessToken)
        {
            Console.WriteLine(BaseApiLink);
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{BaseApiLink}/api/v1/accounts/{accountId}/balances"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

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


        public void ConnectMono()
        {
            throw new NotImplementedException("Has been not implemented yet");
        }

        public async Task ConnectOtherBankAsync(string userId, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{BaseApiLink}/data/v2/accounts");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var accountsJson = doc.RootElement.GetProperty("accounts");

            List<BankAccount> userAccountsList = new List<BankAccount>();

            var bank = _context.Banks.FirstOrDefault(b => b.Name == BankName.OTHER);

            foreach (var tinkAccount in accountsJson.EnumerateArray())
            {
                string id = tinkAccount.GetProperty("id").GetString() ?? "";
                string fullBankId = BankNameMapper.BankNameToIdMap[BankName.OTHER] + id;

                userAccountsList.Add(new BankAccount
                {
                    Id = fullBankId,
                    UserId = userId,
                    BankId = bank != null ? bank.Id : "",
                    Balance = 0
                });
            }

            await _context.BankAccounts.AddRangeAsync(userAccountsList);
            await _context.SaveChangesAsync();
        }

        public string GetTinkAccessToken(string code = "")
        {

            var parameters = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
            };

            var content = new FormUrlEncodedContent(parameters);

            var response = _httpClient.PostAsync($"{BaseApiLink}/api/v1/oauth/token", content).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;

            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            int expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();

            return token;
        }

        public async Task<List<BankConfig>> GetBanksAsync()
        {
            return await _context.Banks.ToListAsync();
        }

        public async Task<BankConfig> GetBankByIdAsync(string id)
        {
            return await _context.Banks.FindAsync(id);
        }

        public async Task<BankConfig> AddBankAsync(BankConfig bank)
        {
            _context.Banks.Add(bank);
            await _context.SaveChangesAsync();
            return bank;
        }

        public async Task<bool> UpdateBankAsync(string id, BankConfig bank)
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

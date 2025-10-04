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
        private readonly string BaseApiLink = Environment.GetEnvironmentVariable("TINK_TINK_API_LINK") ?? "";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private readonly FintechContext _context;

        public BankService(HttpClient httpClient, FintechContext context)
        {
            var tinkLink = Environment.GetEnvironmentVariable("TINK_FLOW_LINK") ?? "";

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

        public Balance GetBalance(BalanceFilter filter)
        {
            throw new NotImplementedException("Has been not implemented yet");
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

            // FIX ME: WTF the bankId violates the foreign key constraint ????
            foreach (var tinkAccount in accountsJson.EnumerateArray())
            {
                string id = tinkAccount.GetProperty("id").GetString();
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

        public string GetTinkAccessToken(string code = "", string scope = "")
        {

            var parameters = new Dictionary<string, string>()
    {
        { "grant_type", "authorization_code" },
        { "code", code },
        { "client_id", _clientId },
        { "client_secret", _clientSecret }
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

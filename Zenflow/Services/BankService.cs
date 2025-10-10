
﻿using FintechStatsPlatform.DTO;
using FintechStatsPlatform.Enumirators;
using FintechStatsPlatform.Exceptions;
using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using static FintechStatsPlatform.Exceptions.ExceptionTypes;


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

        public async Task<List<TinkTransaction>> ListTransactionsAsync(TransactionFilter filter, string userAccessToken)
        {
            // Get User accounts' ids if there is none
            if (filter.AccountIds == null || filter.AccountIds.Length == 0)
            {
                filter.AccountIds = [.. GetUserAccounts(filter.UserId).Select(a => a.Id)];
            }

            filter.AccountIds = [.. filter.AccountIds.Select(a => a.Replace("tink-", ""))];

            // Define parameters and convert them to JSON content
            var parameters = new
            {
                accounts = filter.AccountIds,
                startDate = filter.DateFrom,
                endDate = filter.DateTo,
            };

            var jsonContent = JsonContent.Create(parameters);

            // Form POST request to Tink's API
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = jsonContent,
                RequestUri = new Uri($"{BaseApiLink}/api/v1/search"),
            };

            // Add header with user's access token
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

            var response = await _httpClient.SendAsync(requestMessage);

            // Throw exception in case request failed
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<TinkTransactionResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var transactions = apiResponse?.Results
                .Select(r => r.Transaction).ToList();

            // Attach User's id to each transaction
            if (transactions != null)
            {
                foreach(var transaction in transactions)
                {
                    transaction.UserId = filter.UserId;
                }
            }

            return transactions ?? [];
        }

        private List<BankAccount> GetUserAccounts(string userId)
        {
            return [.. _context.BankAccounts.Where(a => a.UserId == userId)];
        }

        public List<BankConfig> ListBankConfigs(string userId)
        {

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                throw new ExceptionTypes.UserNotFoundException("id",userId);
            else
            {
                var allBankConfigs = _context.Banks.ToList();
                List<BankConfig> filterdConfigs = allBankConfigs.Where(config => !user.IsBankConnected(config.Name)).ToList();
                return filterdConfigs;
            }
        }
        public async Task<List<Balance>> GetBalancesAsync(List<string> accountIds, string token, string userId)
        {
            var results = new List<Balance>();
            foreach (var accountId in accountIds)
                try
                {
                    var balance = await GetBalanceAsync(accountId, token, userId);
                    results.Add(balance);
                }
                catch(NotFoundException ex) 
                {
                    results.Add(new Balance(userId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Untrackable exception occured while attempt of processing account #{accountId} :\n{ex.ToString()}");
                }

            return results;
        }

        public async Task<Balance> GetBalanceAsync(string accountId, string userAccessToken, string userId)
        {
               
            var results = new List<Balance>();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{BaseApiLink}/api/v1/accounts/{accountId}/balances"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Tink API returned {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new AccountNotFoundException("id", accountId);
                throw new ExternalApiException("Tink", $"Status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);

            var available = doc.RootElement
                .GetProperty("balances")
                .GetProperty("available");

            return new Balance
            (   userId,
                available.GetProperty("unscaledValue").GetInt64(),
                available.GetProperty("scale").GetInt32(),
                doc.RootElement.GetProperty("accountId").GetString(),
                available.GetProperty("currencyCode").GetString()
            );
        }


        public void ConnectMono()
        {
            throw new NotImplementedException("Has been not implemented yet");
        }

        public async Task ConnectOtherBankAsync(string userId, string token)
        {
            try
            {

            }
            catch (ExternalApiException ex)
            {

            }
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{BaseApiLink}/data/v2/accounts");
            response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
                throw new ExternalApiException("Tink", $"{response.Content.ToString()}");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            

            var accountsJson = doc.RootElement.GetProperty("accounts");

            List<BankAccount> userAccountsList = new List<BankAccount>();

            var bank = _context.Banks.FirstOrDefault(b => b.Name == BankName.OTHER);
            if (bank == null)
                throw new BankNotFoundException("OTHER");

            foreach (var tinkAccount in accountsJson.EnumerateArray())
            {
                string id = tinkAccount.GetProperty("id").GetString();
                string fullBankId = BankNameMapper.BankNameToIdMap[BankName.OTHER] + id;


                int scale = 0;
                long result = 0;
                long unscaled = 0;

                try
                {
                    var value = tinkAccount
                        .GetProperty("balances")
                        .GetProperty("booked")
                        .GetProperty("amount")
                        .GetProperty("value");

                    unscaled = long.Parse(value.GetProperty("unscaledValue").GetString());
                    scale = int.Parse(value.GetProperty("scale").GetString());
                    result = unscaled * (long)Math.Pow(10, -scale);


                    Console.WriteLine($"Account {id}: unscaledValue={unscaled}, scale={scale}");

                }
                catch (Exception ex)
                {
                    throw new UnexpectedException("attempt to connect to other bank",(CustomException)ex);
                }

                userAccountsList.Add(new BankAccount
                {
                    Id = fullBankId,
                    UserId = userId,
                    BankId = bank?.Id ?? throw new BankNotFoundException(bank.Id, "id"),
                    Balance = result,
                    CurrencyScale = scale
                });
            }

            await _context.BankAccounts.AddRangeAsync(userAccountsList);
            await _context.SaveChangesAsync();
        }
        //decimal Pow10(int exponent)
        //{
        //    decimal result = 1m;
        //    if (exponent > 0)
        //        for (int i = 0; i < exponent; i++)
        //            result *= 10m;
        //    else if (exponent < 0)
        //        for (int i = 0; i < -exponent; i++)
        //            result /= 10m;
        //    return result;
        //}


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

using System.Net.Http.Headers;
using System.Text.Json;
using FintechStatsPlatform.DTO;
using FintechStatsPlatform.Enumirators;
using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;
using Zenflow.Helpers;
using static Zenflow.Helpers.ExceptionTypes;

namespace FintechStatsPlatform.Services
{
    public class BankService
    {
        private readonly HttpClient _httpClient;
        private readonly FintechContext _context;
        private readonly JsonSerializerOptions serializationOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public BankService(HttpClient httpClient, FintechContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<List<TinkTransaction>> ListTransactionsAsync(
            TransactionFilter filter,
            string userAccessToken,
            int? minAmount = null,
            int? maxAmount = null
        )
        {
            if (filter == null)
                throw new ParameterNotFound("filter", "filter", "");

            if (filter.AccountIds.Count == 0)
                filter.AccountIds.AddRange(
                    GetUserAccounts(filter.UserId).Select(a => a.Id!).ToList()
                );

            var tinkAccountIds = filter
                .AccountIds.Where(a =>
                    a.StartsWith(
                        BankNameMapper.BankNameToIdMap[BankName.OTHER],
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Select(a =>
                    a.Replace(
                        BankNameMapper.BankNameToIdMap[BankName.OTHER],
                        "",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ToArray();

            var tinkTransactions = new List<TinkTransaction>();

            if (tinkAccountIds.Length > 0)
            {
                var parameters = new
                {
                    accounts = filter.AccountIds,
                    startDate = filter.DateFrom,
                    endDate = filter.DateTo,
                    minAmount,
                    maxAmount,
                };

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    EnvConfig.TinkListTransactionUri
                )
                {
                    Content = JsonContent.Create(parameters),
                };
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    userAccessToken
                );

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var apiResponse = JsonSerializer.Deserialize<TinkTransactionResponse>(
                    content,
                    serializationOptions
                );

                tinkTransactions = apiResponse?.Results.Select(r => r.Transaction).ToList() ?? [];
            }

            foreach (var transaction in tinkTransactions)
            {
                transaction.UserId = filter.UserId;
            }

            return tinkTransactions;

            //// Define parameters and convert them to JSON content
            //var parameters = new { accounts = filter.AccountIds, startDate = filter.DateFrom, endDate = filter.DateTo };
            //var jsonContent = JsonContent.Create(parameters);

            //// Tink Api request
            //if (tinkAccountIds != null && tinkAccountIds.Length > 0)
            //{
            //    // Form POST request to Tink's API
            //    var requestMessage = new HttpRequestMessage
            //    {
            //        Method = HttpMethod.Post,
            //        Content = jsonContent,
            //        RequestUri = EnvConfig.TinkListTransactionUri,
            //    };

            //    // Add header with user's access token
            //    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

            //    // Send request and throw exception in case it failed
            //    var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            //    response.EnsureSuccessStatusCode();

            //    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            //    var apiResponse = JsonSerializer.Deserialize<TinkTransactionResponse>(content, new JsonSerializerOptions
            //    {
            //        PropertyNameCaseInsensitive = true
            //    });

            //    tinkTransactions = apiResponse?.Results
            //        .Select(r => r.Transaction).ToList();
            //}

            //// Add mono logic later {...}

            //// Attach User's id to each transaction
            //if (tinkTransactions != null)
            //{
            //    foreach (var transaction in tinkTransactions)
            //    {
            //        transaction.UserId = filter.UserId;
            //    }
            //}

            //return tinkTransactions ?? [];
        }

        private List<BankAccount> GetUserAccounts(string userId)
        {
            return [.. _context.BankAccounts.Where(a => a.UserId == userId)];
        }

        public List<BankConfig> ListBankConfigs(string userId)
        {
            var user =
                _context.Users.FirstOrDefault(u => u.Id == userId)
                ?? throw new UserNotFoundException("id", userId);

            var allConfigs = _context.Banks.ToList();

            return allConfigs.Where(config => !user.IsBankConnected(config.Name)).ToList();
        }

        public async Task<List<Balance>> GetBalancesAsync(
            List<string> accountIds,
            string token,
            string userId
        )
        {
            var results = new List<Balance>();

            foreach (var accountId in accountIds)
                try
                {
                    var balance = await GetBalanceAsync(accountId, token, userId)
                        .ConfigureAwait(false);
                    results.Add(balance);
                }
                catch (NotFoundException)
                {
                    results.Add(new Balance(userId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Untrackable exception occured while attempt of processing account #{accountId} :\n{ex.ToString()}"
                    );
                }

            return results;
        }

        public async Task<Balance> GetBalanceAsync(
            string accountId,
            string tinkAccessToken,
            string userId
        )
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                EnvConfig.TinkGetBalanceUri(accountId)
            );
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                tinkAccessToken
            );

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Tink API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}"
                );
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new AccountNotFoundException("id", accountId);

                throw new ExternalApiException("Tink", $"Status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(content);

            var available = doc.RootElement.GetProperty("balances").GetProperty("available");

            return new Balance(
                userId,
                available.GetProperty("unscaledValue").GetInt64(),
                available.GetProperty("scale").GetInt32(),
                doc.RootElement.GetProperty("accountId").GetString() ?? "",
                available.GetProperty("currencyCode").GetString() ?? ""
            );
        }

        public void ConnectMono()
        {
            throw new NotImplementedException("Has been not implemented yet");
        }

        public async Task ConnectOtherBankAsync(string userId, string tinkAccessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, EnvConfig.TinkListAccountUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                tinkAccessToken
            );

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            var accountsJson = doc.RootElement.GetProperty("accounts");

            var bank =
                _context.Banks.FirstOrDefault(b => b.Name == BankName.OTHER)
                ?? throw new BankNotFoundException("OTHER");

            var userAccounts = new List<BankAccount>();

            foreach (var account in accountsJson.EnumerateArray())
            {
                var id = account.GetProperty("id").GetString();
                var fullBankId = BankNameMapper.BankNameToIdMap[BankName.OTHER] + id;

                try
                {
                    var amount = account
                        .GetProperty("balances")
                        .GetProperty("booked")
                        .GetProperty("amount")
                        .GetProperty("value");

                    var unscaled = long.Parse(
                        amount.GetProperty("unscaledValue").GetString() ?? ""
                    );
                    var scale = int.Parse(amount.GetProperty("scale").GetString() ?? "");
                    var balance = unscaled * (long)Math.Pow(10, -scale);

                    userAccounts.Add(
                        new BankAccount
                        {
                            Id = fullBankId,
                            UserId = userId,
                            BankId = bank.Id ?? throw new BankNotFoundException("id"),
                            Balance = balance,
                            CurrencyScale = scale,
                        }
                    );
                }
                catch (Exception ex)
                {
                    throw new UnexpectedException(
                        "Error while parsing account balance",
                        (CustomException)ex
                    );
                }
            }

            await _context.BankAccounts.AddRangeAsync(userAccounts).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public string GetTinkAccessToken(string code = "")
        {
            var parameters = new Dictionary<string, string>()
            {
                { "grant_type", EnvConfig.TinkGrantType },
                { "code", code },
                { "client_id", EnvConfig.TinkClientId },
                { "client_secret", EnvConfig.TinkClientSecret },
            };

            var response = _httpClient
                .PostAsync(EnvConfig.TinkTokentUri, new FormUrlEncodedContent(parameters))
                .Result;
            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().Result;
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("access_token").GetString() ?? "";
        }

        public async Task<List<BankConfig>> GetBanksAsync() =>
            await _context.Banks.ToListAsync().ConfigureAwait(false);

        public async Task<BankConfig> GetBankByIdAsync(string id) =>
            await _context.Banks.FindAsync(id).ConfigureAwait(false);

        public async Task<BankConfig> AddBankAsync(BankConfig bank)
        {
            _context.Banks.Add(bank);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return bank;
        }

        public async Task UpdateBankAsync(string id, BankConfig bank)
        {
            if (bank == null || id != bank.Id)
                return;

            _context.Entry(bank).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BankExistsAsync(id).ConfigureAwait(false))
                    return;
                throw;
            }
        }

        public async Task<bool> DeleteBankAsync(string id)
        {
            var bank = await _context.Banks.FindAsync(id).ConfigureAwait(false);
            if (bank == null)
                return false;

            _context.Banks.Remove(bank);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        private async Task<bool> BankExistsAsync(string id) =>
            await _context.Banks.AnyAsync(b => b.Id == id).ConfigureAwait(false);
    }
}

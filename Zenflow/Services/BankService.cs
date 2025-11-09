using Zenflow.DTO;
using Zenflow.Enumirators;
using Zenflow.Filters;
using Zenflow.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using Zenflow.Helpers;
using static Zenflow.Helpers.ExceptionTypes;

namespace Zenflow.Services
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

            string[] tinkAccountIds = filter
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

            List<TinkTransaction> tinkTransactions = new List<TinkTransaction>();

            if (tinkAccountIds.Length > 0)
            {
                var parameters = new
                {
                    accounts = tinkAccountIds,
                    startDate = filter.DateFrom,
                    endDate = filter.DateTo,
                    minAmount,
                    maxAmount,
                };

                HttpRequestMessage request = new HttpRequestMessage(
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

                HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                TinkTransactionResponse? apiResponse = JsonSerializer.Deserialize<TinkTransactionResponse>(
                    content,
                    serializationOptions
                );

                tinkTransactions = apiResponse?.Results.Select(r => r.Transaction).ToList() ?? [];
            }

            foreach (TinkTransaction transaction in tinkTransactions)
            {
                transaction.UserId = filter.UserId;
            }

            return tinkTransactions;
        }

        private List<BankAccount> GetUserAccounts(string userId)
        {
            return [.. _context.BankAccounts.Where(a => a.UserId == userId)];
        }

        public List<BankConfig> ListBankConfigs(string userId)
        {



            User user =
                _context.Users.FirstOrDefault(u => u.Id == userId)
                ?? throw new UserNotFoundException("id", userId);


            List<BankConfig> allConfigs = _context.Banks.ToList();

            return allConfigs.Where(config => !user.IsBankConnected(config.Name)).ToList();
        }

        public async Task<List<Balance>> GetBalancesAsync(
            List<string> accountIds,
            string token,
            string userId
        )
        {
            List<Balance> results = new List<Balance>();

            foreach (string accountId in accountIds)
                try
                {
                    Balance balance = await GetBalanceAsync(accountId.Replace(BankNameMapper.BankNameToIdMap[BankName.OTHER],
                       "",
                       StringComparison.OrdinalIgnoreCase), token, userId)
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
                        $"Untrackable exception occured while attempt of processing account #{accountId} :\n{ex}"
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
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                EnvConfig.TinkGetBalanceUri(accountId)
            );
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                tinkAccessToken
            );

            HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Tink API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}"
                );
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new AccountNotFoundException("id", accountId);

                throw new ExternalApiException("Tink", $"Status: {response.StatusCode}");
            }

            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using JsonDocument doc = JsonDocument.Parse(content);

            return Balance.FromTinkJson(content, userId);
        }

        public void ConnectMono()
        {
            throw new NotImplementedException("Has been not implemented yet");
        }

        public async Task ConnectOtherBankAsync(string userId, string tinkAccessToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, EnvConfig.TinkListAccountUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                tinkAccessToken
            );

            HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using JsonDocument doc = JsonDocument.Parse(json);

            JsonElement accountsJson = doc.RootElement.GetProperty("accounts");

            User user = _context.Users.FirstOrDefault(user => user.Id == userId)
                ?? throw new UserNotFoundException("User", userId);

            BankConfig bank =
                _context.Banks.FirstOrDefault(b => b.Name == BankName.OTHER)
                ?? throw new BankNotFoundException("OTHER");

            List<BankAccount> userAccounts = new List<BankAccount>();

            foreach (JsonElement account in accountsJson.EnumerateArray())
            {
                BankAccount bankAccount = BankAccount.CreateFromTinkJson
                (
                    account,
                    userId,
                    bank.Id ?? throw new ArgumentException($"There isn't bankId for bank {bank.Name}")
                );

                userAccounts.Add(bankAccount);

            }

            await _context.BankAccounts.AddRangeAsync(userAccounts).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<(string, double)> GetTinkClientToken()
        {
            Dictionary<string, string> clientParameters = new Dictionary<string, string>()
    {
        { "grant_type", "client_credentials" },
        { "scope", "authorization:grant,user:create" },
        { "client_id", EnvConfig.TinkClientId },
        { "client_secret", EnvConfig.TinkClientSecret },
    };

            HttpResponseMessage clientResponse = await _httpClient
                .PostAsync(EnvConfig.TinkTokentUri, new FormUrlEncodedContent(clientParameters));
            clientResponse.EnsureSuccessStatusCode();


            using JsonDocument clientDoc = JsonDocument.Parse(await clientResponse.Content.ReadAsStringAsync());
            string clientToken = clientDoc.RootElement.GetProperty("access_token").GetString() ?? "";
            double expiresIn = clientDoc.RootElement.GetProperty("expires_in").GetInt16();

            return (clientToken, expiresIn);
        }

        public async Task<string> GetTinkLink(string userId, string clientToken)
        {

            // Create permanent user

            Dictionary<string, string> userParameters = new Dictionary<string, string>()
    {
        { "external_user_id", userId },
        { "market", "PL" },
        { "locale", "en_US" },
         { "retention_class", "permanent" },
    };

            HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Post, EnvConfig.TinkCreateUser)
            {
                Content = JsonContent.Create(userParameters)
            };

            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);


            try
            {
                HttpResponseMessage userResponse = await _httpClient.SendAsync(userRequest);
            }
            catch (HttpRequestException ex)
            {
                if (Equals(ex.StatusCode, StatusCodes.Status409Conflict))
                {
                    Console.WriteLine("User exists");
                }
                else
                {
                    throw new UnexpectedException();
                }

            }


            // Authorize User
            try
            {
                Dictionary<string, string> userAuthParameters = new Dictionary<string, string>()
    {
        { "external_user_id", userId },
         { "actor_client_id", EnvConfig.TinkActorClientId },
         { "id_hint", userId},
            { "scope", "authorization:read,authorization:grant,credentials:refresh,credentials:read,credentials:write,providers:read,user:read" },
    };

                HttpRequestMessage userAuthRequest = new HttpRequestMessage(HttpMethod.Post, EnvConfig.TinkUserAuthDelegateCode)
                {
                    Content = new FormUrlEncodedContent(userAuthParameters)
                };

                userAuthRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

                HttpResponseMessage userAuthResponse = await _httpClient.SendAsync(userAuthRequest);
                userAuthResponse.EnsureSuccessStatusCode();

                using JsonDocument userAuthDoc = JsonDocument.Parse(await userAuthResponse.Content.ReadAsStringAsync());
                string authCode = userAuthDoc.RootElement.GetProperty("code").GetString() ?? "";

                string scope = "accounts:read transactions:read user:read balances:read";

                string url = $"https://link.tink.com/1.0/transactions/connect-accounts" +
                 $"?client_id={EnvConfig.TinkClientId}" +
                 $"&redirect_uri={EnvConfig.TinkRedirectUri}" +
                 $"&authorization_code={authCode}" +
                 $"&scope={Uri.EscapeDataString(scope)}" +
                 $"&state={userId}" +
                 $"&market=PL"
                 +
                 $"&locale=en_US";

                return url;
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalApiException("Tink Delegate Access", ex.Message);
            }
        }

        public async Task<(string, double)> GetTinkUserAccessToken(string userId = "", string clientToken = "")
        {

            Dictionary<string, string> userAuthParameters = new Dictionary<string, string>()
        {
        { "external_user_id", userId },
            { "scope", "accounts:read,balances:read,transactions:read,provider-consents:read" },
    };

            HttpRequestMessage userAuthRequest = new HttpRequestMessage(HttpMethod.Post, EnvConfig.TinkUserAuthCode)
            {
                Content = new FormUrlEncodedContent(userAuthParameters)
            };

            userAuthRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

            HttpResponseMessage userAuthResponse = await _httpClient.SendAsync(userAuthRequest);
            userAuthResponse.EnsureSuccessStatusCode();

            using JsonDocument userAuthDoc = JsonDocument.Parse(await userAuthResponse.Content.ReadAsStringAsync());
            string authCode = userAuthDoc.RootElement.GetProperty("code").GetString() ?? "";


            // User token request using the auth code

            Dictionary<string, string> userTokenParameters = new Dictionary<string, string>()
    {
        { "grant_type", EnvConfig.TinkGrantType }, // e.g., "authorization_code"
        { "code", authCode },
        { "client_id", EnvConfig.TinkClientId },
        { "client_secret", EnvConfig.TinkClientSecret },
    };

            HttpRequestMessage userTokenRequest = new HttpRequestMessage(HttpMethod.Post, EnvConfig.TinkTokentUri)
            {
                Content = new FormUrlEncodedContent(userTokenParameters)
            };

            userTokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);



            HttpResponseMessage userTokenResponse = await _httpClient.SendAsync(userTokenRequest);
            userTokenResponse.EnsureSuccessStatusCode();

            using JsonDocument userTokentDoc = JsonDocument.Parse(await userTokenResponse.Content.ReadAsStringAsync());
            string userToken = userTokentDoc.RootElement.GetProperty("access_token").GetString() ?? "";

            double expiresIn = userTokentDoc.RootElement.GetProperty("expires_in").GetInt16();

            return (userToken, expiresIn);
        }

        public async Task<List<BankConfig>> GetBanksAsync()
        {
            return await _context.Banks.ToListAsync().ConfigureAwait(false);
        }

        public async Task<BankConfig> GetBankByIdAsync(string id)
        {
            return await _context.Banks.FindAsync(id).ConfigureAwait(false);
        }

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
            BankConfig? bank = await _context.Banks.FindAsync(id).ConfigureAwait(false);
            if (bank == null)
                return false;

            _context.Banks.Remove(bank);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        private async Task<bool> BankExistsAsync(string id)
        {
            return await _context.Banks.AnyAsync(b => b.Id == id).ConfigureAwait(false);
        }
    }
}

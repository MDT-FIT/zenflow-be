using FintechStatsPlatform.DTO;
using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using Mapster;
using Microsoft.IdentityModel.Protocols.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace FintechStatsPlatform.Services
{
    public class AnalyticService
    {
        private readonly string BaseApiLink = Environment.GetEnvironmentVariable("TINK_API_LINK") ?? "";
        private readonly BankService _bankService;
        private readonly FintechContext _context;
        private readonly HttpClient _httpClient;

        public AnalyticService(BankService bankService, FintechContext context, HttpClient httpClient)
        {
            _bankService = bankService;
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<Stats> getExpensesAsync(StatsFilter filter, string userAccessToken)
        {
            return await GetTypedTransactions(filter, userAccessToken);
        }

        public async Task<Stats> getIncome(StatsFilter filter, string userAccessToken)
        {
            return await GetTypedTransactions(filter, userAccessToken, false);
        }

        private async Task<Stats> GetTypedTransactions(StatsFilter filter, string userAccessToken, bool isExpenses=true)
        {
            // Prepare filters for different periods of time
            var tFilter1 = filter.ToTransactionFilter();
            var tFilter2 = filter.ToTransactionFilter();

            DateTime lastMonthDateTo = filter.DateFrom.AddDays(-1);
            DateTime lastMonthDateFrom = lastMonthDateTo.AddDays(-30);

            tFilter2.DateFrom = lastMonthDateFrom;
            tFilter2.DateTo = lastMonthDateTo;

            // Get transactions for the current period and 30-days-period before
            var currentExpensesRetrieval = _bankService.ListTransactionsAsync(tFilter1, userAccessToken);
            var lastMonthExpensesRetrieval = _bankService.ListTransactionsAsync(tFilter2, userAccessToken);

            await Task.WhenAll(currentExpensesRetrieval, lastMonthExpensesRetrieval); // Wait for both operations to complete

            // Get the resulting transactions
            var currentExpenses = currentExpensesRetrieval.Result.Where(t => isExpenses ? t.Amount < 0 : t.Amount > 0).ToArray();
            var lastMonthExpenses = lastMonthExpensesRetrieval.Result.Where(t => isExpenses ? t.Amount < 0 : t.Amount > 0).ToArray();

            long currentAmount = Math.Abs(currentExpenses.Sum(t => t.Amount));
            long lastMonthAmount = Math.Abs(lastMonthExpenses.Sum(t => t.Amount));

            int scale = 0;
            string currency = string.Empty;

            // Assume for now that scale and currency are the same throughout all transactions 
            if (currentExpenses.Length > 0)
            {
                scale = currentExpenses[0].Scale;
                currency = currentExpenses[0].Currency;
            }
            else if (lastMonthExpenses.Length > 0)
            {
                scale = lastMonthExpenses[0].Scale;
                currency = lastMonthExpenses[0].Currency;
            }

            double changePercentage = Math.Round(CalculateChangePercentage(currentAmount, lastMonthAmount), 2);

            return new Stats(filter.UserId, [.. filter.AccountIds], currentAmount, scale, currency, changePercentage);
        }

        private double CalculateChangePercentage(long current, long prev)
        {
            if (current == 0 && prev == 0)
            {
                return 0;
            }
            else if (current == 0)
            {
                return -100;
            }
            else if (prev == 0)
            {
                return 100;
            }

            return (double)(current - prev) / prev * 100;
        }

        public async Task<TinkCardResponse> getMostUsedCard(StatsFilter filter, string userAccessToken)
        {
            var tFilter = filter.ToTransactionFilter();

            var transactions = await _bankService.ListTransactionsAsync(tFilter, userAccessToken);
            var mostUsedAccountId = transactions?.GroupBy(t => t.AccountId)?.MaxBy(g => g.Count())?.Key;

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BaseApiLink}/data/v2/accounts/{mostUsedAccountId}"),
            };

            // Add header with user's access token
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

            var response = await _httpClient.SendAsync(requestMessage);

            // Throw exception in case request failed
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var mostUsedCard = JsonSerializer.Deserialize<TinkCardResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return mostUsedCard ?? new TinkCardResponse();
        }
    }
}

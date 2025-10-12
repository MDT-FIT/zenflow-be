using System.Net.Http.Headers;
using System.Text.Json;
using FintechStatsPlatform.DTO;
using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;

namespace FintechStatsPlatform.Services
{
    public class AnalyticService
    {
        private readonly string BaseApiLink =
            Environment.GetEnvironmentVariable("TINK_API_LINK") ?? "";
        private readonly BankService _bankService;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions serializationOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public AnalyticService(BankService bankService, HttpClient httpClient)
        {
            _bankService = bankService;
            _httpClient = httpClient;
        }

        public async Task<Stats> GetExpensesAsync(StatsFilter filter, string tinkAccessToken)
        {
            return await GetTypedTransactions(filter, tinkAccessToken).ConfigureAwait(false);
        }

        public async Task<Stats> GetIncomeAsync(StatsFilter filter, string userAccessToken)
        {
            return await GetTypedTransactions(filter, userAccessToken, false).ConfigureAwait(false);
        }

        private async Task<Stats> GetTypedTransactions(
            StatsFilter filter,
            string tinkAccessToken,
            bool isExpenses = true
        )
        {
            // Prepare filters for different periods of time
            var tFilter1 = filter.ToTransactionFilter();
            var tFilter2 = filter.ToTransactionFilter();

            DateTime lastMonthDateTo = filter.DateFrom.AddDays(-1);
            DateTime lastMonthDateFrom = lastMonthDateTo.AddDays(-30);

            tFilter2.DateFrom = lastMonthDateFrom;
            tFilter2.DateTo = lastMonthDateTo;

            int? minAmount = isExpenses ? null : 0;
            int? maxAmount = isExpenses ? 0 : null;

            // Get transactions for the current period and 30-days-period before
            var currentExpensesRetrieval = _bankService.ListTransactionsAsync(
                tFilter1,
                tinkAccessToken,
                minAmount,
                maxAmount
            );
            var lastMonthExpensesRetrieval = _bankService.ListTransactionsAsync(
                tFilter2,
                tinkAccessToken,
                minAmount,
                maxAmount
            );
            await Task.WhenAll(currentExpensesRetrieval, lastMonthExpensesRetrieval)
                .ConfigureAwait(false);

            // Get the resulting transactions
            var currentExpenses = await currentExpensesRetrieval.ConfigureAwait(false);
            var lastMonthExpenses = await lastMonthExpensesRetrieval.ConfigureAwait(false);

            long currentAmount = Math.Abs(currentExpenses.Sum(t => t.Amount));
            long lastMonthAmount = Math.Abs(lastMonthExpenses.Sum(t => t.Amount));

            // Assume for now that scale and currency are the same throughout all transactions
            (int scale, string currency) = GetCurrencyAndScale(currentExpenses, lastMonthExpenses);

            double changePercentage = Math.Round(
                CalculateChangePercentage(currentAmount, lastMonthAmount),
                2
            );

            return new Stats(
                filter.UserId,
                [.. filter.AccountIds],
                currentAmount,
                scale,
                currency,
                changePercentage
            );
        }

        private static (int, string) GetCurrencyAndScale(
            List<TinkTransaction> current,
            List<TinkTransaction> prev
        )
        {
            int scale = 0;
            string currency = string.Empty;

            if (current.Count > 0)
            {
                scale = current[0].Scale;
                currency = current[0].Currency;
            }
            else if (prev.Count > 0)
            {
                scale = prev[0].Scale;
                currency = prev[0].Currency;
            }

            return (scale, currency);
        }

        private static double CalculateChangePercentage(long current, long prev)
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

        public async Task<TinkCardResponse> GetMostUsedCardAsync(
            StatsFilter filter,
            string userAccessToken
        )
        {
            var tFilter = filter.ToTransactionFilter();

            var transactions = await _bankService
                .ListTransactionsAsync(tFilter, userAccessToken)
                .ConfigureAwait(false);

            foreach (var transaction in transactions)
            {
                Console.WriteLine(transaction.AccountId);
            }

            var mostUsedAccountId = transactions
                ?.GroupBy(t => t.AccountId)
                ?.MaxBy(g => g.Count())
                ?.Key;

            if (string.IsNullOrEmpty(mostUsedAccountId))
            {
                return new TinkCardResponse();
            }

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BaseApiLink}/data/v2/accounts/{mostUsedAccountId}"),
            };

            // Add header with user's access token
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                userAccessToken
            );

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            // Throw exception in case request failed
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Console.WriteLine("Raw account response form tink");
            Console.WriteLine(content);

            var mostUsedCard = JsonSerializer.Deserialize<TinkCardResponse>(
                content,
                serializationOptions
            );

            return mostUsedCard ?? new TinkCardResponse();
        }
    }
}

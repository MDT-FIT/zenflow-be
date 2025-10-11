using FintechStatsPlatform.DTO;
using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using Mapster;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace FintechStatsPlatform.Services
{
    public class AnalyticService
    {
        private readonly BankService _bankService;
        private readonly FintechContext _context;

        public AnalyticService(BankService bankService, FintechContext context)
        {
            _bankService = bankService;
            _context = context;
        }

        public async Task<Stats> getExpensesAsync(StatsFilter filter, string userAccessToken)
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
            var currentExpenses = currentExpensesRetrieval.Result.Where(t => t.Amount < 0).ToArray();
            var lastMonthExpenses = lastMonthExpensesRetrieval.Result.Where(t => t.Amount < 0).ToArray();

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

        public Stats getIncome(StatsFilter filter)
        {
            return new Stats("test");
        }

        public Card getMostUsedCard(StatsFilter filter)
        {
            return new Card();
        }
    }
}

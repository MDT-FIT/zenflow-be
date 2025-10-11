using FintechStatsPlatform.DTO;
using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using Mapster;

namespace FintechStatsPlatform.Services
{
    public class AnalyticService
    {
        private BankService _bankService;
        
        public AnalyticService(BankService bankService)
        {
            _bankService = bankService;
        }

        public async Task<Stats> getExpensesAsync(StatsFilter filter, string userAccessToken)
        {
            var transactionsFilter = filter.ToTransactionFilter(); // Could use AutoMapper as well

            var transactions = await _bankService.ListTransactionsAsync(transactionsFilter, userAccessToken);

            var expenses = transactions.Where(t => t.Amount < 0).ToArray();

            long totalAmount = expenses.Sum(t => t.Amount);
            int scale = 0;
            string currency = string.Empty;

            if (expenses.Length > 0)
            {
                scale = expenses[0].Scale;
                currency = expenses[0].Currency;
            }

            return new Stats(filter.UserId, [.. filter.AccountIds], totalAmount, scale, currency);
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

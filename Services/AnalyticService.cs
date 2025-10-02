using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using Mapster;

namespace FintechStatsPlatform.Services
{
    public class AnalyticService
    {
        public Stats getExpenses(StatsFilter filter) 
        {
            return new Stats();
        }

        public Stats getIncome(StatsFilter filter) 
        {
            return new Stats();
        }

        public  Card getMostUsedCard(StatsFilter filter)
        { 
            return new Card();
        }
    }
}

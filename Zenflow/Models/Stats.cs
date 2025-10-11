namespace FintechStatsPlatform.Models
{
    public class Stats : AbstractEntity
    {
        public Stats(string userId, List<string>? accountIds = null, long amount = 0, int scale = 2, string currency = "", double changePercentage = 0.0)
        {
            UserId = userId;
            AccountIds = accountIds ?? new List<string>();
            Amount = amount;
            Scale = scale;
            Currency = currency;
            ChangePercentage = changePercentage;
        }
        public string UserId { get; set; }

        public List<string> AccountIds { get; set; }

        public long Amount { get; set; }

        public int Scale { get; set; }

        public string Currency { get; set; }
        
        public double ChangePercentage { get; set; }
    }
}

namespace FintechStatsPlatform.Models
{
    public class Stats : AbstractEntity
    {
        public Stats(string userId, List<string>? accountIds = null, long amount = 0, int scale = 2, string currency = "")
        {
            UserId = userId;
            AccountIds = accountIds ?? new List<string>();
            Amount = amount;
            Scale = scale;
            Currency = currency;
        }
        public string UserId { get; set; }

        private List<string> AccountIds { get; set; }

        public long Amount { get; set; }

        public int Scale { get; set; }

        public string Currency { get; set; }
    }
}

namespace FintechStatsPlatform.Models
{
    public class Stats : AbstractEntity
    {
        private string userId;

        public string UserId { get { return userId; } set { userId = value; } }

        private string[] accountIds;

        private string[] AccountIds { get { return accountIds; } set { accountIds = value; } }

        private long amount;

        public long Amount { get { return amount; } set { amount = value; } }

        private int scale;

        public int Scale { get { return scale; } set { scale = value; } }

        private string currency;

        public string Currency { get { return currency; } set { currency = value; } }
    }
}

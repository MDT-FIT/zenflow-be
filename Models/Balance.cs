namespace FintechStatsPlatform.Models
{
    public class Balance : AbstractEntity
    {

        public string UserId { get; set; }

        private string AccountId { get; set; }

        public long Amount { get; set; }

        public int Scale { get; set; }

        public string Currency { get; set; }
    }
}
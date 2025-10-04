namespace FintechStatsPlatform.Models
{
    public class BankAccount : AbstractEntity
    {
        public string UserId { get; set; }
        public string BankId { get; set; }
        public int CurrencyScale { get; set; }
        public long Balance { get; set; }

        public User User { get; set; }
        public BankConfig Bank { get; set; }
    }
}
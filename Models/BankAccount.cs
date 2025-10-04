namespace FintechStatsPlatform.Models
{
    public class BankAccount : AbstractEntity
    {
        public string UserId { get; set; }
        public string BankId { get; set; }
        public decimal Balance { get; set; }

        public User User { get; set; }
        public Bank Bank { get; set; }
    }
}

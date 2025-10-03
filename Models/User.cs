namespace FintechStatsPlatform.Models
{
    public class User : AbstractEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string[] AccountIds { get; set; }

        public ICollection<BankAccount> Accounts { get; set; }

        public bool isBankConnected(BankName bank)
        {
            return 1 == 0;
        }
    }
}

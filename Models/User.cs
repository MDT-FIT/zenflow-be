using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class User : AbstractEntity
    {
        static public Dictionary<BankName,string> bankNamesKeyValuePairs = new Dictionary<BankName, string> { 
            { BankName.OTHER, "tink-" },{ BankName.MONO,"mono-"} 
        };
        
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public List<string> AccountIds { get; set; }

        public ICollection<BankAccount> Accounts { get; set; }
        
        public User()
        {
            AccountIds = new List<string>();
        }

        public bool isBankConnected(BankName queryBank)
        {
            if (AccountIds.Any(bank => bank.StartsWith(bankNamesKeyValuePairs[queryBank])))
                return true;
            return false;
        }
    }
}

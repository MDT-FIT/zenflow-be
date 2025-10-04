using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class User : AbstractEntity
    {
        static public Dictionary<BankName,string> BankNameMap = new Dictionary<BankName, string> { 
            { BankName.OTHER, "tink-" },{ BankName.MONO,"mono-"} 
        };
        
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> AccountIds { get; set; }
        public ICollection<BankAccount> Accounts { get; set; }
        public User(string id, string username = "", string email = "", List<string>? accountIds = null)
        {
            Id = id;
            AccountIds = accountIds ?? new List<string>();
            Username = username;
            Email = email;
        }

        public bool isBankConnected(BankName queryBank)
        {
            return AccountIds.Any(bank => bank.StartsWith(BankNameMap[queryBank]));
        }
    }
}

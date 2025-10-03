using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class User : AbstractEntity
    {
        static public Dictionary<BankName,string> bankNamesKeyValuePairs = new Dictionary<BankName, string> { 
            { BankName.OTHER, "tink-" },{ BankName.MONO,"mono-"} 
        };
        public User()
        {
            accountIds = new List<string>();
        }
        
        private string userName;

        public string Username { get { return userName; } set { userName = value; } }

        private string email;

        public string Email { get { return email; } set { email = value; } }

        private string passwordHash;

        public string PasswordHash { get { return passwordHash; } set { passwordHash = value; } }

        private List<string> accountIds;

        private List<string> AccountIds { get { return accountIds; } set { accountIds = value; } }

        public bool isBankConnected(BankName queryBank)
        {
            if (accountIds.Any(bank => bank.StartsWith(bankNamesKeyValuePairs[queryBank])))
                return true;
            return false;
        }
    }
}

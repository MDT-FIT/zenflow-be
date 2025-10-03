using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class User : AbstractEntity
    {
        private string userName;

        public string Username { get { return userName; } set { userName = value; } }

        private string email;

        public string Email { get { return email; } set { email = value; } }

        private string passwordHash;

        public string PasswordHash { get { return passwordHash; } set { passwordHash = value; } }

        private string[] accountIds;

        private string[] AccountIds { get { return accountIds; } set { accountIds = value; } }

        public bool isBankConnected(BankName bank)
        {
            return 1 == 0;
        }
    }
}

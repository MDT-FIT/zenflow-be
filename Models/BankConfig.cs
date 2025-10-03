using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class BankConfig : AbstractEntity
    {

        public BankConfig(string apiLink) 
        {
            this.apiLink = apiLink; 
        }

        private BankName bankName;

        public BankName BankName { get { return bankName; }  set { bankName = value; } }

        private string country;

        public string Country { get { return country; } set { country = value; } }

        private string apiLink;

        public string ApiLink { get { return apiLink; } set { apiLink = value; } }

        private string clientToken;

        public string ClientToken { get { return clientToken; } set { clientToken = value; } }

        private string logo;

        public string Logo { get { return logo; } set { logo = value; } }

    }
}

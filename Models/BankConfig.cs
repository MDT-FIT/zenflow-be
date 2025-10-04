using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class BankConfig : AbstractEntity
    {

        public BankConfig(BankName name = BankName.OTHER, string apiLink = "", string country = "", string cleintToken = "", string logo = "") 
        {
            ApiLink = apiLink; 
            Country = country;
            ClientToken = cleintToken;
            Logo = logo;
            BankName = name;
        }
        public BankName BankName { get; set; }
        public string Country { get; set; }
        public string ApiLink { get; set; }
        public string ClientToken { get; set; }
        public string Logo { get; set; }

        private bool isEnabled;

        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; } }

    }
}

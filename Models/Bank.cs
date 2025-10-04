using System.Xml;

namespace FintechStatsPlatform.Models
{
    public class Bank(string name = "", string logo = "", string apiLink = ""
        , string currency = "") : AbstractEntity
    {
        public string Name { get; set; } = name;
        public string Logo { get; set; } = logo;
        public string ApiLink { get; set; } = apiLink;
        public string Currency { get; set; } = currency;
        public List<BankAccount> BankAccounts { get; set; } = [];
    }
}

using System.Xml.Linq;
using Zenflow.Enumirators;

namespace Zenflow.Models
{
    public class BankConfig : AbstractEntity
    {
        public BankConfig()
        {
            ApiLink = "";
            Currency = "";
            Logo = "";
            Name = BankName.OTHER;
            IsEnabled = false;
        }

        public BankConfig(
            BankName name = BankName.OTHER,
            string apiLink = "",
            string currency = "",
            string logo = "",
            bool isEnabled = false
        )
        {
            ApiLink = apiLink;
            Currency = currency;
            Logo = logo;
            Name = name;
            IsEnabled = isEnabled;
        }

        public BankName Name { get; set; }
        public string Currency { get; set; }
        public string ApiLink { get; set; }
        public string Logo { get; set; }
        public bool IsEnabled { get; set; }
    }
}

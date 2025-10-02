using System.Xml;

namespace FintechStatsPlatform.Models
{
    public enum Country
    {
        Ukraine,
        Poland,
    }

    public class Bank : AbstractEntity
    {
        public string Name { get; set; }
        public XmlDocument Logo { get; set; }
        public Country Country { get; set; }
        public string ApiLink { get; set; }
        
        public ICollection<BankAccount> BankAccounts { get; set; }
    }
}

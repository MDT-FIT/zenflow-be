using FintechStatsPlatform.Enumirators;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace FintechStatsPlatform.Models
{
    public class User : AbstractEntity
    {
        public User()
        {
            Id = "";
            AccountIds = new List<string>();
            Username = "";
            Email = "";
        }
        public User(string id = "", string username = "", string email = "", List<string>? accountIds = null)
        {
            Id = id;
            AccountIds = accountIds ?? new List<string>();
            Username = username;
            Email = email;
        }
        public string? Username { get; set; }
        public string Email { get; set; }
        public List<string> AccountIds { get; set; }
        public ICollection<BankAccount> Accounts { get; set; }


        public bool IsBankConnected(BankName queryBank)
        {
            return AccountIds.Any(bank => bank.StartsWith(BankNameMapper.BankNameToIdMap[queryBank], StringComparison.OrdinalIgnoreCase));
        }
    }
}

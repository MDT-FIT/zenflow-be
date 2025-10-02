using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;

namespace FintechStatsPlatform.Services
{
    public class BankService
    {
        public  List<Transaction> listTransactions(TransactionFilter filter) 
        {
            return new List<Transaction>();
        }

        public List<BankConfig> listBankConfigs(string userdId)
        {
            return new List<BankConfig>();
        }

        public Balance getBalance(BalanceFilter filter)
        { 
            return new Balance();
        }

        public  void connectMono() { }

        public void connectOtherBank(string userdId) { }
    }
}

using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class Transaction : AbstractEntity
    {
        public DateTime Date { get; set; }
        public long Amount { get; set; }
        public TransactionType Type { get; set; }
        public bool Result { get; set; }
    }
}
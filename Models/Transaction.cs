using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class Transaction : AbstractEntity
    {
        private DateTime date;

        public DateTime Date { get { return date; } set { date = value; } }

        private long amount;

        public long Amount { get { return amount; } set { amount = value; } }

        private TransactionType type;

        public TransactionType Type { get { return type; } set { type = value; } }

        private bool result;

        public bool Result { get { return result; } set { result = value; } }

        private void replenish() { }

        private void withdraw() { }

    }
}

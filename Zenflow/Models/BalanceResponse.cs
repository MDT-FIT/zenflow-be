namespace FintechStatsPlatform.Models
{
    public class BalanceValue
    {
        public string CurrencyCode { get; set; }
        public int Scale { get; set; }
        public decimal UnscaledValue { get; set; }  // Для зручності конвертувати в decimal
    }

    public class BalancesDetails
    {
        public BalanceValue Available { get; set; }
        public BalanceValue Booked { get; set; }
        public BalanceValue CreditLimit { get; set; }
    }

    public class BalanceResponse
    {
        public string AccountId { get; set; }
        public BalancesDetails Balances { get; set; }
        public long Refreshed { get; set; } // timestamp
    }

}

using System.ComponentModel;

namespace FintechStatsPlatform.Enumirators
{
    public enum TransactionType
    {
        [Description("INCOME")]
        INCOME,

        [Description("EXPENSES")]
        EXPENSERS,

        [Description("TRANSFERS")]
        TRANSFERS
    }
}

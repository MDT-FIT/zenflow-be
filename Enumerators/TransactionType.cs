using System.ComponentModel;

namespace FintechStatsPlatform.Enumirators
{
    public enum TransactionType
    {
        [Description("INCOME")]
        INCOME,

        [Description("EXPENSERS")]
        EXPENSERS,

        [Description("TRANSFERS")]
        TRANSFERS
    }
}

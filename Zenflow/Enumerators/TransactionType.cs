using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FintechStatsPlatform.Enumirators
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
    {
        [Description("INCOME")]
        INCOME,

        [Description("EXPENSE")]
        EXPENSER,

        [Description("TRANSFER")]
        TRANSFER,

        [Description("DEFAULT")]
        DEFAULT
    }
}

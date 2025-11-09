using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Zenflow.Enumirators
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CardType
    {
        [Description("CHECKING")]
        CHECKING,

        [Description("SAVINGS")]
        SAVINGS,

        [Description("CREDIT_CARD")]
        CREDIT_CARD,
    }
}

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel;

namespace FintechStatsPlatform.Enumirators
{
    public enum BankName
    {
        [Description("OTHER")]
        OTHER,

        [Description("MONO")]
        MONO
    }

    public class BankNameMapper
    {
        private static readonly Dictionary<BankName, string> dictionary = new Dictionary<BankName, string>
        {
            { BankName.OTHER, "other" },
            { BankName.MONO, "mono" }
        };

        public static ValueConverter Map = new ValueConverter<BankName, string>(
                v => dictionary[v],
                v => dictionary.FirstOrDefault(kvp => kvp.Value == v).Key
            );

        static public Dictionary<BankName, string> BankNameToIdMap = new Dictionary<BankName, string> {
            { BankName.OTHER, "tink-" },{ BankName.MONO,"mono-"}
        };
    }
}

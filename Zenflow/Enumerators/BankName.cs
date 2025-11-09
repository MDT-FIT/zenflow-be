using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Zenflow.Enumirators
{
    public enum BankName
    {
        [Description("OTHER")]
        OTHER,

        [Description("MONO")]
        MONO,
    }

    public static class BankNameMapper
    {
        private static readonly Dictionary<BankName, string> dictionary = new Dictionary<
            BankName,
            string
        >
        {
            { BankName.OTHER, "other" },
            { BankName.MONO, "mono" },
        };

        public static readonly ValueConverter Map = new ValueConverter<BankName, string>(
            v => dictionary[v],
            v => dictionary.FirstOrDefault(kvp => kvp.Value == v).Key
        );

        public static readonly Dictionary<BankName, string> BankNameToIdMap = new Dictionary<
            BankName,
            string
        >
        {
            { BankName.OTHER, "tink-" },
            { BankName.MONO, "mono-" },
        };
    }
}

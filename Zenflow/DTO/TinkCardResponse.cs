using FintechStatsPlatform.Enumirators;
using System.Text.Json.Serialization;

namespace FintechStatsPlatform.DTO
{
    public class TinkCardResponse
    {
        public string Name { get; set; }

        public string TypeString { private get; init; }

        [JsonPropertyName("identifiers")]
        public TinkCardIdentifiers TinkCardIdentifiers { private get; set; }

        public string LastFour
        {
            get
            {
                return TinkCardIdentifiers.Pan[^4..];
            }
        }

        public string CardBin
        {
            get
            {
                return TinkCardIdentifiers.Pan[..4];
            }
        }

        public CardType CardType
        {
            get
            {
                return (CardType)Enum.Parse(typeof(CardType), TypeString);
            }
        }
    }

    public class TinkCardIdentifiers
    {
        public string Pan { get; set; }
    }
}

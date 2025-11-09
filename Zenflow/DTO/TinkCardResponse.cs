using System.Text.Json.Serialization;
using Zenflow.Enumirators;

namespace Zenflow.DTO
{
    public class TinkCardResponse
    {
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string TypeString { private get; init; }

        [JsonPropertyName("identifiers")]
        public TinkCardIdentifiers TinkCardIdentifiers { private get; set; }

        public string LastFour
        {
            get
            {
                string pan = TinkCardIdentifiers.Pan;
                return string.IsNullOrEmpty(pan) ? string.Empty : pan[^4..];
            }
        }

        public string CardBin
        {
            get
            {
                string pan = TinkCardIdentifiers.Pan;
                return string.IsNullOrEmpty(pan) ? string.Empty : pan[..4];
            }
        }

        public CardType CardType
        {
            get { return (CardType)Enum.Parse(typeof(CardType), TypeString); }
        }
    }

    public class TinkCardIdentifiers
    {
        [JsonPropertyName("pan")]
        public string Pan { get; set; }
    }
}

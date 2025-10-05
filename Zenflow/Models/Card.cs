using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class Card : AbstractEntity
    {
        public DateTime ExpireDate { get; set; }
        public CardType CardType { get; set; }
        public long Balance { get; set; }
        public string CardBin { get; set; }
        public string LastFour { get; set; }

    }
}

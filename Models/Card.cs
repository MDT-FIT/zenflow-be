using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class Card : AbstractEntity
    {
        private DateTime expireDate;

        public DateTime ExpireDate { get {  return expireDate; } set { expireDate = value; } }

        private CardType cardType;

        public CardType CardType { get { return cardType; } set { cardType = value; } }

        private long balance;

        public long Balance { get { return balance; } set { balance = value; } }

        private string cardBin;

        public string CardBin { get { return cardBin; } set {cardBin = value; } }

        private string lastFour;

        public string LastFour { get { return lastFour; } set { lastFour = value; } }

    }
}

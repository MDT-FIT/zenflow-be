using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class Account : AbstractEntity
    {

        private string bankId;

        public string BankId { get {  return bankId; } set { bankId = value; } }

        private string bankUserId;

        public string BankUserId { get { return bankUserId; } set {bankUserId = value; } }

        private string userId;

        public string UserId { get { return userId; } set { userId = value; } }

        private List<Card> cards;

        public List<Card> Cards { get { return cards; } set { cards = value; } }

        private long balance;

        public long Balance { get { return balance; } set { balance = value; } }

        public Card getCardInfo(string cardNumber, BankName bank)
        {
            return cards.Find(card => String.Concat(card.CardBin, card.LastFour).Equals(cardNumber));
        }
    }
}

using FintechStatsPlatform.Enumirators;

namespace FintechStatsPlatform.Models
{
    public class Account : AbstractEntity
    {
        public Account(string userId, string bankUserId = "", string bankId = "", long balance = 0, List<Card>? cards = null) 
        { 
            UserId = userId;
            BankUserId = bankUserId;
            BankId = bankId;
            Balance = balance;
            Cards = cards ?? new List<Card>();
        }

        public string BankId { get; set; }

        public string BankUserId { get; set; }

        public string UserId { get; set; }

        public List<Card> Cards { get; set; }

        public long Balance { get; set;  }

        public Card getCardInfo(string cardNumber, BankName bank)
        {
            return Cards.Find(card => String.Concat(card.CardBin, card.LastFour).Equals(cardNumber));
        }
    }
}

using Zenflow.Enumirators;
using System.Text.Json;
using static Zenflow.Helpers.ExceptionTypes;

namespace Zenflow.Models
{
    public class BankAccount : AbstractEntity
    {
        public string? UserId { get; set; }
        public string? BankId { get; set; }
        public int CurrencyScale { get; set; }
        public long Balance { get; set; }
        public User? User { get; set; }
        public BankConfig? Bank { get; set; }

        public static BankAccount CreateFromTinkJson(JsonElement accountJson, string userId, string bankId)
        {
            try
            {
                var id = accountJson.GetProperty("id").GetString();
                var fullBankId = BankNameMapper.BankNameToIdMap[BankName.OTHER] + id;
                var amount = accountJson
                    .GetProperty("balances")
                    .GetProperty("booked")
                    .GetProperty("amount")
                    .GetProperty("value");

                var unscaled = long.Parse(
                    amount.GetProperty("unscaledValue").GetString() ?? ""
                );
                var scale = int.Parse(amount.GetProperty("scale").GetString() ?? "");
                var balance = unscaled * (long)Math.Pow(10, -scale);

                return new BankAccount
                {
                    Id = fullBankId,
                    UserId = userId,
                    BankId = bankId,
                    Balance = balance,
                    CurrencyScale = scale,
                };
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(
                    "Error while parsing account balance",
                     (CustomException)ex
                );
            }
        }
    }
}

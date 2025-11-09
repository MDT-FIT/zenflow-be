using System.Text.Json;

namespace Zenflow.Models
{
    public class Balance : AbstractEntity
    {
        public string AccountId { get; set; }

        public string? UserId { get; set; }

        public long Amount { get; set; }

        public int Scale { get; set; }

        public string Currency { get; set; }

        public Balance(
            string userId,
            long amount = 0,
            int scale = 0,
            string currency = "N/A",
            string accountId = "N/A"
        )
        {
            AccountId = accountId;
            UserId = userId;
            Amount = amount;
            Scale = scale;
            Currency = currency;
        }

        public static Balance FromTinkJson(string jsonString, string userId)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement available = doc.RootElement.GetProperty("balances").GetProperty("available");

            return new Balance(
                userId,
                available.GetProperty("unscaledValue").GetInt64(),
                available.GetProperty("scale").GetInt32(),
                available.GetProperty("currencyCode").GetString() ?? "",
                doc.RootElement.GetProperty("accountId").GetString() ?? ""

            );
        }
    }
}

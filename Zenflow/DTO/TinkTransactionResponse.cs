using FintechStatsPlatform.Enumirators;
using FintechStatsPlatform.Migrations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Policy;
using System.Text.Json.Serialization;
using System.Transactions;

namespace FintechStatsPlatform.DTO
{
    public class TinkTransactionResponse
    {
        public List<TinkResult> Results { get; set; }
    }

    public class TinkResult
    {
        public TinkTransaction Transaction { get; set; }
    }

    public class TinkOriginalAmount
    {
        public string CurrencyCode { get; set; }
        public int Scale { get; set; }
        public long UnscaledValue { get; set; }
    }

    public class TinkTransaction
    {
        [JsonPropertyName("type")]
        public string CategoryTypeString { private get; init; }

        [JsonPropertyName("date")]
        public long DateUnixEpoch { private get; init; }

        [JsonPropertyName("currencyDenominatedOriginalAmount")]
        public TinkOriginalAmount OriginalAmount { private get; init; }

        [JsonPropertyName("pending")]
        public bool Pending { private get; init; }

        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("accountId")]
        public string AccountId { get; init; }
        
        public string UserId { get; set; }

        public long Amount
        {
            get
            {
                return OriginalAmount.UnscaledValue;
            }
        }

        public int Scale
        {
            get
            {
                return OriginalAmount.Scale;
            }
        }

        public string Currency
        {
            get
            {
                return OriginalAmount.CurrencyCode;
            }
        }

        public DateTime DateTime
        {
            get
            {
                // Convert unix epoch time to DateTime object
                var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(DateUnixEpoch);
                return dateTimeOffset.DateTime;
            }
        }

        public string Result
        {
            get
            {
                return Pending ? "SUCCESS" : "PENDING";
            }
        }

        public TransactionType TransactionType
        {
            get
            {
                return (TransactionType)Enum.Parse(typeof(TransactionType), CategoryTypeString);
            }
        }
    }
}

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
        [JsonPropertyName("categoryType")]
        public string CategoryTypeString { private get; init; }
        [JsonPropertyName("date")]
        public long DateUnixEpoch { private get; init; }
        [JsonPropertyName("currencyDenominatedOriginalAmount")]
        public TinkOriginalAmount OriginalAmount { private get; init; }
        public string Id { get; init; }
        public string AccountId { get; init; }
        public bool Pending { get; init; }

        [JsonIgnore] 
        public TransactionType CategoryType
        {
            get
            {
                return (TransactionType)Enum.Parse(typeof(TransactionType), CategoryTypeString);
            }
        }

        [JsonIgnore]
        public DateTime Date
        {
            get
            {
                // Convert unix epoch time to DateTime object
                var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(DateUnixEpoch);
                return dateTimeOffset.DateTime;
            }
        }

        [JsonIgnore]
        public long Amount
        {
            get
            {
                return OriginalAmount.UnscaledValue;
            }
        }

        [JsonIgnore]
        public string Currency
        {
            get
            {
                return OriginalAmount.CurrencyCode;
            }
        }

        [JsonIgnore]
        public int Scale
        {
            get
            {
                return OriginalAmount.Scale;
            }
        }
    }
}

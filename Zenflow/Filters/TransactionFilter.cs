using MapsterMapper;
using System.Text;

namespace FintechStatsPlatform.Filters
{
    public class TransactionFilter : AbstractFilter
    {

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; } = DateTime.Now;

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (accountIds != null && accountIds.Length != 0)
            {
                foreach (var accountId in accountIds)
                {
                    sb.Append($"accounts={accountId}&");
                }
            }

            long dateFromTimestamp = ((DateTimeOffset)DateFrom).ToUnixTimeMilliseconds();
            long dateToTimestamp = ((DateTimeOffset)DateTo).ToUnixTimeMilliseconds();

            sb.Append($"startDate={dateFromTimestamp}&");
            sb.Append($"endDate={dateToTimestamp}");

            return sb.ToString();
        }

    }
}

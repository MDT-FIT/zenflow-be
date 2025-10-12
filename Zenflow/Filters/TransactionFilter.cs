using System.Text;
using MapsterMapper;

namespace FintechStatsPlatform.Filters
{
    public class TransactionFilter : AbstractFilter
    {
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; } = DateTime.Now;

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (AccountIds != null && AccountIds.Count != 0)
            {
                foreach (var accountId in AccountIds)
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

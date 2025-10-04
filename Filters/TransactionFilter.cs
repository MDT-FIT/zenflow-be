using MapsterMapper;

namespace FintechStatsPlatform.Filters
{
    public class TransactionFilter : AbstractFilter
    {

        private DateTime dateFrom;
        public DateTime DateFrom { get { return dateFrom; } set { dateFrom = value; } }

        private DateTime dateTo;
        public DateTime DateTo { get { return dateTo; } set { dateTo = value; } }

    }
}
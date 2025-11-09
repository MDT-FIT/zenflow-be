using MapsterMapper;

namespace Zenflow.Filters
{
    public class BalanceFilter : AbstractFilter
    {
        private DateTime dateFrom;
        public DateTime DateFrom
        {
            get { return dateFrom; }
            set { dateFrom = value; }
        }

        private DateTime dateTo;
        public DateTime DateTo
        {
            get { return dateTo; }
            set { dateTo = value; }
        }
    }
}

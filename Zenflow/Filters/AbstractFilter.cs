using MapsterMapper;

namespace FintechStatsPlatform.Filters
{
    public abstract class AbstractFilter
    {
        protected string id;
        public string Id { get { return id; } set { id = value; } }

        protected string[] accountIds;

        public string[] AccountIds { get { return accountIds; } set { accountIds = value; } }

        protected string userId;

        public string UserId { get { return userId; } set { userId = value; } }


        protected IMapper mapper;

        protected IMapper Mapper { get { return mapper; } set { mapper = value; } }

    }
}

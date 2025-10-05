using MapsterMapper;

namespace FintechStatsPlatform.Filters
{
    public abstract class AbstractFilter
    {
        protected string id;
        protected string Id { get { return id; } set { id = value; } }

        protected string[] accountIds;

        protected string[] AccountIds { get { return accountIds; } set { accountIds = value; } }

        protected string userId;

        protected string UserId { get { return userId; } set { userId = value; } }


        protected IMapper mapper;

        protected IMapper Mapper { get { return mapper; } set { mapper = value; } }

    }
}

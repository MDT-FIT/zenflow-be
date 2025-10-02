using MapsterMapper;

namespace FintechStatsPlatform.Models
{
    public abstract class AbstractEntity
    {
        protected string id;

        protected string Id {get { return id;} set { id = value; } }

        protected IMapper mapper;

        protected IMapper Mapper { get { return mapper; } set { mapper = value; } }

        protected DateTime createdAt;

        protected DateTime CreatedAt { get { return createdAt; } set { createdAt = value; } }

        protected DateTime updatedAt;

        protected DateTime UpdatedAt { get {return updatedAt; } set { updatedAt = value; } }

    }
}

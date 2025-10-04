using MapsterMapper;

namespace FintechStatsPlatform.Models
{
    public abstract class AbstractEntity
    {
        public string Id { get; set; }
        public IMapper Mapper { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
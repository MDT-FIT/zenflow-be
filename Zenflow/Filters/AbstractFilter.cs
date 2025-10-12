using MapsterMapper;

namespace FintechStatsPlatform.Filters
{
    public abstract class AbstractFilter
    {
        public required string Id { get; set; }

        public List<string> AccountIds { get; } = new List<string>();
        public required string UserId { get; set; }

    }
}

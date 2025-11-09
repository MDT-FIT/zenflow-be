using MapsterMapper;

namespace Zenflow.Filters
{
    public abstract class AbstractFilter
    {
        public string? Id { get; set; }

        public List<string> AccountIds { get; set; } = new List<string>();
        public required string UserId { get; set; }
    }
}

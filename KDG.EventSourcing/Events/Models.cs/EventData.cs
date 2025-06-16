using System.Data;
namespace KDG.EventSourcing.Events.Models
{
    public struct EventData<T>
    {
        public required IDbTransaction Transaction { get; set; }
        public required EventEntities Entities { get; set; }
        public required string Name;
        public required NodaTime.Instant Occurred;
        public required Guid User;
        public required T Data;
    }
}

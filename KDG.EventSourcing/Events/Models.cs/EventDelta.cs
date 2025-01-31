namespace KDG.EventSourcing.Events.Models
{
    public class EventDelta
    {
        public required string Version { get; set; }
        public required List<ColumnDelta> Deltas { get; set; }
    }
}

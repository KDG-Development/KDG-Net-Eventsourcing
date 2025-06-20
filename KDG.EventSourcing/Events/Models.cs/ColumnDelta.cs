namespace KDG.EventSourcing.Events.Models
{
    public class ColumnDelta
    {
        public required string Field { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? OldDisplayValue { get; set; }
        public string? NewDisplayValue { get; set; }
    }
}

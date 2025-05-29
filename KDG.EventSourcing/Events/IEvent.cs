using KDG.EventSourcing.Attributes;
using KDG.EventSourcing.Events.Models;

namespace KDG.EventSourcing.Events
{
    public interface IEvent<I, O>
        where O : class
    {
        public EventEntities Entities(I data);
        public Task<O> Execute(EventData<I> data);
        public EventDelta Delta(O? original, O output);
        public Task<O?> GetOriginal(EventData<I> data);

        // Default implementation
        public EventDelta FindDeltas(O? original, O updated, ValueMapping<O> valueMappings, ValueMapping<O> displayValueMappings, string version)
        {
            var props = typeof(O).GetProperties()
                .Where(p => p.CanRead
                    && p.GetCustomAttributes(typeof(LogDeltaAttribute), true).Length != 0
                );
            var deltas = new List<ColumnDelta>();
            foreach (var prop in props)
            {
                var oldValue = original == null ? null : valueMappings.GetMappedValue(prop, original);
                var newValue = updated == null ? null : valueMappings.GetMappedValue(prop, updated);
                if (original == null
                    || (oldValue == null && newValue != null)
                    || (oldValue != null && !oldValue.Equals(newValue))
                )
                {
                    var newDelta = new ColumnDelta
                    {
                        Field = prop.Name,
                        OldValue = oldValue,
                        NewValue = newValue,
                    };
                    if (original != null)
                    {
                        newDelta.OldDisplayValue = displayValueMappings.ContainsKey(prop.Name)
                            ? displayValueMappings.GetMappedValue(prop, original)
                            : oldValue;
                    }
                    if (updated != null)
                    {
                        newDelta.NewDisplayValue = displayValueMappings.ContainsKey(prop.Name)
                            ? displayValueMappings.GetMappedValue(prop, updated)
                            : newValue;
                    }
                    deltas.Add(newDelta);
                }
            }
            return new EventDelta
            {
                Version = version,
                Deltas = deltas,
            };
        }
    }
}

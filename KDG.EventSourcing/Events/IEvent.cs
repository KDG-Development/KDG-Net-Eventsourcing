using KDG.EventSourcing.Attributes;

namespace KDG.EventSourcing.Events
{
    public interface IEvent<I, O>
        where O : class
    {
        public Models.EventEntities Entities(I data);
        public Task<O> Execute(Models.EventData<I> data);
        public Models.EventDelta Delta(O? original, O output);
        public Task<O?> GetOriginal(Models.EventData<I> data);

        // Default implementation
        public Models.EventDelta FindDeltas(O? original, O updated, Dictionary<string, Func<O, string>> displayValueMappings, string version)
        {
            var props = typeof(O).GetProperties()
                .Where(p => p.CanRead
                    && p.GetCustomAttributes(typeof(LogDeltaAttribute), true).Length != 0
                );
            var deltas = new List<Models.ColumnDelta>();
            foreach (var prop in props)
            {
                if (original == null
                    || (prop.GetValue(original) == null && prop.GetValue(updated) != null)
                    || (prop.GetValue(original) != null && !prop.GetValue(original)!.Equals(prop.GetValue(updated)))
                ){
                    var newDelta = new Models.ColumnDelta
                    {
                        Field = prop.Name,
                        OldValue = original != null ? prop.GetValue(original)?.ToString() : null,
                        NewValue = updated != null ? prop.GetValue(updated)?.ToString() : null,
                    };
                    if (original != null)
                    {
                        newDelta.OldDisplayValue = displayValueMappings.ContainsKey(prop.Name)
                            ? displayValueMappings[prop.Name](original)
                            : prop.GetValue(original)?.ToString();
                    }
                    if (updated != null)
                    {
                        newDelta.NewDisplayValue = displayValueMappings.ContainsKey(prop.Name)
                            ? displayValueMappings[prop.Name](updated)
                            : prop.GetValue(updated)?.ToString();
                    }
                    deltas.Add(newDelta);
                }
            }
            return new Models.EventDelta
            {
                Version = version,
                Deltas = deltas,
            };
        }
    }
}

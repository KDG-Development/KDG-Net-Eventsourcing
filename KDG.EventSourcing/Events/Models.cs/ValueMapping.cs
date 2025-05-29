using System.Reflection;

namespace KDG.EventSourcing.Events.Models
{
    public class ValueMapping<T> : Dictionary<string, Func<T, string>>
    {
        public string? GetMappedValue(PropertyInfo propInfo, T? obj)
        {
            if (obj == null)
            {
                return null;
            }
            if (ContainsKey(propInfo.Name))
            {
                return this[propInfo.Name](obj);
            }
            return propInfo.GetValue(obj)?.ToString();
        }
    }
}
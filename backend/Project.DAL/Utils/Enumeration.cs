using System.Collections.ObjectModel;
using System.Reflection;

namespace Project.DAL.Utils
{
    public abstract class Enumeration<TEnum>(Guid id, string name) : IEquatable<Enumeration<TEnum>>
        where TEnum : Enumeration<TEnum>
    {
        public Guid Id { get; protected init; } = id;
        public string Name { get; protected init; } = name;

        private static readonly Dictionary<Guid, TEnum> Enumerations = CreateEnumeration();

        public static TEnum? FromId(Guid id) => Enumerations.TryGetValue(id, out TEnum? enumeration) ? enumeration : default;
        public static TEnum? FromName(string name) => Enumerations.Values.SingleOrDefault(x => x.Name == name);
        public bool Equals(Enumeration<TEnum>? other)
        {
            if (other is null) return false;

            return GetType().Equals(other.GetType()) && Name == other.Name;
        }
        public override bool Equals(object? obj) => obj is Enumeration<TEnum> other && Equals(other);
        public override string ToString() => Name;
        private static Dictionary<Guid, TEnum> CreateEnumeration()
        {
            Type enumerationType = typeof(TEnum);

            IEnumerable<TEnum> fieldsForType = enumerationType
                .GetFields(
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.FlattenHierarchy)
                .Where(fieldInfo => enumerationType.IsAssignableFrom(fieldInfo.FieldType))
                .Select(fieldInfo => (TEnum)fieldInfo.GetValue(default)!);

            return fieldsForType.ToDictionary(x => x.Id);
        }
        public static IReadOnlyCollection<TEnum> GetValues() => new ReadOnlyCollection<TEnum>([.. Enumerations.Values]);
        public override int GetHashCode() => Id.GetHashCode();
    }
}
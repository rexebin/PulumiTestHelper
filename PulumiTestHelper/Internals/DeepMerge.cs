using System.Collections.Immutable;

namespace PulumiTestHelper.Internals;

internal static class DeepMerge
{
    internal static Dictionary<string, object> DeepMergeInto(
        this Dictionary<string, object> toBeMerged,
        Dictionary<string, object> baseDictionary)
    {
        var result = new Dictionary<string, object>(baseDictionary);

        foreach (var toBeMergedValue in toBeMerged)
        {
            var hasDuplicatedKey = baseDictionary.TryGetValue(toBeMergedValue.Key, out var toBeMergedItem);
            result[toBeMergedValue.Key] = hasDuplicatedKey switch
            {
                false => toBeMergedValue.Value,

                true when toBeMergedValue.Value is IEnumerable<Dictionary<string, object>> baseItemValue =>
                    DeepMergeInto(toBeMergedItem, baseItemValue),

                true when toBeMergedValue.Value is IEnumerable<Dictionary<string, string>> baseItemValue =>
                    DeepMergeInto(toBeMergedItem,
                        baseItemValue.Select(x => x.ToDictionary(d => d.Key, d => d.Value as object))),

                true when toBeMergedValue.Value is Dictionary<string, object> baseItemValue => DeepMergeInto(
                    toBeMergedItem,
                    baseItemValue),

                true when toBeMergedValue.Value is Dictionary<string, string> baseItemValue => DeepMergeInto(
                    toBeMergedItem,
                    baseItemValue.ToDictionary(x => x.Key, x => x.Value as object)),

                _ => result[toBeMergedValue.Key]
            };
        }

        return result;
    }

    private static IEnumerable<Dictionary<string, object>> DeepMergeInto(object? values1,
        IEnumerable<Dictionary<string, object>> values2)
    {
        return values1 switch
        {
            IEnumerable<Dictionary<string, object>> v1 => values2.Select((x, index) =>
            {
                var enumerable = v1.ToArray();
                return enumerable.Length > index ? x.DeepMergeInto(enumerable[index]) : x;
            }),

            IEnumerable<Dictionary<string, string>> v1 => values2.Select((x, index) =>
            {
                var enumerable = v1.ToArray();
                return enumerable.Length > index
                    ? x.DeepMergeInto(enumerable[index].ToDictionary(z => z.Key, z => z.Value as object))
                    : x;
            }),

            IEnumerable<ImmutableDictionary<string, object>> v1 => values2.Select((x, index) =>
            {
                var enumerable = v1.ToArray();
                return enumerable.ToArray().Length > index
                    ? x.DeepMergeInto(new Dictionary<string, object>(enumerable.ToArray()[index]))
                    : x;
            }),

            ImmutableArray<object> v1 => DeepMergeInto(values2.ToArray(), v1),

            _ => values2
        };
    }

    private static Dictionary<string, object> DeepMergeInto(object? values1,
        Dictionary<string, object> values2)
    {
        return values1 switch
        {
            ImmutableDictionary<string, object> v => DeepMergeInto(
                values2, new Dictionary<string, object>(v)),

            Dictionary<string, object> v => DeepMergeInto(
                values2, v),

            Dictionary<string, string> v => DeepMergeInto(
                values2, v.ToDictionary(x => x.Key, x => x.Value as object)),

            _ => values2
        };
    }


    private static IEnumerable<Dictionary<string, object>> DeepMergeInto(Dictionary<string, object>[] values1,
        ImmutableArray<object> values2)
    {
        var result = new List<Dictionary<string, object>>();
        var dict2 = values2.ToArray();
        foreach (var x in values1.Select((item, index) => (item, index)))
            if (dict2.Length > x.index && dict2[x.index] is ImmutableDictionary<string, object>)
            {
                var a = new Dictionary<string, object>((dict2[x.index] as ImmutableDictionary<string, object>)!);
                result.Add(x.item.DeepMergeInto(a));
            }
            else
            {
                result.Add(x.item);
            }

        return result;
    }

    internal static Dictionary<string, object> DeepMergeMany(this IEnumerable<Dictionary<string, object>> listOfDictionary)
    {
        return listOfDictionary.Aggregate(new Dictionary<string, object>(),
            (current, next) =>
                current.DeepMergeInto(next));
    }
}
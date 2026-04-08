using Limekuma.ScoreFilter;
using System.Collections.Frozen;
using System.Reflection;

namespace Limekuma.Utils;

internal static class ScoreFilterHelper
{
    private static readonly FrozenDictionary<string, IScoreFilter> Filters = BuildFilters();

    internal static IScoreFilter? GetFilterByTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return null;
        }

        foreach (string tag in tags)
        {
            if (Filters.TryGetValue(tag, out IScoreFilter? filter))
            {
                return filter;
            }
        }

        return null;
    }

    private static FrozenDictionary<string, IScoreFilter> BuildFilters()
    {
        Dictionary<string, IScoreFilter> filters = new(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Type> filterTypes = typeof(IScoreFilter).Assembly.GetTypes().Where(type =>
            type is { IsInterface: false, IsAbstract: false } &&
            typeof(IScoreFilter).IsAssignableFrom(type));

        foreach (Type type in filterTypes)
        {
            if (Activator.CreateInstance(type) is not IScoreFilter filter)
            {
                continue;
            }

            string? tag = type.GetCustomAttribute<ScoreFilterTagAttribute>()?.Tag;

            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            filters[tag] = filter;
        }

        return filters.ToFrozenDictionary();
    }
}
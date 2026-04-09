using Limekuma.Prober.Common;
using Limekuma.ScoreFilter;
using System.Collections.Frozen;
using System.Reflection;

namespace Limekuma.Utils;

internal static class ScoreFilterHelper
{
    private static readonly FrozenDictionary<string, (IScoreFilter, bool)> Filters = BuildFilters();

    internal static (Func<CommonRecord, bool>, bool) GetPredicateByTags(IEnumerable<string>? tags, string? condition)
    {
        List<IScoreFilter> selectedFilters = [];
        bool maskMutex = false;
        if (tags is not null)
        {
            foreach (string tag in tags)
            {
                if (!Filters.TryGetValue(tag, out (IScoreFilter, bool) filter_maskMutex))
                {
                    continue;
                }

                (IScoreFilter filter, bool maskMutexL) = filter_maskMutex;
                selectedFilters.Add(filter);
                maskMutex |= maskMutexL;
            }
        }

        List<Func<CommonRecord, bool>> predicates = selectedFilters.ConvertAll(filter => filter.GetFilter(condition));
        return (record => predicates.TrueForAll(predicate => predicate(record)), maskMutex);
    }

    private static FrozenDictionary<string, (IScoreFilter, bool)> BuildFilters()
    {
        Dictionary<string, (IScoreFilter, bool)> filters = new(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Type> filterTypes = typeof(IScoreFilter).Assembly.GetTypes().Where(type =>
            type is { IsInterface: false, IsAbstract: false } && typeof(IScoreFilter).IsAssignableFrom(type));

        foreach (Type type in filterTypes)
        {
            if (Activator.CreateInstance(type) is not IScoreFilter filter)
            {
                continue;
            }

            string? tag = type.GetCustomAttribute<ScoreFilterTagAttribute>()?.Tag;
            bool maskMutex = type.GetCustomAttribute<ScoreFilterTagAttribute>()?.MaskMutex ?? false;

            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            filters[tag] = (filter, maskMutex);
        }

        return filters.ToFrozenDictionary();
    }
}

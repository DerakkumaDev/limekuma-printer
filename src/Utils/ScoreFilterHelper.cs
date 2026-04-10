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
        if (tags is null)
        {
            return (_ => true, false);
        }

        IEnumerable<(IScoreFilter, bool)> selectedFilters =
            tags.Select(tag => Filters.GetValueOrDefault(tag));
        IEnumerable<Func<CommonRecord, bool>> predicates = selectedFilters.Select(x => x.Item1.GetFilter(condition));
        bool maskMutex = selectedFilters.Any(x => x.Item2);

        return (record => predicates.All(predicate => predicate(record)), maskMutex);
    }

    private static FrozenDictionary<string, (IScoreFilter, bool)> BuildFilters() => typeof(IScoreFilter).Assembly
        .GetTypes().Where(type =>
            type is { IsInterface: false, IsAbstract: false } && typeof(IScoreFilter).IsAssignableFrom(type))
        .Select(type => new
        {
            Type = type,
            Attribute = type.GetCustomAttribute<ScoreFilterTagAttribute>()
        }).Where(x => !string.IsNullOrWhiteSpace(x.Attribute?.Tag)).Select(x => new
        {
            x.Attribute!.Tag,
            Filter = Activator.CreateInstance(x.Type) as IScoreFilter,
            x.Attribute.MaskMutex
        }).Where(x => x.Filter is not null).ToFrozenDictionary(x => x.Tag!,
            x => (filter: x.Filter!, maskMutex: x.MaskMutex), StringComparer.OrdinalIgnoreCase);
}

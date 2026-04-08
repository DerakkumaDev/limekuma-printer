using Limekuma.ScoreProcesser;
using System.Collections.Frozen;
using System.Reflection;

namespace Limekuma.Utils;

internal static class ScoreProcesserHelper
{
    internal sealed record SelectedProcesser(IScoreProcesser Processer, bool MaskMutex, bool RequireSecondData);

    private static readonly FrozenDictionary<string, SelectedProcesser> Processers = BuildProcessers();

    internal static SelectedProcesser? GetProcesserByTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return null;
        }

        foreach (string tag in tags)
        {
            if (Processers.TryGetValue(tag, out SelectedProcesser? Processer))
            {
                return Processer;
            }
        }

        return null;
    }

    private static FrozenDictionary<string, SelectedProcesser> BuildProcessers()
    {
        Dictionary<string, SelectedProcesser> Processers = new(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Type> ProcesserTypes = typeof(IScoreProcesser).Assembly.GetTypes().Where(type =>
            type is { IsInterface: false, IsAbstract: false } &&
            typeof(IScoreProcesser).IsAssignableFrom(type));

        foreach (Type type in ProcesserTypes)
        {
            if (Activator.CreateInstance(type) is not IScoreProcesser Processer)
            {
                continue;
            }

            ScoreProcesserTagAttribute? tagAttribute = type.GetCustomAttribute<ScoreProcesserTagAttribute>();
            string? tag = tagAttribute?.Tag;

            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            Processers[tag] = new(Processer, tagAttribute?.MaskMutex ?? false, tagAttribute?.RequireSecondData ?? false);
        }

        return Processers.ToFrozenDictionary();
    }
}
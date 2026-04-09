using Limekuma.ScoreProcesser;
using System.Collections.Frozen;
using System.Reflection;

namespace Limekuma.Utils;

internal static class ScoreProcesserHelper
{
    private static readonly FrozenDictionary<string, SelectedProcesser> Processers = BuildProcessers();

    internal static SelectedProcesser? GetProcesserByTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return null;
        }

        foreach (string tag in tags)
        {
            if (Processers.TryGetValue(tag, out SelectedProcesser? processer))
            {
                return processer;
            }
        }

        return null;
    }

    private static FrozenDictionary<string, SelectedProcesser> BuildProcessers()
    {
        Dictionary<string, SelectedProcesser> processers = new(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Type> processerTypes = typeof(IScoreProcesser).Assembly.GetTypes().Where(type =>
            type is { IsInterface: false, IsAbstract: false } && typeof(IScoreProcesser).IsAssignableFrom(type));

        foreach (Type type in processerTypes)
        {
            if (Activator.CreateInstance(type) is not IScoreProcesser processer)
            {
                continue;
            }

            ScoreProcesserTagAttribute? tagAttribute = type.GetCustomAttribute<ScoreProcesserTagAttribute>();
            string? tag = tagAttribute?.Tag;

            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            processers[tag] = new(processer, tagAttribute?.MaskMutex ?? false,
                tagAttribute?.RequireSecondData ?? false);
        }

        return processers.ToFrozenDictionary();
    }

    internal sealed record SelectedProcesser(IScoreProcesser Processer, bool MaskMutex, bool RequireSecondData);
}

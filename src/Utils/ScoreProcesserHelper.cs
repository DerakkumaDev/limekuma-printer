using Limekuma.ScoreProcesser;
using System.Collections.Frozen;
using System.Reflection;

namespace Limekuma.Utils;

internal static class ScoreProcesserHelper
{
    private static readonly FrozenDictionary<string, SelectedProcesser> Processers = BuildProcessers();

    internal static SelectedProcesser? GetProcesserByTags(IEnumerable<string>? tags) => tags
        ?.Select(tag => Processers.GetValueOrDefault(tag)).FirstOrDefault(processer => processer is not null);

    private static FrozenDictionary<string, SelectedProcesser> BuildProcessers() => typeof(IScoreProcesser).Assembly
        .GetTypes().Where(type =>
            type is { IsInterface: false, IsAbstract: false } && typeof(IScoreProcesser).IsAssignableFrom(type))
        .Select(type => new
        {
            Type = type,
            Attribute = type.GetCustomAttribute<ScoreProcesserTagAttribute>()
        }).Where(x => !string.IsNullOrWhiteSpace(x.Attribute?.Tag)).Select(x => new
        {
            x.Attribute!.Tag,
            Processer = Activator.CreateInstance(x.Type) as IScoreProcesser,
            x.Attribute.MaskMutex,
            x.Attribute.RequireSecondData
        }).Where(x => x.Processer is not null).ToFrozenDictionary(x => x.Tag!,
            x => new SelectedProcesser(x.Processer!, x.MaskMutex, x.RequireSecondData),
            StringComparer.OrdinalIgnoreCase);

    internal sealed record SelectedProcesser(IScoreProcesser Processer, bool MaskMutex, bool RequireSecondData);
}

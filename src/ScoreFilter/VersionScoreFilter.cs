using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Frozen;

namespace Limekuma.ScoreFilter;

[ScoreFilterTag("version")]
public sealed class VersionScoreFilter : IScoreFilter
{
    public Func<CommonRecord, bool> GetFilter(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return _ => true;
        }

        if (!ConstantMap.VersionMap.TryGetValue(condition, out FrozenSet<string>? version))
        {
            return _ => true;
        }

        return x => version.Contains(x.Chart.Song.Genre);
    }
}

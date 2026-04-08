using Limekuma.Prober.Common;

namespace Limekuma.ScoreFilter;

[ScoreFilterTag("rank")]
public sealed class RankScoreFilter : IScoreFilter
{
    public Func<CommonRecord, bool> GetFilter(string? condition)
    {
        if (!Enum.TryParse(condition, out Ranks rank))
        {
            return _ => true;
        }

        return x => x.Rank >= rank;
    }
}
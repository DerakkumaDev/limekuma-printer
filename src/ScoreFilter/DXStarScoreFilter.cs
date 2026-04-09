using Limekuma.Prober.Common;

namespace Limekuma.ScoreFilter;

[ScoreFilterTag("dx_star")]
public sealed class DXScoreRankScoreFilter : IScoreFilter
{
    public Func<CommonRecord, bool> GetFilter(string? condition)
    {
        if (!int.TryParse(condition, out int dxScoreRank))
        {
            return _ => true;
        }

        return x => x.DXScoreRank >= dxScoreRank;
    }
}

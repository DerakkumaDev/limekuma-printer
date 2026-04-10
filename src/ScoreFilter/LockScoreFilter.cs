using Limekuma.Prober.Common;
using Limekuma.Utils;

namespace Limekuma.ScoreFilter;

[ScoreFilterTag("lock", true)]
public sealed class LockScoreFilter : IScoreFilter
{
    public Func<CommonRecord, bool> GetFilter(string? condition) => x =>
    {
        float sumScore = (x.Chart.Notes.Tap + x.Chart.Notes.Touch + x.Chart.Notes.Hold * 2 + x.Chart.Notes.Slide * 3 +
                          x.Chart.Notes.Break * 5) * 5;
        float minScore = Math.Min((1 - (sumScore - 1) / sumScore) * 100,
            x.Chart.Notes.Break > 0 ? 1f / x.Chart.Notes.Break / 2 : 101);
        (float minAcc, _, _) = ConstantMap.GetRatingFactors(x.Rank);
        float maxAcc = minAcc + minScore;
        return x.Achievements >= minAcc && x.Achievements < maxAcc;
    };
}

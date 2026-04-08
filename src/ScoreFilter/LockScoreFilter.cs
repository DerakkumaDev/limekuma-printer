using Limekuma.Prober.Common;
using Limekuma.Utils;

namespace Limekuma.ScoreFilter;

[ScoreFilterTag("lock", true)]
public sealed class LockScoreFilter : IScoreFilter
{
    public Func<CommonRecord, bool> GetFilter(string? condition) => x =>
    {
        float sumScore = 0;
        for (int i = 0; i < 5; i++)
        {
            (int notes, int weight) = i switch
            {
                0 => (x.Chart.Notes.Tap, 1),
                1 => (x.Chart.Notes.Hold, 2),
                2 => (x.Chart.Notes.Slide, 3),
                3 => (x.Chart.Notes.Touch, 1),
                4 => (x.Chart.Notes.Break, 5),
                _ => throw new IndexOutOfRangeException()
            };

            sumScore += notes * weight;
        }

        sumScore *= 5;
        float minScore = Math.Min((1 - ((sumScore - 1) / sumScore)) * 100, x.Chart.Notes.Break > 0 ? 1f / x.Chart.Notes.Break / 2 : 101);
        (float minAcc, _, _) = ConstantMap.RatingMap[x.Rank];
        float maxAcc = minAcc + minScore;
        return x.Achievements >= maxAcc && x.Achievements < minAcc;
    };
}
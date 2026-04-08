using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Models;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("std_dev")]
public sealed class StdDevScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records)
    {
        ImmutableArray<CommonRecord>.Builder ever = ImmutableArray.CreateBuilder<CommonRecord>(35);
        ImmutableArray<CommonRecord>.Builder current = ImmutableArray.CreateBuilder<CommonRecord>(15);
        foreach (CommonRecord record in records.OrderByDescending(x =>
        {
            double stdDev = 0;
            double fitLevel = x.Chart.LevelValue;
            if (Status.Shared.ChartStatus.TryGetValue(x.Chart.Song.Id.ToString(), out List<ChartState>? chartState))
            {
                ChartState chart = chartState[(int)x.Chart.Difficulty - 1];
                stdDev = chart.StandardDeviation;
                fitLevel = chart.FitLevel;
            }

            return x.DXRating * (1 + (stdDev / 10)) * (1 + (fitLevel / (fitLevel > 0 ? 10 : 1)));
        }).ThenByDescending(x => x.Chart.LevelValue).ThenByDescending(x => x.Achievements))
        {
            (record.Chart.Song.InCurrentGenre switch
            {
                true => current,
                false => ever
            }).Add(record);

            if (ever.Count >= 35 && current.Count >= 15)
            {
                break;
            }
        }

        return (ever.ToImmutable(), current.ToImmutable());
    }
}
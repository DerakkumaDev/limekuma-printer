using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("std_dev")]
public sealed class StdDevScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records)
    {
        CommonRecord[] rankedRecords = records.AsParallel().Select(record =>
            {
                double stdDev = 0;
                double fitLevel = record.Chart.LevelValue;
                if (Status.Shared.TryGetChartState(record.Chart.Song.Id, (int)record.Chart.Difficulty - 1,
                        out ChartState? chartState))
                {
                    stdDev = chartState.StandardDeviation;
                    fitLevel = chartState.FitLevel;
                }

                record.ExtraInfo = (float)stdDev;
                double score = record.DXRating * (1 + stdDev / 10) * (1 + fitLevel / (fitLevel > 0 ? 10 : 1));
                return (Record: record, Score: score);
            }).OrderByDescending(x => x.Score).ThenByDescending(x => x.Record.Chart.LevelValue)
            .ThenByDescending(x => x.Record.Achievements).Select(x => x.Record).ToArray();
        return rankedRecords.SplitTopBestsByQuota(35, 15);
    }
}

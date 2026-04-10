using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using System.Collections.Immutable;
using Status = Limekuma.Prober.DivingFish.Models.Status;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("fit_level", true)]
public sealed class FitLevelScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records)
    {
        if (records.Any(r => r.DXScore is 0 && (r.DXScoreRank > 0 || r.Rank > Ranks.A)))
        {
            throw new RpcException(new(StatusCode.PermissionDenied, "Mask enabled"));
        }

        ParallelQuery<CommonRecord> projectedRecords = records.AsParallel().Select(record =>
        {
            double fitLevel = record.Chart.LevelValue;
            if (Status.Shared.TryGetChartState(record.Chart.Song.Id, (int)record.Chart.Difficulty - 1,
                    out ChartState chartState))
            {
                fitLevel = chartState.FitLevel;
            }

            (_, float coefficient, _) = ConstantMap.GetRatingFactors(record.Rank);
            int rating = (int)(fitLevel * (record.Achievements > 100.5 ? 100.5 : record.Achievements) * coefficient);
            float level = (int)(fitLevel * 100) / 100f;
            return (CommonRecord)new()
            {
                Achievements = record.Achievements,
                DXRating = rating,
                Chart = new()
                {
                    LevelValue = level,
                    Difficulty = record.Chart.Difficulty,
                    Level = record.Chart.Level,
                    Notes = record.Chart.Notes,
                    Song = record.Chart.Song,
                    TotalDXScore = record.Chart.TotalDXScore
                },
                ComboFlag = record.ComboFlag,
                DXScore = record.DXScore,
                DXScoreRank = record.DXScoreRank,
                Rank = record.Rank,
                SyncFlag = record.SyncFlag,
                ExtraInfo = (float)fitLevel
            };
        });
        return projectedRecords.SplitTopBestsByQuota(35, 15);
    }
}

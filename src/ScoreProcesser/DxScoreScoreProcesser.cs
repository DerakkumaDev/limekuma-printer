using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("dx_score", true)]
public sealed class DxScoreScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records)
    {
        if (records.Any(r => r.DXScore is 0 && (r.DXScoreRank > 0 || r.Rank > Ranks.A)))
        {
            throw new RpcException(new(StatusCode.PermissionDenied, "Mask enabled"));
        }

        ParallelQuery<CommonRecord> projectedRecords = records.AsParallel().Select(record =>
        {
            float achievements = (float)record.DXScore / record.Chart.TotalDXScore * 101;
            (Ranks rank, float coefficient) = ConstantMap.ResolveRankAndCoefficient(achievements);

            int rating = (int)(record.Chart.LevelValue * achievements * coefficient);
            return (CommonRecord)new()
            {
                Achievements = achievements,
                DXRating = rating,
                Chart = record.Chart,
                ComboFlag = record.ComboFlag,
                DXScore = record.DXScore,
                DXScoreRank = record.DXScoreRank,
                Rank = rank,
                SyncFlag = record.SyncFlag
            };
        });
        return projectedRecords.SplitTopBestsByQuota(35, 15);
    }
}

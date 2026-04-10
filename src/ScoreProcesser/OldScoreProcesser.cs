using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("old", true)]
public sealed class OldScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records)
    {
        if (records.Any(r => r.DXScore is 0 && (r.DXScoreRank > 0 || r.Rank > Ranks.A)))
        {
            throw new RpcException(new(StatusCode.PermissionDenied, "Mask enabled"));
        }

        ParallelQuery<CommonRecord> projectedRecords = records.AsParallel().Select(record =>
        {
            (_, _, float oldCoefficient) = ConstantMap.GetRatingFactors(record.Rank);
            int rating = (int)(record.Achievements * (record.Achievements > 100.5 ? 100.5 : record.Achievements) *
                               oldCoefficient);
            return (CommonRecord)new()
            {
                Achievements = record.Achievements,
                DXRating = rating,
                Chart = record.Chart,
                ComboFlag = record.ComboFlag,
                DXScore = record.DXScore,
                DXScoreRank = record.DXScoreRank,
                Rank = record.Rank,
                SyncFlag = record.SyncFlag
            };
        });
        return projectedRecords.SplitTopBestsByQuota(25, 15);
    }
}

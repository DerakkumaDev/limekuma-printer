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

        ImmutableArray<CommonRecord>.Builder ever = ImmutableArray.CreateBuilder<CommonRecord>(25);
        ImmutableArray<CommonRecord>.Builder current = ImmutableArray.CreateBuilder<CommonRecord>(15);
        foreach (CommonRecord record in records.SortRecordForBests())
        {
            (_, _, float oldCoefficient) = ConstantMap.RatingMap[record.Rank];
            int rating = (int)(record.Achievements * (record.Achievements > 100.5 ? 100.5 : record.Achievements) *
                               oldCoefficient);
            CommonRecord newRecord = new()
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

            (record.Chart.Song.InCurrentGenre switch
            {
                true => current,
                false => ever
            }).Add(newRecord);

            if (ever.Count >= 25 && current.Count >= 15)
            {
                break;
            }
        }

        return (ever.ToImmutable(), current.ToImmutable());
    }
}

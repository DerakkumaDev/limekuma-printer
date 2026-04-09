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

        ImmutableArray<CommonRecord>.Builder ever = ImmutableArray.CreateBuilder<CommonRecord>(35);
        ImmutableArray<CommonRecord>.Builder current = ImmutableArray.CreateBuilder<CommonRecord>(15);
        foreach (CommonRecord record in records.SortRecordForBests())
        {
            float achievements = (float)record.DXScore / record.Chart.TotalDXScore * 101;
            Ranks rank = Ranks.D;
            float coefficient = 0;
            foreach ((Ranks rankKey, (float minAcc, float coefficientValue, _)) in ConstantMap.RatingMap)
            {
                if (record.Achievements < minAcc)
                {
                    continue;
                }

                rank = rankKey;
                coefficient = coefficientValue;
            }
            int rating = (int)(record.Chart.LevelValue * achievements * coefficient);
            CommonRecord newRecord = new()
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

            (record.Chart.Song.InCurrentGenre switch
            {
                true => current,
                false => ever
            }).Add(newRecord);

            if (ever.Count >= 35 && current.Count >= 15)
            {
                break;
            }
        }

        return (ever.ToImmutable(), current.ToImmutable());
    }
}
using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("fit_level", true)]
public sealed class FitLevelScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records)
    {
        if (records.Any(r => r.DXScore is 0 && (r.DXStar > 0 || r.Rank > Ranks.A)))
        {
            throw new RpcException(new(StatusCode.PermissionDenied, "Mask enabled"));
        }

        ImmutableArray<CommonRecord>.Builder ever = ImmutableArray.CreateBuilder<CommonRecord>(35);
        ImmutableArray<CommonRecord>.Builder current = ImmutableArray.CreateBuilder<CommonRecord>(15);
        foreach (CommonRecord record in records.SortRecordForBests())
        {
            double fitLevel = record.Chart.LevelValue;
            if (Limekuma.Prober.DivingFish.Models.Status.Shared.ChartStatus.TryGetValue(record.Chart.Song.Id.ToString(), out List<ChartState>? chartState))
            {
                fitLevel = chartState[(int)record.Chart.Difficulty - 1].FitLevel;
            }

            (_, float coefficient, _) = ConstantMap.RatingMap[record.Rank];
            int rating = (int)(fitLevel * (record.Achievements > 100.5 ? 100.5 : record.Achievements) * coefficient);
            float level = ((int)(fitLevel * 100)) / 100f;
            CommonRecord newRecord = new()
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
                DXStar = record.DXStar,
                Rank = record.Rank,
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
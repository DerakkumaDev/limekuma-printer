using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("steal", false, true)]
public sealed class StealScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records1p,
        IReadOnlyList<CommonRecord> records2p)
    {
        (ImmutableArray<CommonRecord> baseEver, ImmutableArray<CommonRecord> baseCurrent) =
            records1p.SplitTopBestsByQuota(35, 15);
        int everMin = baseEver.IsDefaultOrEmpty ? 0 : baseEver[^1].DXRating;
        int currentMin = baseCurrent.IsDefaultOrEmpty ? 0 : baseCurrent[^1].DXRating;

        bool handleType = records1p.Count > records2p.Count;
        IReadOnlyList<CommonRecord> processRecords = handleType ? records2p : records1p;
        IReadOnlyList<CommonRecord> controlRecords = handleType ? records1p : records2p;
        FrozenDictionary<(int SongId, CommonDifficulties Difficulty), CommonRecord> controlLookup =
            controlRecords.ToFrozenDictionary(x => (x.Chart.Song.Id, x.Chart.Difficulty));
        ParallelQuery<CommonRecord> selectedRecords = processRecords.AsParallel()
            .Where(processRecord => processRecord.Chart.Song.Type is not CommonSongTypes.Utage).Select(processRecord =>
            {
                if (!controlLookup.TryGetValue((processRecord.Chart.Song.Id, processRecord.Chart.Difficulty),
                        out CommonRecord? controlRecord))
                {
                    return null;
                }

                CommonRecord record = handleType ? processRecord : controlRecord;
                CommonRecord anotherRecord = handleType ? controlRecord : processRecord;
                CommonRecord selected = new()
                {
                    Achievements = record.Achievements,
                    Chart = record.Chart,
                    ComboFlag = record.ComboFlag,
                    DXRating = record.DXRating,
                    DXScore = record.DXScore,
                    DXScoreRank = record.DXScoreRank,
                    Rank = record.Rank,
                    SyncFlag = record.SyncFlag,
                    ExtraInfo = anotherRecord.DXRating
                };
                return selected;
            }).OfType<CommonRecord>();

        ImmutableArray<CommonRecord> current =
        [
            .. selectedRecords.Where(x => x.Chart.Song.InCurrentGenre).OrderByDescending(x => x.DXRating > currentMin)
                .ThenByDescending(x => x.DXRating - x.ExtraInfo).ThenByDescending(x => x.Chart.LevelValue)
                .ThenByDescending(x => x.Achievements).Take(15)
        ];
        ImmutableArray<CommonRecord> ever =
        [
            .. selectedRecords.Where(x => !x.Chart.Song.InCurrentGenre).OrderByDescending(x => x.DXRating > everMin)
                .ThenByDescending(x => x.DXRating - x.ExtraInfo).ThenByDescending(x => x.Chart.LevelValue)
                .ThenByDescending(x => x.Achievements).Take(35)
        ];

        return (ever, current);
    }
}

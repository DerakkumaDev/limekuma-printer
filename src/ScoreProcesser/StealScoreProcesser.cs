using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("steal", false, true)]
public sealed class StealScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records1p, IReadOnlyList<CommonRecord> records2p)
    {
        List<CommonRecord> everRecords = [];
        List<CommonRecord> currentRecords = [];
        int everMin = 0, currentMin = 0, everCount = 0, currentCount = 0;
        foreach (CommonRecord record in records1p.SortRecordForBests())
        {
            if (currentCount + everCount >= 50)
            {
                break;
            }

            if (record.Chart.Song.InCurrentGenre)
            {
                if (currentCount >= 15)
                {
                    continue;
                }

                currentMin = record.DXRating;
                ++currentCount;
            }

            if (everCount >= 35)
            {
                continue;
            }

            everMin = record.DXRating;
            ++everCount;
        }

        bool handleType = records1p.Count > records2p.Count;
        IReadOnlyList<CommonRecord> processRecords = handleType ? records2p : records1p;
        IReadOnlyList<CommonRecord> controlRecords = handleType ? records1p : records2p;
        foreach (CommonRecord processRecord in processRecords)
        {
            if (processRecord.Chart.Song.Type is CommonSongTypes.Utage)
            {
                continue;
            }

            CommonRecord? controlRecord = controlRecords.FirstOrDefault(x => x.Chart.Song.Id == processRecord.Chart.Song.Id && x.Chart.Difficulty == processRecord.Chart.Difficulty);
            if (controlRecord is null)
            {
                continue;
            }

            CommonRecord record = handleType ? processRecord : controlRecord;
            CommonRecord anotherRecord = handleType ? controlRecord : processRecord;
            record.ExtraInfo = anotherRecord.DXRating;
            (record.Chart.Song.InCurrentGenre switch
            {
                true => currentRecords,
                false => everRecords
            }).Add(record);
        }

        ImmutableArray<CommonRecord> current = [.. currentRecords.OrderByDescending(x => x.DXRating > currentMin).ThenByDescending(x => x.DXRating - x.ExtraInfo).ThenByDescending(x => x.Chart.LevelValue).ThenByDescending(x => x.Achievements).Take(15)];
        ImmutableArray<CommonRecord> ever = [.. everRecords.OrderByDescending(x => x.DXRating > everMin).ThenByDescending(x => x.DXRating - x.ExtraInfo).ThenByDescending(x => x.Chart.LevelValue).ThenByDescending(x => x.Achievements).Take(35)];

        return (ever, current);
    }
}
using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("coop", false, true)]
public sealed class CoopScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records1p, IReadOnlyList<CommonRecord> records2p)
    {
        IEnumerable<CommonRecord> records = records1p.Select(x =>
        {
            x.ExtraInfo = 0;
            return x;
        }).Union(records2p.Select(x =>
        {
            x.ExtraInfo = 1;
            return x;
        })).DistinctBy(x => (x.Chart.Song.Id, x.Chart.Difficulty));

        ImmutableArray<CommonRecord>.Builder ever = ImmutableArray.CreateBuilder<CommonRecord>(35);
        ImmutableArray<CommonRecord>.Builder current = ImmutableArray.CreateBuilder<CommonRecord>(15);
        foreach (CommonRecord record in records.SortRecordForBests())
        {
            (record.Chart.Song.InCurrentGenre switch
            {
                true => current.Count < 15 ? current : ever,
                false => ever.Count < 35 ? ever : ever
            }).Add(record);

            if (ever.Count >= 35 && current.Count >= 15)
            {
                break;
            }
        }

        return (ever.ToImmutable(), current.ToImmutable());
    }
}
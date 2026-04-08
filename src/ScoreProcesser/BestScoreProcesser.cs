using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("best")]
public sealed class BestScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records)
    {
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
using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("best")]
public sealed class BestScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records)
    {
        (ImmutableArray<CommonRecord>.Builder Ever, ImmutableArray<CommonRecord>.Builder Current) state = (
            ImmutableArray.CreateBuilder<CommonRecord>(35), ImmutableArray.CreateBuilder<CommonRecord>(15));
        (ImmutableArray<CommonRecord>.Builder Ever, ImmutableArray<CommonRecord>.Builder Current) rankedState = records
            .SortRecordForBests().Aggregate(state, static (acc, record) =>
            {
                if (acc.Ever.Count >= 35 && acc.Current.Count >= 15)
                {
                    return acc;
                }

                (record.Chart.Song.InCurrentGenre switch
                {
                    true => acc.Current.Count < 15 ? acc.Current : acc.Ever,
                    false => acc.Ever.Count < 35 ? acc.Ever : acc.Current
                }).Add(record);
                return acc;
            });
        ImmutableArray<CommonRecord>.Builder ever = rankedState.Ever;
        ImmutableArray<CommonRecord>.Builder current = rankedState.Current;
        return (ever.ToImmutable(), current.ToImmutable());
    }
}

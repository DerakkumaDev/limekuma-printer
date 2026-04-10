using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

[ScoreProcesserTag("coop", false, true)]
public sealed class CoopScoreProcesser : IScoreProcesser
{
    public (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records1p,
        IReadOnlyList<CommonRecord> records2p)
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
        return records.SplitTopBestsByQuota(35, 15);
    }
}

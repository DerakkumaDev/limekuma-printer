using Limekuma.Prober.Common;
using System.Collections.Immutable;

namespace Limekuma.Utils;

internal static class SortExtensions
{
    extension(IEnumerable<CommonRecord> records)
    {
        internal IOrderedEnumerable<CommonRecord> SortRecordForBests() => records.OrderByDescending(x => x.DXRating)
            .ThenByDescending(x => x.Chart.LevelValue).ThenByDescending(x => x.Achievements);

        internal IOrderedEnumerable<CommonRecord> SortRecordForList() => records.OrderByDescending(x => x.Achievements)
            .ThenByDescending(x => x.DXRating).ThenByDescending(x => x.Chart.LevelValue);

        internal (ImmutableArray<CommonRecord> Ever, ImmutableArray<CommonRecord> Current) SplitTopBestsByQuota(
            int everQuota, int currentQuota)
        {
            IOrderedEnumerable<CommonRecord> sorted = records.SortRecordForBests();
            ImmutableArray<CommonRecord> ever =
                [.. sorted.Where(record => !record.Chart.Song.InCurrentGenre).Take(everQuota)];
            ImmutableArray<CommonRecord> current =
                [.. sorted.Where(record => record.Chart.Song.InCurrentGenre).Take(currentQuota)];
            return (ever, current);
        }
    }
}

using Limekuma.Prober.Common;

namespace Limekuma.Utils;

internal static class SortExtensions
{
    extension(IEnumerable<CommonRecord> records)
    {
        internal IOrderedEnumerable<CommonRecord> SortRecordForBests() =>
            records.OrderByDescending(x => x.DXRating).ThenByDescending(x => x.Chart.LevelValue).ThenByDescending(x => x.Achievements);

        internal IOrderedEnumerable<CommonRecord> SortRecordForList() =>
            records.OrderByDescending(x => x.Achievements).ThenByDescending(x => x.DXRating).ThenByDescending(x => x.Chart.LevelValue);
    }
}
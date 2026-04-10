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
            ImmutableArray<CommonRecord>.Builder ever = ImmutableArray.CreateBuilder<CommonRecord>(everQuota);
            ImmutableArray<CommonRecord>.Builder current = ImmutableArray.CreateBuilder<CommonRecord>(currentQuota);
            foreach (CommonRecord record in records.SortRecordForBests())
            {
                if (record.Chart.Song.InCurrentGenre)
                {
                    if (current.Count < currentQuota)
                    {
                        current.Add(record);
                    }
                }
                else if (ever.Count < everQuota)
                {
                    ever.Add(record);
                }

                if (ever.Count >= everQuota && current.Count >= currentQuota)
                {
                    break;
                }
            }

            return (ever.ToImmutable(), current.ToImmutable());
        }
    }
}

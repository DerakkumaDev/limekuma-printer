using Limekuma.Prober.Common;

namespace Limekuma.Utils;

internal static class SortExtensions
{
    extension(List<CommonRecord> records)
    {
        internal void SortRecordForBests() => records.Sort((x, y) =>
        {
            int compare = y.DXRating.CompareTo(x.DXRating);
            if (compare is not 0)
            {
                return compare;
            }

            compare = y.LevelValue.CompareTo(x.LevelValue);
            if (compare is not 0)
            {
                return compare;
            }

            compare = y.Achievements.CompareTo(x.Achievements);
            if (compare is not 0)
            {
                return compare;
            }

            return 0;
        });

        internal void SortRecordForList() => records.Sort((x, y) =>
        {
            int compare = y.Achievements.CompareTo(x.Achievements);
            if (compare is not 0)
            {
                return compare;
            }

            compare = y.DXRating.CompareTo(x.DXRating);
            if (compare is not 0)
            {
                return compare;
            }

            compare = y.LevelValue.CompareTo(x.LevelValue);
            if (compare is not 0)
            {
                return compare;
            }

            return 0;
        });
    }
}
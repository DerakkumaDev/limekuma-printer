namespace Limekuma.Utils;

internal static class SortExtensions
{
    extension(List<Prober.DivingFish.Models.Record> records)
    {
        internal void SortRecord()
        {
            records.Sort((x, y) =>
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
        }
    }

    extension(List<Prober.Lxns.Models.Record> records)
    {
        internal void SortRecord()
        {
            records.Sort((x, y) =>
            {
                int compare;
                if (y.DXRating.HasValue && x.DXRating.HasValue)
                {
                    compare = y.DXRating.Value.CompareTo(x.DXRating);
                    if (compare is not 0)
                    {
                        return compare;
                    }
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
        }
    }
}
namespace Limekuma.Prober.Common;

public record CommonRecord : IComparer<CommonRecord>
{
    public required CommonChart Chart { get; init; }

    public required ComboFlags ComboFlag { get; init; }

    public required SyncFlags SyncFlag { get; init; }

    public required Ranks Rank { get; init; }

    public required double Achievements { get; init; }

    public required int DXScore { get; init; }

    public required int DXStar { get; init; }

    public required int DXRating { get; init; }

    public int Compare(CommonRecord? x, CommonRecord? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        int result = x.DXRating.CompareTo(y.DXRating);
        if (result is not 0)
        {
            return result;
        }

        result = x.Chart.LevelValue.CompareTo(y.Chart.LevelValue);
        if (result is not 0)
        {
            return result;
        }

        return x.Achievements.CompareTo(y.Achievements);
    }
}
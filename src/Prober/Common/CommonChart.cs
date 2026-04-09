namespace Limekuma.Prober.Common;

public class CommonChart
{
    public required CommonSong Song { get; init; }

    public required CommonDifficulties Difficulty { get; init; }

    public required int TotalDXScore { get; init; }

    public required string Level { get; init; }

    public required double LevelValue { get; init; }

    public required Notes Notes { get; init; }
}

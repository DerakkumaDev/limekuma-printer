namespace Limekuma.Prober.Common;

public record CommonRecord
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public required CommonDifficulties Difficulty { get; init; }

    public required ComboFlags ComboFlag { get; init; }

    public required SyncFlags SyncFlag { get; init; }

    public required Ranks Rank { get; init; }

    public required CommonSongTypes Type { get; init; }

    public required double Achievements { get; init; }

    public required int DXScore { get; init; }

    public required int DXRating { get; init; }

    public required int TotalDXScore { get; init; }

    public required double LevelValue { get; init; }

    public string AudioUrl => $"https://assets2.lxns.net/maimai/music/{Id % 10000}.mp3";

    public string JacketUrl => $"https://assets2.lxns.net/maimai/jacket/{Id % 10000}.png";
}
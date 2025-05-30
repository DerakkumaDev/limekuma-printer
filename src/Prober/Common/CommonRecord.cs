using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Common;

public record CommonRecord
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public required CommonDifficulties Difficulty { get; init; }

    public required ComboFlags ComboFlag { get; init; }

    public required SyncFlags SyncFlag { get; init; }

    public required Ranks Rank { get; init; }

    public required CommonSongTypes Type { get; init; }

    public required decimal Achievements { get; init; }

    public required int DXScore { get; init; }

    public required decimal DXRating { get; init; }

    public string AudioUrl => $"https://assets2.lxns.net/maimai/music/{Id}.mp3";

    public string JacketUrl => $"https://assets2.lxns.net/maimai/jacket/{Id}.png";
}

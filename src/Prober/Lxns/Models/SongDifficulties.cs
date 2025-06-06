using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record SongDifficulties
{
    [JsonPropertyName("standard")]
    public required List<SongDifficulty> Standard { get; init; }

    [JsonPropertyName("dx")]
    public required List<SongDifficulty> DX { get; init; }

    [JsonPropertyName("utage")]
    public List<SongDifficultyUtage>? Utage { get; init; }
}
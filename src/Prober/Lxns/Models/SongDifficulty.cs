using Limekuma.Prober.Lxns.Enums;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record SongDifficulty
{
    [JsonPropertyName("type")]
    public required SongTypes Type { get; init; }

    [JsonPropertyName("difficulty")]
    public required Difficulties Difficulty { get; init; }

    [JsonPropertyName("level")]
    public required string Level { get; init; }

    [JsonPropertyName("level_value")]
    public required double LevelValue { get; init; }

    [JsonPropertyName("note_designer")]
    public required string Charter { get; init; }

    [JsonPropertyName("version")]
    public required int Version { get; init; }

    [JsonPropertyName("notes")]
    public Notes? Notes { get; init; }
}

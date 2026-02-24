using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Enums;
using Limekuma.Utils;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public class SimpleRecord
{
    [JsonPropertyName("achievements")]
    public required double Achievements { get; init; }

    [JsonPropertyName("fc")]
    public required Union<ComboFlags, string> ComboFlag { get; init; }

    [JsonPropertyName("fs")]
    public required Union<SyncFlags, string> SyncFlag { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("level")]
    public required string Level { get; init; }

    [JsonPropertyName("level_index")]
    public required int DifficultyIndex { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("type")]
    public required SongTypes Type { get; init; }
}
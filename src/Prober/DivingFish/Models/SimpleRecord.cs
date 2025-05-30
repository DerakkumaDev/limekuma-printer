using DXKumaBot.Backend.Prober.Common;
using DXKumaBot.Backend.Prober.DivingFish.Enums;
using DXKumaBot.Backend.Utils;
using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public class SimpleRecord
{
    [JsonPropertyName("achievements")]
    public required decimal Achievements { get; init; }

    [JsonPropertyName("fc")]
    public required Optional<ComboFlags, string> ComboFlag { get; init; }

    [JsonPropertyName("fs")]
    public required Optional<SyncFlags, string> SyncFlag { get; init; }

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

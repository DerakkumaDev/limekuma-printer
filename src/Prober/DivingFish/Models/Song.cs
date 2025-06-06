using Limekuma.Prober.DivingFish.Enums;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record Song
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("type")]
    public required SongTypes Type { get; init; }

    [JsonPropertyName("ds")]
    public required List<double> LevelValues { get; init; }

    [JsonPropertyName("level")]
    public required List<string> Levels { get; init; }

    [JsonPropertyName("cids")]
    public required List<int> ChartIds { get; init; }

    [JsonPropertyName("charts")]
    public required List<Chart> Charts { get; init; }

    [JsonPropertyName("basic_info")]
    public required BasicInfo BasicInfo { get; init; }
}
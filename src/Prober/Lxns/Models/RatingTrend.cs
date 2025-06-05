using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record RatingTrend
{
    [JsonPropertyName("total")]
    public required int Total { get; init; }

    [JsonPropertyName("standard")]
    public required int Standard { get; init; }

    [JsonPropertyName("dx")]
    public required int DX { get; init; }

    [JsonPropertyName("date")]
    public required DateTimeOffset DateTime { get; init; }
}
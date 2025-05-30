using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public record Status
{
    [JsonPropertyName("charts")]
    public required List<ChartState> ChartStatus { get; init; }

    [JsonPropertyName("diff_data")]
    public required List<LevelState> LevelStatus { get; init; }
}

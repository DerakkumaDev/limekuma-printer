using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public record LevelState
{
    [JsonPropertyName("achievements")]
    public required decimal AverageAchievements { get; init; }

    [JsonPropertyName("dist")]
    public required List<decimal> RankDistribution { get; init; }

    [JsonPropertyName("fc_dist")]
    public required List<decimal> ComboDistribution { get; init; }
}

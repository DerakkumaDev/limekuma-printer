using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record LevelState
{
    [JsonPropertyName("achievements")]
    public required double AverageAchievements { get; init; }

    [JsonPropertyName("dist")]
    public required List<double> RankDistribution { get; init; }

    [JsonPropertyName("fc_dist")]
    public required List<double> ComboDistribution { get; init; }
}
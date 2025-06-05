using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record ChartState
{
    [JsonPropertyName("cnt")]
    public required int SampleSize { get; init; }

    [JsonPropertyName("diff")]
    public required string Level { get; init; }

    [JsonPropertyName("fit_diff")]
    public required double FitLevel { get; init; }

    [JsonPropertyName("avg")]
    public required double AverageAchievements { get; init; }

    [JsonPropertyName("avg_dx")]
    public required double AverageDXScore { get; init; }

    [JsonPropertyName("std_dev")]
    public required double StandardDeviation { get; init; }

    [JsonPropertyName("dist")]
    public required List<int> RankDistribution { get; init; }

    [JsonPropertyName("fc_dist")]
    public required List<int> ComboDistribution { get; init; }
}

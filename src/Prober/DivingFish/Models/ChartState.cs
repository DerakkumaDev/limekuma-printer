using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public record ChartState
{
    [JsonPropertyName("cnt")]
    public required int SampleSize { get; init; }

    [JsonPropertyName("diff")]
    public required string Level { get; init; }

    [JsonPropertyName("fit_diff")]
    public required decimal FitLevel { get; init; }

    [JsonPropertyName("avg")]
    public required decimal AverageAchievements { get; init; }

    [JsonPropertyName("avg_dx")]
    public required decimal AverageDXScore { get; init; }

    [JsonPropertyName("std_dev")]
    public required decimal StandardDeviation { get; init; }

    [JsonPropertyName("dist")]
    public required List<int> RankDistribution { get; init; }

    [JsonPropertyName("fc_dist")]
    public required List<int> ComboDistribution { get; init; }
}

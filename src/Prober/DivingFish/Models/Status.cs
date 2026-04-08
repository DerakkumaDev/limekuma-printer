using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record Status
{
    private readonly DateTimeOffset _pullTime = DateTimeOffset.Now;

    [JsonPropertyName("charts")]
    public required Dictionary<string, List<ChartState>> ChartStatus { get; init; }

    [JsonPropertyName("diff_data")]
    public required Dictionary<string, LevelState> LevelStatus { get; init; }

    public static Status Shared
    {
        get
        {
            if (field is null || DateTimeOffset.Now.AddHours(10).Date != field._pullTime.AddHours(10).Date)
            {
                DfResourceClient _resource = new();
                field = _resource.GetChartStstusAsync().Result;
            }

            return field;
        }
    }
}
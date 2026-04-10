using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record Status
{
    private readonly DateTimeOffset _pullTime = DateTimeOffset.Now;

    [JsonPropertyName("charts")]
    public required Dictionary<string, List<ChartState>> ChartStatus { get; init; }

    [JsonPropertyName("diff_data")]
    public required Dictionary<string, LevelState> LevelStatus { get; init; }

    public FrozenDictionary<string, ImmutableArray<ChartState>> FrozenChartStatus => field ??=
        ChartStatus.ToFrozenDictionary(pair => pair.Key, pair => pair.Value.ToImmutableArray(), StringComparer.Ordinal);

    public FrozenDictionary<string, LevelState> FrozenLevelStatus =>
        field ??= LevelStatus.ToFrozenDictionary(StringComparer.Ordinal);

    public static Status Shared
    {
        get
        {
            if (field is not null && DateTimeOffset.Now.AddHours(10).Date == field._pullTime.AddHours(10).Date)
            {
                return field;
            }

            DfResourceClient resource = new();
            field = resource.GetChartStstusAsync().Result;

            return field;
        }
    }

    public bool TryGetChartState(int songId, int difficultyIndex, out ChartState chartState)
    {
        chartState = null!;
        if (!FrozenChartStatus.TryGetValue(songId.ToString(), out ImmutableArray<ChartState> chartStates))
        {
            return false;
        }

        if (difficultyIndex >= chartStates.Length)
        {
            return false;
        }

        chartState = chartStates[difficultyIndex];
        return true;
    }
}

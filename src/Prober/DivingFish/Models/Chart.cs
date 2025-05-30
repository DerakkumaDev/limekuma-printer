using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public record Chart
{
    [JsonPropertyName("notes")]
    public required List<int> Notes { get; set; }

    [JsonPropertyName("charter")]
    public required string Charter { get; set; }
}

using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public record Bests
{
    [JsonPropertyName("sd")]
    public required List<Record> Ever { get; set; }

    [JsonPropertyName("dx")]
    public required List<Record> Current { get; set; }
}

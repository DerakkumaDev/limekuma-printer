using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Lxns.Models;

public record Notes
{
    [JsonPropertyName("total")]
    public required int Total { get; init; }

    [JsonPropertyName("tap")]
    public required int Tap { get; init; }

    [JsonPropertyName("hold")]
    public required int Hold { get; init; }

    [JsonPropertyName("slide")]
    public required int Slide { get; init; }

    [JsonPropertyName("touch")]
    public required int Touch { get; init; }

    [JsonPropertyName("break")]
    public required int Break { get; init; }
}
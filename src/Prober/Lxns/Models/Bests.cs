using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Lxns.Models;

public record Bests
{
    [JsonPropertyName("standard_total")]
    public required int EverTotal { get; set; }

    [JsonPropertyName("dx_total")]
    public required int CurrentTotal { get; set; }

    [JsonPropertyName("standard")]
    public required List<Record> Ever { get; set; }

    [JsonPropertyName("dx")]
    public required List<Record> Current { get; set; }
}

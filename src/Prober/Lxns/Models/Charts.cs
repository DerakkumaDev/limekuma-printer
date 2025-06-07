using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record Charts
{
    [JsonPropertyName("standard")]
    public required List<Chart> Standard { get; init; }

    [JsonPropertyName("dx")]
    public required List<Chart> DX { get; init; }

    [JsonPropertyName("utage")]
    public List<UtageChart>? Utage { get; init; }
}
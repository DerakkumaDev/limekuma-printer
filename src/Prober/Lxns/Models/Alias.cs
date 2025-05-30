using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Lxns.Models;

public record Alias
{
    [JsonPropertyName("song_id")]
    public required int Id { get; init; }

    [JsonPropertyName("aliases")]
    public required List<string> Aliases { get; init; }
}
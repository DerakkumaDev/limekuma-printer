using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record Version
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("version")]
    public required int VersionNumber { get; init; }
}
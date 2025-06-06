using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record CollectionGenre
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("genre")]
    public required string Genre { get; init; }
}
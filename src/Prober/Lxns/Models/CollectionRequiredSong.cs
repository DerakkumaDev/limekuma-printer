using DXKumaBot.Backend.Prober.Lxns.Enums;
using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Lxns.Models;

public record CollectionRequiredSong
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("type")]
    public required SongTypes Type { get; init; }

    [JsonPropertyName("completed")]
    public bool? Completed { get; init; }

    [JsonPropertyName("completed_difficulties")]
    public List<Difficulties>? CompletedDifficulties { get; init; }
}

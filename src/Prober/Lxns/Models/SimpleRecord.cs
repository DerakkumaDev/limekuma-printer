using DXKumaBot.Backend.Prober.Common;
using DXKumaBot.Backend.Prober.Lxns.Enums;
using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Lxns.Models;

public record SimpleRecord
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("song_name")]
    public required string Title { get; init; }

    [JsonPropertyName("level")]
    public required string Level { get; init; }

    [JsonPropertyName("level_index")]
    public required Difficulties Difficulty { get; init; }

    [JsonPropertyName("fc")]
    public ComboFlags? ComboFlag { get; init; }

    [JsonPropertyName("fs")]
    public SyncFlags? SyncFlag { get; init; }

    [JsonPropertyName("rate")]
    public Ranks Rank { get; init; }

    [JsonPropertyName("type")]
    public required SongTypes Type { get; init; }
}
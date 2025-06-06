using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record VoteResult
{
    [JsonPropertyName("music_id")]
    public required string Id { get; init; }

    [JsonPropertyName("total_vote")]
    public required int TotalVote { get; init; }

    [JsonPropertyName("down_vote")]
    public required int DownVote { get; init; }
}
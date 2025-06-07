using Limekuma.Utils;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record UtageChart : Chart
{
    [JsonPropertyName("kanji")]
    public required string Kanji { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("is_buddy")]
    public required bool IsBuddy { get; init; }

    [JsonPropertyName("notes")]
    public new Optional<Notes, BuddyNotes>? Notes { get; init; }
}
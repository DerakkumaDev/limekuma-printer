using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns.Enums;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record CollectionRequired
{
    [JsonPropertyName("difficulties")]
    public List<Difficulties>? Difficulties { get; init; }

    [JsonPropertyName("rate")]
    public Ranks? Rank { get; init; }

    [JsonPropertyName("fc")]
    public ComboFlags? ComboFlag { get; init; }

    [JsonPropertyName("fs")]
    public SyncFlags? SyncFlag { get; init; }

    [JsonPropertyName("songs")]
    public List<CollectionRequiredSong>? Songs { get; init; }

    [JsonPropertyName("completed")]
    public bool? Completed { get; init; }
}

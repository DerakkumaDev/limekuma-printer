using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Lxns.Models;

public record Song
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("artist")]
    public required string Artist { get; init; }

    [JsonPropertyName("genre")]
    public required string Genre { get; init; }

    [JsonPropertyName("bpm")]
    public required int BPM { get; init; }

    [JsonPropertyName("map")]
    public string? Map { get; init; }

    [JsonPropertyName("version")]
    public required int Version { get; init; }

    [JsonPropertyName("rights")]
    public string? Rights { get; init; }

    [JsonPropertyName("disabled")]
    public bool? Disabled { get; init; } = false;

    [JsonPropertyName("difficulties")]
    public required SongDifficulties Difficulties { get; init; }
    public string AudioUrl => $"https://assets2.lxns.net/maimai/music/{Id}.mp3";
    public string JacketUrl => $"https://assets2.lxns.net/maimai/jacket/{Id}.png";
}

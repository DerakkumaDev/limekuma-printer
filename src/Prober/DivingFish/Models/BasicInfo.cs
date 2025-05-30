using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public record BasicInfo
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("artist")]
    public required string Artist { get; set; }

    [JsonPropertyName("genre")]
    public required string Genre { get; set; }

    [JsonPropertyName("bpm")]
    public required int Bpm { get; set; }

    [JsonPropertyName("release_date")]
    public required string ReleaseDate { get; set; }

    [JsonPropertyName("from")]
    public required string From { get; set; }

    [JsonPropertyName("is_new")]
    public required bool InCurrentVersion { get; set; }
}

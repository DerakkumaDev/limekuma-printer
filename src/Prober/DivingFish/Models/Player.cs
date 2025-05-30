using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public record Player
{
    [JsonPropertyName("additional_rating")]
    public required int ClassRank { get; set; }

    [JsonPropertyName("nickname")]
    public required string Name { get; set; }

    [JsonPropertyName("plate")]
    public required string PlateName { get; set; }

    [JsonPropertyName("rating")]
    public required int Rating { get; set; }

    [JsonPropertyName("records")]
    public required List<Record> Records { get; set; }

    [JsonPropertyName("username")]
    public required string Account { get; set; }
}

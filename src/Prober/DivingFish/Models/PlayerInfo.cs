using DXKumaBot.Backend.Prober.Common;
using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public record PlayerInfo
{
    [JsonPropertyName("username")]
    public required string Account { get; set; }

    [JsonPropertyName("rating")]
    public required int Rating { get; set; }

    [JsonPropertyName("additional_rating")]
    public required int ClassRank { get; set; }

    [JsonPropertyName("nickname")]
    public required string Name { get; set; }

    [JsonPropertyName("plate")]
    public required string PlateName { get; set; }

    [JsonPropertyName("charts")]
    public required Bests Bests { get; set; }

    [JsonPropertyName("user_general_data")]
    public required object? UserGeneralData { get; set; }

    public static implicit operator CommonUser(PlayerInfo player)
    {
        return new()
        {
            Name = player.Name,
            Rating = player.Rating,
            TrophyColor = "normal",
            TrophyText = "新人出道",
            ClassRank = 0,
            CourseRank = player.ClassRank,
            IconId = 101,
            FrameId = 558001,
            PlateId = 458001,
        };
    }
}

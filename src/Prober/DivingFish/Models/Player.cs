using Limekuma.Prober.Common;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record Player
{
    [JsonPropertyName("username")]
    public required string Account { get; set; }

    [JsonPropertyName("rating")]
    public required int Rating { get; set; }

    [JsonPropertyName("additional_rating")]
    public required CommonCourseRank ClassRank { get; set; }

    [JsonPropertyName("nickname")]
    public required string Name { get; set; }

    [JsonPropertyName("plate")]
    public required string PlateName { get; set; }

    [JsonPropertyName("charts")]
    public required Bests Bests { get; set; }

    [JsonPropertyName("user_general_data")]
    public required object? UserGeneralData { get; set; }

    public static implicit operator CommonUser(Player player) =>
        new()
        {
            Name = player.Name,
            Rating = player.Rating,
            TrophyColor = TrophyColor.Normal,
            TrophyText = "なかよしmai友～！",
            ClassRank = Common.ClassRank.B5,
            CourseRank = player.ClassRank,
            IconId = 458001,
            FrameId = 558001,
            PlateId = 458001
        };
}
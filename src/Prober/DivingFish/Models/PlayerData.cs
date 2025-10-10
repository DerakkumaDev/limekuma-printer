using Limekuma.Prober.Common;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record PlayerData
{
    [JsonPropertyName("additional_rating")]
    public required CommonCourseRank ClassRank { get; set; }

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

    public static implicit operator CommonUser(PlayerData player) =>
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
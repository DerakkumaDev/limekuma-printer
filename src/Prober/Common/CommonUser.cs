namespace Limekuma.Prober.Common;

public record CommonUser
{
    public required string Name { get; set; }

    public required int Rating { get; set; }

    public required TrophyColor TrophyColor { get; set; }

    public required string TrophyText { get; set; }

    public required CommonCourseRank CourseRank { get; set; }

    public required ClassRank ClassRank { get; set; }

    public required int IconId { get; set; }

    public required int PlateId { get; set; }

    public required int FrameId { get; set; }

    public int RatingLevel => Rating switch
    {
        < 1000 => 1,
        < 2000 => 2,
        < 4000 => 3,
        < 7000 => 4,
        < 10000 => 5,
        < 12000 => 6,
        < 13000 => 7,
        < 14000 => 8,
        < 14500 => 9,
        < 15000 => 10,
        > 14999 => 11
    };

    public string IconUrl => $"https://assets2.lxns.net/maimai/icon/{IconId}.png";

    public string PlateUrl => $"https://assets2.lxns.net/maimai/plate/{PlateId}.png";
}
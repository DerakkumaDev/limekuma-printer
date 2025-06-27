using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns.Enums;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record Player
{
    public LxnsDeveloperClient? Client { get; internal set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("rating")]
    public required int Rating { get; set; }

    [JsonPropertyName("friend_code")]
    public required long FriendCode { get; set; }

    [JsonPropertyName("trophy")]
    public Trophy? Trophy { get; set; }

    [JsonPropertyName("trophy_name")]
    public string? TrophyName { get; set; }

    [JsonPropertyName("course_rank")]
    public required CourseRank CourseRank { get; set; }

    [JsonPropertyName("class_rank")]
    public required ClassRank ClassRank { get; set; }

    [JsonPropertyName("star")]
    public required int Star { get; set; }

    [JsonPropertyName("icon")]
    public Icon? Icon { get; set; }

    [JsonPropertyName("name_plate")]
    public NamePlate? NamePlate { get; set; }

    [JsonPropertyName("frame")]
    public Frame? Frame { get; set; }

    [JsonPropertyName("upload_time")]
    public DateTimeOffset? UploadTime { get; set; }

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

    public static implicit operator CommonUser(Player player) =>
        new()
        {
            Name = player.Name,
            Rating = player.Rating,
            TrophyColor = player.Trophy?.Color ?? TrophyColor.Normal,
            TrophyText = player.Trophy?.Name ?? "新人出道",
            ClassRank = player.ClassRank,
            CourseRank = (CommonCourseRank)(player.CourseRank < CourseRank.Shinshodan ? player.CourseRank : player.CourseRank - 1),
            IconId = player.Icon?.Id ?? 101,
            FrameId = player.Frame?.Id ?? 200502,
            PlateId = player.NamePlate?.Id ?? 101
        };

    public async Task<Record> GetBestAsync(int id, Difficulties difficulty, SongTypes type,
        CancellationToken cancellationToken = default) =>
        await Client!.GetBestAsync(FriendCode, id, difficulty, type, cancellationToken);

    public async Task<Record> GetBestAsync(string title, Difficulties difficulty, SongTypes type,
        CancellationToken cancellationToken = default) =>
        await Client!.GetBestAsync(FriendCode, title, difficulty, type, cancellationToken);

    public async Task<Bests> GetBestsAsync(CancellationToken cancellationToken = default) =>
        await Client!.GetBestsAsync(FriendCode, cancellationToken);

    public async Task<Bests> GetAllPerfectBestsAsync(CancellationToken cancellationToken = default) =>
        await Client!.GetAllPerfectBestsAsync(FriendCode, cancellationToken);

    public async Task<List<Record>> GetRecordsAsync(int id, SongTypes type,
        CancellationToken cancellationToken = default) =>
        await Client!.GetRecordsAsync(FriendCode, id, type, cancellationToken);

    public async Task<List<Record>> GetRecordsAsync(string title, SongTypes type,
        CancellationToken cancellationToken = default) =>
        await Client!.GetRecordsAsync(FriendCode, title, type, cancellationToken);

    public async Task UploadRecordsAsync(List<Record> records, CancellationToken cancellationToken = default) =>
        await Client!.UploadRecordsAsync(FriendCode, records, cancellationToken);

    public async Task<List<Record>> GetRecentsAsync(CancellationToken cancellationToken = default) =>
        await Client!.GetRecentsAsync(FriendCode, cancellationToken);

    public async Task<List<SimpleRecord>> GetAllRecordsAsync(CancellationToken cancellationToken = default) =>
        await Client!.GetAllRecordsAsync(FriendCode, cancellationToken);

    public async Task<List<RatingTrend>> GetDXRatingTrendAsync(int? version = null,
        CancellationToken cancellationToken = default) =>
        await Client!.GetDXRatingTrendAsync(FriendCode, version, cancellationToken);

    public async Task<List<Record>> GetHistoryAsync(int id, SongTypes type, Difficulties difficulty,
        CancellationToken cancellationToken = default) =>
        await Client!.GetHistoryAsync(FriendCode, id, type, difficulty, cancellationToken);

    public async Task<NamePlate> GetNamePlateProgressAsync(int id, CancellationToken cancellationToken = default) =>
        await Client!.GetNamePlateProgressAsync(FriendCode, id, cancellationToken);

    public async Task UploadFromHtmlAsync(string html, CancellationToken cancellationToken = default) =>
        await Client!.UploadFromHtmlAsync(FriendCode, html, cancellationToken);
}
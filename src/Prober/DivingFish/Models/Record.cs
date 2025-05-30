using DXKumaBot.Backend.Prober.Common;
using DXKumaBot.Backend.Prober.DivingFish.Enums;
using DXKumaBot.Backend.Utils;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Models;

public class Record
{
    [JsonPropertyName("achievements")]
    public required decimal Achievements { get; init; }

    [JsonPropertyName("cid")]
    public required int ChartId { get; init; }

    [JsonPropertyName("ds")]
    public required decimal LevelValue { get; init; }

    [JsonPropertyName("dxScore")]
    public required int DXScore { get; init; }

    [JsonPropertyName("fc")]
    public required Optional<ComboFlags, string> ComboFlag { get; init; }

    [JsonPropertyName("fs")]
    public required Optional<SyncFlags, string> SyncFlag { get; init; }

    [JsonPropertyName("level")]
    public required string Level { get; init; }

    [JsonPropertyName("level_index")]
    public required int DifficultyIndex { get; init; }

    [JsonPropertyName("level_label")]
    public required Difficulties Difficulty { get; init; }

    [JsonPropertyName("ra")]
    public required int DXRating { get; init; }

    [JsonPropertyName("rate")]
    public required Ranks Rank { get; init; }

    [JsonPropertyName("song_id")]
    public required int Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("type")]
    public required SongTypes Type { get; init; }

    public string AudioUrl => $"https://assets2.lxns.net/maimai/music/{Id % 10000}.mp3";

    public string JacketUrl => $"https://assets2.lxns.net/maimai/jacket/{Id % 10000}.png";

    public static implicit operator CommonRecord(Record record)
    {
        return new()
        {
            Id = record.Id,
            Title = record.Title,
            Difficulty = (CommonDifficulties)record.Difficulty,
            ComboFlag = record.ComboFlag,
            SyncFlag = record.SyncFlag,
            Rank = record.Rank,
            Type = (CommonSongTypes)record.Type,
            Achievements = record.Achievements,
            DXRating = record.DXRating,
            DXScore = record.DXScore,
        };
    }
}

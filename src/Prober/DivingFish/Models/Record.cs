using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Enums;
using Limekuma.Utils;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public class Record
{
    [JsonPropertyName("achievements")]
    public required double Achievements { get; init; }

    [JsonPropertyName("cid")]
    public int? ChartId { get; init; }

    [JsonPropertyName("ds")]
    public required double LevelValue { get; init; }

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

    public Song Song
    {
        get
        {
            field ??= Songs.Shared.First(x => x.Id == Id.ToString());
            return field;
        }
    }

    public int TotalDXScore
    {
        get
        {
            if (field is 0)
            {
                field = Song.Charts[DifficultyIndex].Notes.Sum() * 3;
            }

            return field;
        }
    }

    public int DXStar
    {
        get
        {
            if (field is not 0)
            {
                return field;
            }

            double dxScorePersent = (double)DXScore / TotalDXScore;
            field = dxScorePersent switch
            {
                < 0.9 => 1,
                < 0.93 => 2,
                < 0.95 => 3,
                < 0.97 => 4,
                <= 1 => 5,
                _ => throw new InvalidDataException()
            };
            return field;
        }
    }

    public static implicit operator CommonRecord(Record record) =>
        new()
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
            DXStar = record.DXStar,
            DXScore = record.DXScore,
            TotalDXScore = record.TotalDXScore,
            LevelValue = record.LevelValue
        };
}
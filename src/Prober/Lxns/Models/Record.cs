using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns.Enums;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record Record : SimpleRecord
{
    [JsonPropertyName("achievements")]
    public required double Achievements { get; init; }

    [JsonPropertyName("dx_score")]
    public required int DXScore { get; init; }

    [JsonPropertyName("dx_rating")]
    public double? DXRating { get; init; }

    [JsonPropertyName("rate")]
    public new Ranks? Rank { get; init; }

    [JsonPropertyName("play_time")]
    public DateTimeOffset? PlayTime { get; init; }

    [JsonPropertyName("upload_time")]
    public DateTimeOffset? UploadTime { get; init; }

    [JsonPropertyName("last_played_time")]
    public DateTimeOffset? LastPlayedTime { get; init; }

    public string AudioUrl => $"https://assets2.lxns.net/maimai/music/{Id}.mp3";

    public string JacketUrl => $"https://assets2.lxns.net/maimai/jacket/{Id}.png";

    public Chart Chart
    {
        get
        {
            if (field is null)
            {
                Song song = SongData.Shared.Songs.First(x => x.Id == Id);
                field = (Type switch
                {
                    SongTypes.Standard => song.Charts.Standard,
                    SongTypes.DX => song.Charts.DX,
                    _ => throw new InvalidDataException()
                })[(int)Difficulty];
            }

            return field;
        }
    }

    public int TotalDXScore
    {
        get
        {
            if (field is 0)
            {
                field = Chart.Notes!.Total * 3;
            }

            return field;
        }
    }

    public double LevelValue
    {
        get
        {
            if (field is 0)
            {
                field = Chart.LevelValue;
            }

            return field;
        }
    }

    public static implicit operator CommonRecord(Record record) =>
        new()
        {
            Id = record.Type is SongTypes.Standard ? record.Id : record.Id + 10000,
            Title = record.Title,
            Difficulty = (CommonDifficulties)(record.Difficulty + 1),
            ComboFlag = record.ComboFlag ?? ComboFlags.None,
            SyncFlag = record.SyncFlag ?? SyncFlags.None,
            Rank = record.Rank ?? Ranks.D,
            Type = (CommonSongTypes)record.Type,
            Achievements = record.Achievements,
            DXRating = (int)(record.DXRating ?? 0),
            DXScore = record.DXScore,
            TotalDXScore = record.TotalDXScore,
            LevelValue = record.LevelValue
        };
}
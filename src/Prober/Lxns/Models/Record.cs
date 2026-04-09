using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns.Enums;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record Record : SimpleRecord
{
    private Lazy<Chart>? _chart;

    private Lazy<double>? _levelValue;

    private Lazy<int>? _totalDXScore;

    [JsonPropertyName("achievements")]
    public required double Achievements { get; init; }

    [JsonPropertyName("dx_score")]
    public required int DXScore { get; init; }

    [JsonPropertyName("dx_star")]
    public required int DXScoreRank { get; init; }

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

    public Chart Chart => (_chart ??= new(() =>
    {
        SongData songData = SongData.Shared;
        if (!songData.SongsById.TryGetValue(Id, out Song? song))
        {
            throw new InvalidDataException();
        }

        return (Type switch
        {
            SongTypes.Standard => song.Charts.Standard,
            SongTypes.DX => song.Charts.DX,
            _ => throw new InvalidDataException()
        })[(int)Difficulty];
    })).Value;

    public int TotalDXScore => (_totalDXScore ??= new(() => Chart.Notes!.Total * 3)).Value;

    public double LevelValue => (_levelValue ??= new(() => Chart.LevelValue)).Value;

    private static CommonDifficulties MapDifficulty(Difficulties difficulty) => difficulty switch
    {
        Difficulties.Dummy => CommonDifficulties.Dummy,
        Difficulties.Basic => CommonDifficulties.Basic,
        Difficulties.Advanced => CommonDifficulties.Advanced,
        Difficulties.Expert => CommonDifficulties.Expert,
        Difficulties.Master => CommonDifficulties.Master,
        Difficulties.ReMaster => CommonDifficulties.ReMaster,
        _ => throw new InvalidDataException()
    };

    public static implicit operator CommonRecord(Record record)
    {
        Chart chart = record.Chart;
        SongData songData = SongData.Shared;
        int versionGroup = chart.Version / 100;
        if (!songData.VersionsByGroup.TryGetValue(versionGroup, out Version? version))
        {
            throw new InvalidDataException();
        }

        bool inCurrentGenre = songData.Versions[^1].VersionNumber / 100 == versionGroup;

        return new()
        {
            Chart = new()
            {
                Song = new()
                {
                    Id = record.Type is SongTypes.Standard ? record.Id : record.Id + 10000,
                    Title = record.Title,
                    Type = (CommonSongTypes)record.Type,
                    Genre = version.Title,
                    InCurrentGenre = inCurrentGenre,
                    AudioUrl = record.AudioUrl,
                    JacketUrl = record.JacketUrl
                },
                Difficulty = MapDifficulty(record.Difficulty),
                TotalDXScore = record.TotalDXScore,
                Level = record.Level,
                LevelValue = record.LevelValue,
                Notes = chart.Notes!
            },
            ComboFlag = record.ComboFlag ?? ComboFlags.None,
            SyncFlag = record.SyncFlag ?? SyncFlags.None,
            Rank = record.Rank ?? Ranks.D,
            Achievements = record.Achievements,
            DXRating = (int)(record.DXRating ?? 0),
            DXScoreRank = record.DXScoreRank,
            DXScore = record.DXScore
        };
    }
}

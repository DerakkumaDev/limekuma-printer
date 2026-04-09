using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Enums;
using Limekuma.Utils;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public class Record
{
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

    [JsonPropertyName("achievements")]
    public required double Achievements { get; init; }

    [JsonPropertyName("cid")]
    public int? ChartId { get; init; }

    [JsonPropertyName("ds")]
    public required double LevelValue { get; init; }

    [JsonPropertyName("dxScore")]
    public required int DXScore { get; init; }

    [JsonPropertyName("fc")]
    public required Union<ComboFlags, string> ComboFlag { get; init; }

    [JsonPropertyName("fs")]
    public required Union<SyncFlags, string> SyncFlag { get; init; }

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

    public string AudioUrl => $"https://assets2.lxns.net/maimai/music/{(Id is > 10000 and < 100000 ? Id % 10000 : Id)}.mp3";

    public string JacketUrl => $"https://maimai.diving-fish.com/covers/{Id}.png";

    private Lazy<Song>? _song;

    public Song Song => (_song ??= new(() => Songs.GetById(Id.ToString()))).Value;

    private Lazy<int>? _totalDXScore;

    public int TotalDXScore => (_totalDXScore ??= new(() => Song.Charts[DifficultyIndex].Notes.Total * 3)).Value;

    private Lazy<int>? _dxScoreRank;

    public int DXScoreRank => (_dxScoreRank ??= new(() => ((double)DXScore / TotalDXScore) switch
    {
        < 0.9 => 1,
        < 0.93 => 2,
        < 0.95 => 3,
        < 0.97 => 4,
        <= 1 => 5,
        _ => throw new InvalidDataException()
    })).Value;

    public static implicit operator CommonRecord(Record record)
    {
        Song song = record.Song;
        Chart chart = song.Charts[record.DifficultyIndex];
        BasicInfo basicInfo = song.BasicInfo;

        return new()
        {
            Chart = new()
            {
                Song = new()
                {
                    Id = record.Id,
                    Title = record.Title,
                    Type = (CommonSongTypes)record.Type,
                    Genre = basicInfo.Genre,
                    InCurrentGenre = basicInfo.InCurrentVersion,
                    AudioUrl = record.AudioUrl,
                    JacketUrl = record.JacketUrl
                },
                Difficulty = MapDifficulty(record.Difficulty),
                TotalDXScore = record.TotalDXScore,
                Level = record.Level,
                LevelValue = record.LevelValue,
                Notes = chart.Notes
            },
            ComboFlag = record.ComboFlag,
            SyncFlag = record.SyncFlag,
            Rank = record.Rank,
            Achievements = record.Achievements,
            DXRating = record.DXRating,
            DXScoreRank = record.DXScoreRank,
            DXScore = record.DXScore
        };
    }
}
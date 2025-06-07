using Limekuma.Prober.DivingFish.Enums;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record Song
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("type")]
    public required SongTypes Type { get; init; }

    [JsonPropertyName("ds")]
    public required List<double> LevelValues { get; init; }

    [JsonPropertyName("level")]
    public required List<string> Levels { get; init; }

    [JsonPropertyName("cids")]
    public required List<int> ChartIds { get; init; }

    [JsonPropertyName("charts")]
    public required List<Chart> Charts { get; init; }

    [JsonPropertyName("basic_info")]
    public required BasicInfo BasicInfo { get; init; }
}

internal record Songs
{
    private Songs(List<Song> songs)
    {
        _pullTime = DateTimeOffset.Now;
        _songs = songs;
    }

    private readonly DateTimeOffset _pullTime;
    private readonly List<Song> _songs;
    private static Songs? _shared;

    public static implicit operator Songs(List<Song> songs) => new(songs);

    public static implicit operator List<Song>(Songs a) => a._songs;

    public static List<Song> Shared
    {
        get
        {
            if (_shared is null || DateTimeOffset.Now.AddHours(10).Date != _shared._pullTime.AddHours(10).Date)
            {
                DfResourceClient _resource = new();
                _shared = _resource.GetSongsAsync().Result;
            }

            return _shared;
        }
    }
}
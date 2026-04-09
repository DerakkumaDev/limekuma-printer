using Limekuma.Prober.DivingFish.Enums;
using System.Collections.Frozen;
using System.Collections.Immutable;
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
    private static Songs? _shared;

    private readonly DateTimeOffset _pullTime;
    private readonly ImmutableArray<Song> _songs;
    private readonly FrozenDictionary<string, Song> _songsById;

    private Songs(IEnumerable<Song> songs)
    {
        _pullTime = DateTimeOffset.Now;
        _songs = [.. songs];
        _songsById = songs.ToFrozenDictionary(x => x.Id);
    }

    private static Songs SharedSongs
    {
        get
        {
            if (_shared is not null && DateTimeOffset.Now.AddHours(10).Date == _shared._pullTime.AddHours(10).Date)
            {
                return _shared;
            }

            DfResourceClient resource = new();
            _shared = (Songs)resource.GetSongsAsync().Result;

            return _shared;
        }
    }

    public static ImmutableArray<Song> Shared => SharedSongs;

    public static Song GetById(string id) => SharedSongs._songsById.TryGetValue(id, out Song? song)
        ? song
        : throw new InvalidDataException();

    public static explicit operator Songs(List<Song> songs) => new(songs);

    public static implicit operator ImmutableArray<Song>(Songs a) => a._songs;
}

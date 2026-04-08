using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record SongData
{
    public SongData() => PullTime = DateTimeOffset.Now;

    public DateTimeOffset PullTime { get; }

    [JsonPropertyName("songs")]
    public required List<Song> Songs { get; set; }

    [JsonPropertyName("genres")]
    public required List<Genre> Genres { get; set; }

    [JsonPropertyName("versions")]
    public required List<Version> Versions { get; set; }

    private FrozenDictionary<int, Song>? _songsById;

    public FrozenDictionary<int, Song> SongsById => _songsById ??= Songs.ToFrozenDictionary(x => x.Id);

    private FrozenDictionary<int, Version>? _versionsByGroup;

    public FrozenDictionary<int, Version> VersionsByGroup => _versionsByGroup ??=
        Versions.GroupBy(x => x.VersionNumber / 100).ToFrozenDictionary(x => x.Key, x => x.First());

    public static SongData Shared
    {
        get
        {
            if (field is null || DateTimeOffset.Now.AddHours(10).Date != field.PullTime.AddHours(10).Date)
            {
                LxnsResourceClient _resource = new();
                field = _resource.GetSongsAsync(includeNotes: true).Result;
            }

            return field;
        }
    }
}
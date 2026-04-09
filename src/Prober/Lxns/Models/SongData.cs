using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record SongData
{
    private readonly DateTimeOffset _pullTime = DateTimeOffset.Now;

    [JsonPropertyName("songs")]
    public required List<Song> Songs { get; set; }

    [JsonPropertyName("genres")]
    public required List<Genre> Genres { get; set; }

    [JsonPropertyName("versions")]
    public required List<Version> Versions { get; set; }

    public FrozenDictionary<int, Song> SongsById => field ??= Songs.ToFrozenDictionary(x => x.Id);

    public FrozenDictionary<int, Version> VersionsByGroup => field ??=
        Versions.GroupBy(x => x.VersionNumber / 100).ToFrozenDictionary(x => x.Key, x => x.First());

    public static SongData Shared
    {
        get
        {
            if (field is not null && DateTimeOffset.Now.AddHours(10).Date == field._pullTime.AddHours(10).Date)
            {
                return field;
            }

            LxnsResourceClient resource = new();
            field = resource.GetSongsAsync(includeNotes: true).Result;

            return field;
        }
    }
}

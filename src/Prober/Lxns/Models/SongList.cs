using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record SongList
{
    public SongList()
    {
        PullTime = DateTimeOffset.Now;
    }

    public DateTimeOffset PullTime { get; }

    [JsonPropertyName("songs")]
    public required List<Song> Songs { get; set; }

    [JsonPropertyName("genres")]
    public required List<Genre> Genres { get; set; }

    [JsonPropertyName("versions")]
    public required List<Version> Versions { get; set; }
}

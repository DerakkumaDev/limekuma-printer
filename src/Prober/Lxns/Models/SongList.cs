using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Lxns.Models;

public record SongList
{
    [JsonPropertyName("songs")]
    public required List<Song> Songs { get; set; }

    [JsonPropertyName("genres")]
    public required List<Genre> Genres { get; set; }

    [JsonPropertyName("versions")]
    public required List<Version> Versions { get; set; }
}

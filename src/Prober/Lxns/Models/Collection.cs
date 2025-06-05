using Limekuma.Prober.Common;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record Collection
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("required")]
    public List<CollectionRequired>? Required { get; init; }
}

public record Trophy : Collection
{
    [JsonPropertyName("color")]
    public required TrophyColor Color { get; init; }
}

public record Icon : Collection
{
    public string Url => $"https://assets2.lxns.net/maimai/icon/{Id}.png";

    [JsonPropertyName("genre")]
    public required string Genre { get; init; }
}

public record NamePlate : Collection
{
    public string Url => $"https://assets2.lxns.net/maimai/plate/{Id}.png";

    [JsonPropertyName("genre")]
    public required string Genre { get; init; }
}

public record Frame : Collection
{
    [JsonPropertyName("genre")]
    public required string Genre { get; init; }
}

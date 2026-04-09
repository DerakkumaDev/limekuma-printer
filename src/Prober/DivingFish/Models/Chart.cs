using Limekuma.Prober.Common;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Models;

public record Chart
{
    [JsonPropertyName("notes")]
    public required List<int> NotesNumber { get; set; }

    [JsonPropertyName("charter")]
    public required string Charter { get; set; }

    public Notes Notes => new()
    {
        Total = NotesNumber.Sum(),
        Tap = NotesNumber[0],
        Hold = NotesNumber[1],
        Slide = NotesNumber[2],
        Touch = NotesNumber.Count > 4 ? NotesNumber[3] : 0,
        Break = NotesNumber.Count < 5 ? NotesNumber[3] : NotesNumber[4]
    };
}

using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<SongTypes>))]
public enum ResponseStatus
{
    Success,
    [JsonStringEnumMemberName("error")]
    Error
}

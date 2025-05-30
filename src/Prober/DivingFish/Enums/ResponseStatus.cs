using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<SongTypes>))]
public enum ResponseStatus
{
    Success,
    [JsonStringEnumMemberName("error")]
    Error
}

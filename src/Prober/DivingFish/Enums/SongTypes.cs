using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<SongTypes>))]
public enum SongTypes
{
    [JsonStringEnumMemberName("SD")]
    Standard,
    [JsonStringEnumMemberName("DX")]
    DX,
    [JsonStringEnumMemberName("UTAGE")]
    Utage
}

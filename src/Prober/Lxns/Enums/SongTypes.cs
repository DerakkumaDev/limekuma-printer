using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Lxns.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<SongTypes>))]
public enum SongTypes
{
    [JsonStringEnumMemberName("standard")]
    Standard,
    [JsonStringEnumMemberName("dx")]
    DX,
    [JsonStringEnumMemberName("utage")]
    Utage
}

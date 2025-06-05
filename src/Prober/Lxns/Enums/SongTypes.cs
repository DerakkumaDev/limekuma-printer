using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Enums;

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

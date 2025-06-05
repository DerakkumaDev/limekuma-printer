using System.Text.Json.Serialization;

namespace Limekuma.Prober.Common;

[JsonConverter(typeof(JsonStringEnumConverter<Ranks>))]
public enum Ranks
{
    [JsonStringEnumMemberName("d")]
    D,
    [JsonStringEnumMemberName("c")]
    C,
    [JsonStringEnumMemberName("b")]
    B,
    [JsonStringEnumMemberName("bb")]
    BB,
    [JsonStringEnumMemberName("bbb")]
    BBB,
    [JsonStringEnumMemberName("a")]
    A,
    [JsonStringEnumMemberName("aa")]
    AA,
    [JsonStringEnumMemberName("aaa")]
    AAA,
    [JsonStringEnumMemberName("s")]
    S,
    [JsonStringEnumMemberName("sp")]
    SPlus,
    [JsonStringEnumMemberName("ss")]
    SS,
    [JsonStringEnumMemberName("ssp")]
    SSPlus,
    [JsonStringEnumMemberName("sss")]
    SSS,
    [JsonStringEnumMemberName("sssp")]
    SSSPlus
}

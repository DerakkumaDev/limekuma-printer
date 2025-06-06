using System.Text.Json.Serialization;

namespace Limekuma.Prober.Common;

[JsonConverter(typeof(JsonStringEnumConverter<ComboFlags>))]
public enum ComboFlags
{
    None,

    [JsonStringEnumMemberName("fc")]
    FullCombo,

    [JsonStringEnumMemberName("fcp")]
    FullComboPlus,

    [JsonStringEnumMemberName("ap")]
    AllPerfect,

    [JsonStringEnumMemberName("app")]
    AllPerfectPlus
}
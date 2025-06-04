using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Common;

[JsonConverter(typeof(JsonStringEnumConverter<TrophyColor>))]
public enum TrophyColor
{
    Normal,
    Bronze,
    Silver,
    Gold,
    Rainbow
}

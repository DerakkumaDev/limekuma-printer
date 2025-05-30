using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.DivingFish.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<Difficulties>))]
public enum Difficulties
{
    Dummy,
    Basic,
    Advanced,
    Expert,
    Master,
    [JsonStringEnumMemberName("Re:MASTER")]
    ReMaster
}

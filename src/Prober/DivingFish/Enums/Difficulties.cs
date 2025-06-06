using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish.Enums;

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
using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Prober.Common;

[JsonConverter(typeof(JsonStringEnumConverter<SyncFlags>))]
public enum SyncFlags
{
    None,
    [JsonStringEnumMemberName("fs")]
    FullSync,
    [JsonStringEnumMemberName("fsp")]
    FullSyncPlus,
    [JsonStringEnumMemberName("fsd")]
    FullSyncDX,
    [JsonStringEnumMemberName("fsdp")]
    FullSyncDXPlus,
    [JsonStringEnumMemberName("sync")]
    SyncPlay
}

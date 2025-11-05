using Limekuma.Prober.Common;
using Limekuma.Render;

namespace Limekuma.Utils;

internal static class ServiceHelper
{
    internal static async Task PrepareUserDataAsync(CommonUser user)
    {
        await PrepareImageAsset("Icon", user.IconId, user.IconUrl);
        await PrepareImageAsset("Plate", user.PlateId, user.PlateUrl);
        await PrepareImageAsset("Frame", user.FrameId, user.FrameUrl);
    }

    internal static async Task PrepareRecordDataAsync(IReadOnlyList<CommonRecord> records) =>
        await Parallel.ForEachAsync(records, async (record, _) => await PrepareRecordDataAsync(record));

    internal static async Task PrepareRecordDataAsync(CommonRecord record) =>
        await PrepareImageAsset("Jacket", record.Id % 10000, record.JacketUrl);

    private static async Task PrepareImageAsset(string type, int id, string url)
    {
        AssetProvider assetProvider = AssetProvider.Shared;
        string? path = assetProvider.GetPath(type);
        if (path is null)
        {
            return;
        }

        path = Path.Combine(path, $"{id}.png");
        if (File.Exists(path))
        {
            return;
        }

        using HttpClient http = new();
        using Stream stream = await http.GetStreamAsync(url);
        using FileStream fileStream = File.OpenWrite(path);
        stream.CopyTo(fileStream);
    }
}
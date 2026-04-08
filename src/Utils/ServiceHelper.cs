using Limekuma.Prober.Common;
using Limekuma.Render;
using System.Reflection;

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
        await PrepareImageAsset("Jacket", record.Chart.Song.Id % 10000, record.Chart.Song.JacketUrl);

    private static async Task PrepareImageAsset(string assetKey, int id, string url)
    {
        string? basePath = AssetProvider.Shared.GetPath(assetKey);
        if (string.IsNullOrEmpty(basePath))
        {
            return;
        }

        string path = Path.Combine(basePath, $"{id}.png");
        if (File.Exists(path))
        {
            return;
        }
        Directory.CreateDirectory(basePath);

        using HttpClient http = new();
        http.DefaultRequestHeaders.UserAgent.Add(new("limekuma",
            Assembly.GetExecutingAssembly().GetName().Version?.ToString()));
        await using Stream stream = await http.GetStreamAsync(url);
        await using FileStream fileStream = File.OpenWrite(path);
        await stream.CopyToAsync(fileStream);
    }
}
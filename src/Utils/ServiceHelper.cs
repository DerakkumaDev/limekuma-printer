using Limekuma.Draw;
using Limekuma.Prober.Common;

namespace Limekuma.Utils;

internal static class ServiceHelper
{
    internal static async Task PrepareUserDataAsync(CommonUser user)
    {
        await PrepareImageAsset(BestsDrawer.IconRootPath, user.IconId, user.IconUrl);
        await PrepareImageAsset(BestsDrawer.PlateRootPath, user.PlateId, user.PlateUrl);
        await PrepareImageAsset(BestsDrawer.FrameRootPath, user.FrameId, user.FrameUrl);
    }

    internal static async Task PrepareRecordDataAsync(IReadOnlyList<CommonRecord> records) =>
        await Parallel.ForEachAsync(records, async (record, _) => await PrepareRecordDataAsync(record));

    internal static async Task PrepareRecordDataAsync(CommonRecord record) =>
        await PrepareImageAsset(DrawerBase.JacketRootPath, record.Id % 10000, record.JacketUrl);

    private static async Task PrepareImageAsset(string path, int id, string url)
    {
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
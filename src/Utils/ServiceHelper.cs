using Limekuma.Draw;
using Limekuma.Prober.Common;

namespace Limekuma.Utils;

internal static class ServiceHelper
{
    internal static async Task PrepareUserDataAsync(CommonUser user)
    {
        if (!File.Exists(Path.Combine(BestsDrawer.IconRootPath, $"{user.IconId}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = File.OpenWrite(Path.Combine(BestsDrawer.IconRootPath, $"{user.IconId}.png"));
            using Stream imageStream = await http.GetStreamAsync(user.IconUrl);
            imageStream.CopyTo(stream);
        }

        if (!File.Exists(Path.Combine(BestsDrawer.PlateRootPath, $"{user.PlateId}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = File.OpenWrite(Path.Combine(BestsDrawer.PlateRootPath, $"{user.PlateId}.png"));
            using Stream imageStream = await http.GetStreamAsync(user.PlateUrl);
            imageStream.CopyTo(stream);
        }

        if (!File.Exists(Path.Combine(BestsDrawer.FrameRootPath, $"{user.FrameId}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = File.OpenWrite(Path.Combine(BestsDrawer.FrameRootPath, $"{user.FrameId}.png"));
            using Stream imageStream = await http.GetStreamAsync(user.FrameUrl);
            imageStream.CopyTo(stream);
        }
    }

    internal static async Task PrepareRecordDataAsync(IList<CommonRecord> records)
    {
        foreach (CommonRecord record in records)
        {
            await PrepareRecordDataAsync(record);
        }
    }

    internal static async Task PrepareRecordDataAsync(CommonRecord record)
    {
        if (File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png")))
        {
            return;
        }

        using HttpClient http = new();
        using FileStream stream = File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png"));
        using Stream imageStream = await http.GetStreamAsync(record.JacketUrl);
        imageStream.CopyTo(stream);
    }
}
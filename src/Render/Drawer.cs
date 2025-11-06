using Limekuma.Prober.Common;
using SixLabors.ImageSharp;
using System.Collections;

namespace Limekuma.Render;

public class Drawer
{
    public async Task<Image> DrawBestsAsync(CommonUser user, IEnumerable<CommonRecord> ever,
        IEnumerable<CommonRecord> current, int everTotal, int currentTotal, string typename, string prober) =>
        await DrawBestsAsync(user, ever, current, everTotal, currentTotal, typename, prober, false, false);

    public async Task<Image> DrawBestsAsync(CommonUser user, IEnumerable<CommonRecord> ever,
        IEnumerable<CommonRecord> current, int everTotal, int currentTotal, string typename, string prober,
        bool isAnime) =>
        await DrawBestsAsync(user, ever, current, everTotal, currentTotal, typename, prober, isAnime, false);

    public async Task<Image> DrawBestsAsync(CommonUser user, IEnumerable<CommonRecord> ever,
        IEnumerable<CommonRecord> current, int everTotal, int currentTotal, string typename, string prober,
        bool isAnime, bool drawLevelSeg) => await DrawBestsAsync(user, ever, current, everTotal, currentTotal, typename,
        prober, isAnime, drawLevelSeg, "./Resources/Layouts/bests.xml");

    public async Task<Image> DrawBestsAsync(CommonUser user, IEnumerable<CommonRecord> ever,
        IEnumerable<CommonRecord> current, int everTotal, int currentTotal, string typename, string prober,
        bool isAnime, bool drawLevelSeg, string xmlPath)
    {
        Dictionary<string, object> scope = new()
        {
            ["user"] = user,
            ["ever"] = ever,
            ["current"] = current,
            ["everTotal"] = everTotal,
            ["currentTotal"] = currentTotal,
            ["typename"] = typename,
            ["prober"] = prober,
            ["isAnime"] = isAnime,
            ["drawLevelSeg"] = drawLevelSeg,
        };
        return await DrawAsync(scope, xmlPath);
    }

    public async Task<Image> DrawListAsync(CommonUser user, IEnumerable<CommonRecord> records, int page, int total,
        IEnumerable<int> counts, string level, string prober) => await DrawListAsync(user, records, page, total, counts,
        level, prober, "./Resources/Layouts/list.xml");

    public async Task<Image> DrawListAsync(CommonUser user, IEnumerable<CommonRecord> records, int page, int total,
        IEnumerable<int> counts, string level, string prober, string xmlPath)
    {
        Dictionary<string, object> scope = new()
        {
            ["user"] = user,
            ["records"] = records,
            ["page"] = page,
            ["total"] = total,
            ["counts"] = counts,
            ["level"] = level,
            ["prober"] = prober
        };
        return await DrawAsync(scope, xmlPath);
    }

    private async Task<Image> DrawAsync(Dictionary<string, object> scope, string xmlPath)
    {
        IAsyncExpressionEngine expr = new NCalcExpressionEngine();
        expr.RegisterFunction("Reverse", (object l) =>
        {
            if (l is IEnumerable<object> e)
            {
                return e.Reverse();
            }

            return (IEnumerable)(l.ToString() ?? "[NIL]").Reverse();
        });
        XmlSceneLoader loader = new(expr);
        Node tree = await loader.LoadAsync(xmlPath, scope);
        AssetProvider assets = AssetProvider.Shared;
        Image image = Renderer.Render((CanvasNode)tree, assets, assets);
        return image;
    }
}
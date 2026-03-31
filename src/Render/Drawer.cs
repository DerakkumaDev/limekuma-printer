using Limekuma.Prober.Common;
using Limekuma.Render.ExpressionEngine;
using Limekuma.Render.Nodes;
using SixLabors.ImageSharp;

namespace Limekuma.Render;

public sealed class Drawer
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
        List<CommonRecord> everList = [.. ever];
        List<CommonRecord> currentList = [.. current];
        List<object> everCards = [.. everList.Select((record, idx) => new { Record = record, Index = idx + 1 })];
        List<object> currentCards =
            [.. currentList.Select((record, idx) => new { Record = record, Index = idx + everList.Count + 1 })];
        int everDelta = everList.Count > 34 ? everList[0].DXRating - everList[^1].DXRating : everList.Count > 0 ? everList[0].DXRating : 0;
        int currentDelta = currentList.Count > 14 ? currentList[0].DXRating - currentList[^1].DXRating : currentList.Count > 0 ? currentList[0].DXRating : 0;
        int everMax = everList.Count > 0 ? everList[0].DXRating : 0;
        int everMin = everList.Count > 0 ? everList[^1].DXRating : 0;
        int currentMax = currentList.Count > 0 ? currentList[0].DXRating : 0;
        int currentMin = currentList.Count > 0 ? currentList[^1].DXRating : 0;
        int realRating = everTotal + currentTotal;
        string proberState = "on";
        if (user.Rating != realRating)
        {
            proberState = "off";
        }
        else if (everList.Any(r => r.DXScore is 0 && (r.DXStar > 0 || r.Rank < Ranks.C)) ||
                 currentList.Any(r => r.DXScore is 0 && (r.DXStar > 0 || r.Rank < Ranks.C)))
        {
            proberState = "warning";
        }

        Dictionary<string, object?> scope = new(StringComparer.OrdinalIgnoreCase)
        {
            ["user"] = user,
            ["everCards"] = everCards,
            ["currentCards"] = currentCards,
            ["everDelta"] = everDelta,
            ["currentDelta"] = currentDelta,
            ["everTotal"] = everTotal,
            ["currentTotal"] = currentTotal,
            ["realRating"] = realRating,
            ["typename"] = typename,
            ["prober"] = prober,
            ["isAnime"] = isAnime,
            ["drawLevelSeg"] = drawLevelSeg,
            ["proberState"] = proberState,
            ["everMax"] = everMax,
            ["everMin"] = everMin,
            ["currentMax"] = currentMax,
            ["currentMin"] = currentMin,
        };
        return await DrawAsync(scope, xmlPath);
    }

    public async Task<Image> DrawListAsync(CommonUser user, IEnumerable<CommonRecord> records, int page, int total,
        IEnumerable<int> counts, string level, string prober) => await DrawListAsync(user, records, page, total, counts,
        level, prober, "./Resources/Layouts/list.xml");

    public async Task<Image> DrawListAsync(CommonUser user, IEnumerable<CommonRecord> records, int page, int total,
        IEnumerable<int> counts, string level, string prober, string xmlPath)
    {
        List<CommonRecord> list = [.. records];
        List<object> recordCards = [.. list.Select((record, idx) => new { Record = record, Index = idx + 1 })];
        List<int> countList = counts.ToList();
        int totalCount = countList.Count > 0 ? countList[^1] : 0;
        bool warning = list.Any(r => r.DXScore is 0 && (r.DXStar > 0 || r.Rank < Ranks.C));
        Dictionary<string, object?> scope = new(StringComparer.OrdinalIgnoreCase)
        {
            ["user"] = user,
            ["recordCards"] = recordCards,
            ["page"] = page,
            ["total"] = total,
            ["counts"] = countList[..^1],
            ["statsTotalCount"] = totalCount,
            ["level"] = level,
            ["prober"] = prober,
            ["proberState"] = warning ? "warning" : "on",
            ["isAnime"] = false,
        };
        return await DrawAsync(scope, xmlPath);
    }

    private async Task<Image> DrawAsync(Dictionary<string, object?> scope, string xmlPath)
    {
        AsyncNCalcEngine expr = new();
        RegisterFunctions(expr);
        TemplateReader loader = new(expr);
        Node tree = await loader.LoadAsync(xmlPath, scope);
        AssetProvider assets = AssetProvider.Shared;
        return NodeRenderer.Render((CanvasNode)tree, assets, assets);
    }

    private void RegisterFunctions(AsyncNCalcEngine expr)
    {
        expr.RegisterFunction("ToString", (object x) => Convert.ToString(x));
    }
}
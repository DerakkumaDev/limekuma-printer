using Limekuma.Prober.Common;
using Limekuma.Render.ExpressionEngine;
using Limekuma.Render.Nodes;
using SixLabors.ImageSharp;
using System.Collections;
using System.Collections.Immutable;

namespace Limekuma.Render;

public sealed class Drawer
{
    public async Task<Image> DrawBestsAsync(CommonUser user, IReadOnlyList<CommonRecord> ever,
        IReadOnlyList<CommonRecord> current, int everTotal, int currentTotal, string? condition, string prober, IEnumerable<string> tags) =>
        await DrawBestsAsync(user, ever, current, everTotal, currentTotal, condition, prober, tags, "./Resources/Layouts/bests.xml");

    public async Task<Image> DrawBestsAsync(CommonUser user, IReadOnlyList<CommonRecord> ever,
        IReadOnlyList<CommonRecord> current, int everTotal, int currentTotal, string? condition, string prober, IEnumerable<string> tags, string xmlPath)
    {
        int everMax = ever.Count > 0 ? ever[0].DXRating : 0;
        int everMin = ever.Count > 0 ? ever[^1].DXRating : 0;
        int currentMax = current.Count > 0 ? current[0].DXRating : 0;
        int currentMin = current.Count > 0 ? current[^1].DXRating : 0;
        bool mayMask = ever.Any(r => r.DXScore is 0 && (r.DXScoreRank > 0 || r.Rank > Ranks.A)) || current.Any(r => r.DXScore is 0 && (r.DXScoreRank > 0 || r.Rank > Ranks.A));
        Dictionary<string, object?> scope = new(StringComparer.OrdinalIgnoreCase)
        {
            ["userInfo"] = user,
            ["everRecords"] = ever,
            ["currentRecords"] = current,
            ["everRating"] = everTotal,
            ["currentRating"] = currentTotal,
            ["condition"] = condition,
            ["proberName"] = prober,
            ["tags"] = tags,
            ["mayMask"] = mayMask,
            ["everMax"] = everMax,
            ["everMin"] = everMin,
            ["currentMax"] = currentMax,
            ["currentMin"] = currentMin,
        };
        return await DrawAsync(scope, xmlPath);
    }

    public async Task<Image> DrawListAsync(CommonUser user, IReadOnlyList<CommonRecord> records, int page,
        ImmutableArray<int> counts, int totalCount, int startIndex, string condition, bool mayMask, string prober, IEnumerable<string> tags) =>
        await DrawListAsync(user, records, page, counts, totalCount, startIndex, condition, mayMask, prober, tags, "./Resources/Layouts/list.xml");

    public async Task<Image> DrawListAsync(CommonUser user, IReadOnlyList<CommonRecord> records, int page,
        ImmutableArray<int> counts, int totalCount, int startIndex, string condition, bool mayMask, string prober, IEnumerable<string> tags, string xmlPath)
    {
        int totalPages = (int)Math.Ceiling((double)totalCount / 55);
        Dictionary<string, object?> scope = new(StringComparer.OrdinalIgnoreCase)
        {
            ["userInfo"] = user,
            ["pageRecords"] = records,
            ["pageNumber"] = page,
            ["totalPages"] = totalPages,
            ["rankCounts"] = counts[..7],
            ["comboCounts"] = counts[7..^1],
            ["totalCount"] = totalCount,
            ["startIndex"] = startIndex,
            ["condition"] = condition,
            ["proberName"] = prober,
            ["tags"] = tags,
            ["mayMask"] = mayMask,
        };
        return await DrawAsync(scope, xmlPath);
    }

    private static async Task<Image> DrawAsync(IDictionary<string, object?> scope, string xmlPath)
    {
        AsyncNCalcEngine expr = new();
        RegisterFunctions(expr);
        TemplateReader loader = new(expr);
        Node tree = await loader.LoadAsync(xmlPath, scope);
        AssetProvider assets = AssetProvider.Shared;
        return NodeRenderer.Render((CanvasNode)tree, assets, assets);
    }

    private static void RegisterFunctions(AsyncNCalcEngine expr)
    {
        expr.RegisterFunction("ToString", (object x) => Convert.ToString(x));
        expr.RegisterFunction("Count", (IList x) => x.Count);
    }
}
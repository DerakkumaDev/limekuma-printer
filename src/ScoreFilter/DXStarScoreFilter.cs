using Limekuma.Prober.Common;

namespace Limekuma.ScoreFilter;

[ScoreFilterTag("dx_star")]
public sealed class DXStarScoreFilter : IScoreFilter
{
    public bool MaskMutex => false;

    public Func<CommonRecord, bool> GetFilter(string? condition)
    {
        if (!int.TryParse(condition, out int dxStar))
        {
            return _ => true;
        }

        return x => x.DXStar >= dxStar;
    }
}
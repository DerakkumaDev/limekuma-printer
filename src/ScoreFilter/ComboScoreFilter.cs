using Limekuma.Prober.Common;

namespace Limekuma.ScoreFilter;

[ScoreFilterTag("combo")]
public sealed class ComboScoreFilter : IScoreFilter
{
    public Func<CommonRecord, bool> GetFilter(string? condition)
    {
        if (!Enum.TryParse(condition, out ComboFlags comboFlag))
        {
            return _ => true;
        }

        return x => x.ComboFlag >= comboFlag;
    }
}
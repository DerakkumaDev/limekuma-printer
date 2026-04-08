using Limekuma.Prober.Common;

namespace Limekuma.ScoreFilter;

public interface IScoreFilter
{
    Func<CommonRecord, bool> GetFilter(string? condition);
    bool MaskMutex { get; }
}
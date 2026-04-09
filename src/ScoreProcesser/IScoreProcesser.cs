using Grpc.Core;
using Limekuma.Prober.Common;
using System.Collections.Immutable;

namespace Limekuma.ScoreProcesser;

public interface IScoreProcesser
{
    (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records) =>
        throw new RpcException(new(StatusCode.InvalidArgument,
            "Single score data is not supported by this processer."));

    (ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>) Process(IReadOnlyList<CommonRecord> records1p,
        IReadOnlyList<CommonRecord> records2p) => Process(records1p);
}

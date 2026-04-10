using Grpc.Core;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using System.Net;

namespace Limekuma.Services;

internal static class DfGatewayService
{
    private static readonly DfResourceClient ResourceClient = new();

    internal static Task<PlayerData> GetPlayerDataAsync(string token, uint qq)
    {
        DfDeveloperClient client = new(token);
        return ServiceExecutionHelper.ExecuteWithHttpMappingAsync(() => client.GetPlayerDataAsync(qq),
            (HttpStatusCode.BadRequest, StatusCode.NotFound), (HttpStatusCode.Forbidden, StatusCode.PermissionDenied));
    }

    internal static Task<Player> GetPlayerAsync(uint qq)
    {
        return ServiceExecutionHelper.ExecuteWithHttpMappingAsync(() => ResourceClient.GetPlayerAsync(qq),
            (HttpStatusCode.BadRequest, StatusCode.NotFound), (HttpStatusCode.Forbidden, StatusCode.PermissionDenied));
    }
}

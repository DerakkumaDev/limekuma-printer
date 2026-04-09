using Grpc.Core;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using System.Net;

namespace Limekuma.Services;

internal static class DfGatewayService
{
    internal static Task<PlayerData> GetPlayerDataAsync(string token, uint qq)
    {
        DfDeveloperClient client = new(token);
        return ServiceExecutionHelper.ExecuteWithHttpMappingAsync(() => client.GetPlayerDataAsync(qq),
            (HttpStatusCode.BadRequest, StatusCode.NotFound), (HttpStatusCode.Forbidden, StatusCode.PermissionDenied));
    }

    internal static Task<Player> GetPlayerAsync(uint qq)
    {
        DfResourceClient client = new();
        return ServiceExecutionHelper.ExecuteWithHttpMappingAsync(() => client.GetPlayerAsync(qq),
            (HttpStatusCode.BadRequest, StatusCode.NotFound), (HttpStatusCode.Forbidden, StatusCode.PermissionDenied));
    }
}

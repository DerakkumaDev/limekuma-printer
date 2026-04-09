using Grpc.Core;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using System.Net;

namespace Limekuma.Services;

internal static class LxnsGatewayService
{
    internal static Task<Player> GetPlayerByQqAsync(string devToken, uint qq)
    {
        LxnsDeveloperClient client = new(devToken);
        return ServiceExecutionHelper.ExecuteWithHttpMappingAsync(() => client.GetPlayerByQQAsync(qq),
            (HttpStatusCode.NotFound, StatusCode.NotFound), (HttpStatusCode.Forbidden, StatusCode.PermissionDenied));
    }

    internal static async Task<Player> GetPlayerByPersonalTokenAsync(string devToken, string personalToken)
    {
        LxnsDeveloperClient developer = new(devToken);
        LxnsPersonalClient personal = new(personalToken);
        Player player = await ServiceExecutionHelper.ExecuteWithHttpMappingAsync(
            () => personal.GetPlayerAsync(developer), (HttpStatusCode.Unauthorized, StatusCode.Unauthenticated));

        return await ServiceExecutionHelper.ExecuteWithHttpMappingAsync(
            () => developer.GetPlayerAsync(player.FriendCode), (HttpStatusCode.NotFound, StatusCode.NotFound),
            (HttpStatusCode.Forbidden, StatusCode.PermissionDenied));
    }

    internal static Task<List<Record>> GetRecordsAsync(string personalToken)
    {
        LxnsPersonalClient personal = new(personalToken);
        return ServiceExecutionHelper.ExecuteWithHttpMappingAsync(() => personal.GetRecordsAsync(),
            (HttpStatusCode.Unauthorized, StatusCode.Unauthenticated));
    }

    internal static Task<Bests> GetBestsAsync(Player player) => ServiceExecutionHelper.ExecuteWithHttpMappingAsync(
        () => player.GetBestsAsync(), (HttpStatusCode.BadRequest, StatusCode.NotFound),
        (HttpStatusCode.Forbidden, StatusCode.PermissionDenied));
}

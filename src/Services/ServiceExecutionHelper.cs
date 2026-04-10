using Grpc.Core;
using Limekuma.Prober.Common;
using System.Collections.Frozen;
using System.Net;
using System.Text.Json;

namespace Limekuma.Services;

internal static class ServiceExecutionHelper
{
    internal static void EnsureArgument(bool condition, string message = "Invalid arguments")
    {
        if (!condition)
        {
            throw new RpcException(new(StatusCode.InvalidArgument, message));
        }
    }

    internal static void EnsurePermission(bool condition, string message)
    {
        if (!condition)
        {
            throw new RpcException(new(StatusCode.PermissionDenied, message));
        }
    }

    internal static bool HasMaskedScores(IEnumerable<CommonRecord> records) =>
        records.Any(r => r.DXScore is 0 && (r.DXScoreRank > 0 || r.Rank > Ranks.A));

    internal static T DeserializeOrThrow<T>(string payload, string message) where T : class =>
        JsonSerializer.Deserialize<T>(payload) ?? throw new RpcException(new(StatusCode.InvalidArgument, message));

    internal static async Task<T> ExecuteWithHttpMappingAsync<T>(Func<Task<T>> action,
        params (HttpStatusCode HttpStatus, StatusCode RpcStatus)[] mappings)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException ex) when (TryMapHttpStatus(ex.StatusCode, mappings, out StatusCode rpcStatus))
        {
            throw new RpcException(new(rpcStatus, ex.Message, ex));
        }
    }

    private static bool TryMapHttpStatus(HttpStatusCode? statusCode,
        (HttpStatusCode HttpStatus, StatusCode RpcStatus)[] mappings, out StatusCode rpcStatus)
    {
        if (!statusCode.HasValue)
        {
            rpcStatus = default;
            return false;
        }

        FrozenDictionary<HttpStatusCode, StatusCode> mapping =
            mappings.ToFrozenDictionary(x => x.HttpStatus, x => x.RpcStatus);
        if (mapping.TryGetValue(statusCode.Value, out rpcStatus))
        {
            return true;
        }

        rpcStatus = default;
        return false;
    }
}

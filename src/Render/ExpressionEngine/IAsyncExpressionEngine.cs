namespace Limekuma.Render.ExpressionEngine;

public interface IAsyncExpressionEngine
{
    Task<T?> EvalAsync<T>(string expr, object? scope);
    Task<object?> EvalAsync(string expr, object? scope);
    void RegisterFunction(string name, Delegate func);
}
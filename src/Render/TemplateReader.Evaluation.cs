using Limekuma.Render.ExpressionEngine;
using Limekuma.Render.Types;
using SixLabors.ImageSharp;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Limekuma.Render;

public sealed partial class TemplateReader
{
    private async Task<string> EvaluateRequiredTemplateAttributeAsync(XElement element, string name, object? scope) =>
        await EvaluateTemplateAsync(GetRequiredAttributeValue(element, name), scope);

    private async Task<string> EvaluateTemplateAttributeOrAsync(XElement element, string name, string defaultValue,
        object? scope)
    {
        XAttribute? attribute = element.Attribute(name);
        if (attribute is null)
        {
            return defaultValue;
        }

        return await EvaluateTemplateAsync(attribute.Value, scope);
    }

    private async Task<string?> EvaluateOptionalTemplateAttributeAsync(XElement element, string name, object? scope)
    {
        XAttribute? attribute = element.Attribute(name);
        if (attribute is null)
        {
            return null;
        }

        return await EvaluateTemplateAsync(attribute.Value, scope);
    }

    private async Task<T> EvaluateRequiredExpressionAttributeAsync<T>(XElement element, string name, object? scope)
    {
        string raw = GetRequiredAttributeValue(element, name);
        T value = await EvaluateExpressionAsAsync<T>(raw, scope) ?? throw new InvalidOperationException(
            $"DSL[AttributeNull] Required attribute evaluated to null. Context: element='{element.Name.LocalName}', attribute='{name}', expression='{raw}'");
        return value;
    }

    private async Task<T> EvaluateExpressionAttributeOrAsync<T>(XElement element, string name, T defaultValue,
        object? scope)
    {
        XAttribute? attribute = element.Attribute(name);
        if (attribute is null)
        {
            return defaultValue;
        }

        if (defaultValue is null && string.IsNullOrEmpty(attribute.Value))
        {
            return defaultValue;
        }

        T? value = await EvaluateExpressionAsAsync<T>(attribute.Value, scope);
        return value ?? defaultValue;
    }

    private async Task<T> EvaluateRequiredExpressionAsAsync<T>(string raw, object? scope)
    {
        T value = await EvaluateExpressionAsAsync<T>(raw, scope) ??
                  throw new InvalidOperationException(
                      $"DSL[ExpressionEval] Can not evaluate expression. Context: expression='{raw}'");
        return value;
    }

    private async Task<Color> ParseColorAttributeOrAsync(XElement element, string name, string defaultRaw,
        object? scope)
    {
        string raw = GetAttributeValueOrDefault(element, name, defaultRaw);
        string colorText = await EvaluateTemplateAsync(raw, scope);
        return Color.Parse(colorText);
    }

    private async Task<Color?> ParseColorAttributeAsync(XElement element, string name, object? scope)
    {
        string? raw = element.Attribute(name)?.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        string colorText = await EvaluateTemplateAsync(raw, scope);
        return Color.Parse(colorText);
    }

    private async Task<ResamplerType> ParseResamplerTypeAsync(XElement element, object? scope) =>
        await EvaluateExpressionAttributeOrAsync(element, "resampler", ResamplerType.Lanczos3, scope);

    private async Task<string> EvaluateTemplateAsync(string? template, object? scope)
    {
        if (template is null)
        {
            return string.Empty;
        }

        List<string> expressionTexts = [];
        StringBuilder safeTemplateBuilder = new(template.Length);
        int templateOffset = 0;
        int tokenIndex = 0;
        foreach (Match match in ExprSegmentRegex().Matches(template))
        {
            safeTemplateBuilder.Append(template, templateOffset, match.Index - templateOffset);
            safeTemplateBuilder.Append("__EXPR_TOKEN_");
            safeTemplateBuilder.Append(tokenIndex);
            safeTemplateBuilder.Append("__");
            expressionTexts.Add(match.Groups[1].Value);
            templateOffset = match.Index + match.Length;
            tokenIndex++;
        }

        safeTemplateBuilder.Append(template, templateOffset, template.Length - templateOffset);
        string safeTemplate = safeTemplateBuilder.ToString();

        IDictionary<string, object?> values = ScopeFlattener.Flatten(scope);
        string formatted = Formatter.Format(safeTemplate, values);
        if (expressionTexts.Count is 0)
        {
            return formatted;
        }

        StringBuilder output = new(formatted.Length);
        int formattedOffset = 0;

        foreach (Match match in ExprTokenRegex().Matches(formatted))
        {
            output.Append(formatted, formattedOffset, match.Index - formattedOffset);
            if (!int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture,
                    out int exprIndex) || exprIndex < 0 || exprIndex >= expressionTexts.Count)
            {
                output.Append(match.Value);
                formattedOffset = match.Index + match.Length;
                continue;
            }

            object? value = await _expressionEngine.EvalAsync(expressionTexts[exprIndex], scope);
            output.Append(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
            formattedOffset = match.Index + match.Length;
        }

        output.Append(formatted, formattedOffset, formatted.Length - formattedOffset);
        return output.ToString();
    }

    private async Task<T?> EvaluateExpressionAsAsync<T>(string raw, object? scope)
    {
        Type targetType = typeof(T);
        Type nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        bool isEnumTarget = nonNullableType.IsEnum;

        string templateValue = await EvaluateTemplateAsync(raw, scope);
        if (TryConvert(templateValue, nonNullableType, out object? convertedTemplateValue))
        {
            return (T?)convertedTemplateValue;
        }

        if (isEnumTarget && !string.IsNullOrWhiteSpace(templateValue))
        {
            throw BuildInvalidEnumException(nonNullableType, templateValue, raw);
        }

        object? expressionValue = await _expressionEngine.EvalAsync(raw, scope);
        if (expressionValue is null)
        {
            return default;
        }

        if (targetType.IsInstanceOfType(expressionValue))
        {
            return (T)expressionValue;
        }

        if (TryConvert(expressionValue, nonNullableType, out object? convertedExpressionValue))
        {
            return (T?)convertedExpressionValue;
        }

        if (isEnumTarget)
        {
            throw BuildInvalidEnumException(nonNullableType, expressionValue, raw);
        }

        return (T?)Convert.ChangeType(expressionValue, nonNullableType, CultureInfo.InvariantCulture);
    }

    private static InvalidOperationException
        BuildInvalidEnumException(Type enumType, object? rawValue, string expression) => new(
        $"DSL[Enum] Invalid enum value. Context: enum='{enumType.Name}', value='{Convert.ToString(rawValue, CultureInfo.InvariantCulture)}', expression='{expression}', allowed='{string.Join(", ", Enum.GetNames(enumType))}'");

    private static bool TryConvert(object? value, Type targetType, out object? converted)
    {
        if (value is null)
        {
            converted = null;
            return false;
        }

        if (targetType == typeof(string))
        {
            converted = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            return true;
        }

        if (targetType == typeof(int) && int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
        {
            converted = intValue;
            return true;
        }

        if (targetType == typeof(float) && float.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
        {
            converted = floatValue;
            return true;
        }

        if (targetType == typeof(double) && double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
        {
            converted = doubleValue;
            return true;
        }

        if (targetType == typeof(bool) &&
            bool.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out bool boolValue))
        {
            converted = boolValue;
            return true;
        }

        if (targetType.IsEnum && Enum.TryParse(targetType, Convert.ToString(value, CultureInfo.InvariantCulture), true,
                out object? enumValue))
        {
            converted = enumValue;
            return true;
        }

        converted = null;
        return false;
    }

    [GeneratedRegex("__EXPR_TOKEN_(\\d+)__")]
    private static partial Regex ExprTokenRegex();
}

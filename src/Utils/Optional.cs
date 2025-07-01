using System.Text.Json;
using System.Text.Json.Serialization;

namespace Limekuma.Utils;

[JsonConverter(typeof(OptionalJsonConverter))]
public class Optional<TA, TB>
{
    private readonly TA? _valueA;
    private readonly TB? _valueB;

    public Optional(TA? value) => _valueA = value;
    public Optional(TB? value) => _valueB = value;

    public Optional(object? obj)
    {
        if (obj is null)
        {
            return;
        }

        if (obj is TA valueA)
        {
            _valueA = valueA;
        }

        if (obj is TB valueB)
        {
            _valueB = valueB;
        }
    }

    public object? Value
    {
        get
        {
            if (_valueA is not null)
            {
                return _valueA;
            }

            if (_valueB is not null)
            {
                return _valueB;
            }

            return null;
        }
    }

    public static implicit operator TA?(Optional<TA, TB> o) => o._valueA;

    public static implicit operator TB?(Optional<TA, TB> o) => o._valueB;

    public static implicit operator Optional<TA, TB>(TA a) => new(a);

    public static implicit operator Optional<TA, TB>(TB b) => new(b);

    public new Type? GetType() => Value?.GetType();

    public new string? ToString() => Value?.ToString();

    public new bool? Equals(object? obj) => Value?.Equals(obj);

    public new int? GetHashCode() => Value?.GetHashCode();
}

public class OptionalJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<,>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type[] typeArgs = typeToConvert.GetGenericArguments();
        Type converterType = typeof(OptionalJsonConverterInner<,>).MakeGenericType(typeArgs);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private class OptionalJsonConverterInner<TA, TB> : JsonConverter<Optional<TA, TB>>
    {
        public override Optional<TA, TB> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                TA? valueA = JsonSerializer.Deserialize<TA>(ref reader, options);
                return new(valueA);
            }
            catch (JsonException)
            {
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                TB? valueB = JsonSerializer.Deserialize<TB>(ref reader, options);
                return new(valueB);
            }
            catch (JsonException)
            {
            }
            catch (InvalidOperationException)
            {
            }

            return new(null);
        }

        public override void Write(Utf8JsonWriter writer, Optional<TA, TB> value, JsonSerializerOptions options)
        {
            if (value.Value is null)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}
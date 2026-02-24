using System.Text.Json;
using System.Text.Json.Serialization;

namespace Limekuma.Utils;

[JsonConverter(typeof(UnionJsonConverter))]
public class Union<TA, TB>
{
    private readonly TA? _valueA;
    private readonly TB? _valueB;

    public Union(TA? value) => _valueA = value;
    public Union(TB? value) => _valueB = value;

    public Union(object? obj)
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

    public static implicit operator TA?(Union<TA, TB> o) => o._valueA;

    public static implicit operator TB?(Union<TA, TB> o) => o._valueB;

    public static implicit operator Union<TA, TB>(TA a) => new(a);

    public static implicit operator Union<TA, TB>(TB b) => new(b);

    public new Type? GetType() => Value?.GetType();

    public new string? ToString() => Value?.ToString();

    public new bool? Equals(object? obj) => Value?.Equals(obj);

    public new int? GetHashCode() => Value?.GetHashCode();
}

public class UnionJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType &&
                                                           typeToConvert.GetGenericTypeDefinition() ==
                                                           typeof(Union<,>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type[] typeArgs = typeToConvert.GetGenericArguments();
        Type converterType = typeof(UnionJsonConverterInner<,>).MakeGenericType(typeArgs);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private class UnionJsonConverterInner<TA, TB> : JsonConverter<Union<TA, TB>>
    {
        public override Union<TA, TB> Read(ref Utf8JsonReader reader, Type typeToConvert,
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

        public override void Write(Utf8JsonWriter writer, Union<TA, TB> value, JsonSerializerOptions options)
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
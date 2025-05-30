using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DXKumaBot.Backend.Utils;

public class Optional<TA, TB>
{
    private readonly TA? _valueA;
    private readonly TB? _valueB;
    public Optional(TA? value) => _valueA = value;
    public Optional(TB? value) => _valueB = value;

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

    public new Type? GetType() => Value?.GetType();
}

public class OptionalTATBConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<,>);

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        Type[] typeArguments = type.GetGenericArguments();
        Type valueAType = typeArguments[0];
        Type valueBType = typeArguments[1];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(OptionsConverterInner<,>).MakeGenericType(
                [valueAType, valueBType]),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [options],
            culture: null)!;

        return converter;
    }

    private class OptionsConverterInner<TA, TB>(JsonSerializerOptions options) : JsonConverter<Optional<TA, TB>>
    {
        private readonly JsonConverter<TA> _valueAConverter = (JsonConverter<TA>)options.GetConverter(typeof(TA));
        private readonly JsonConverter<TB> _valueBConverter = (JsonConverter<TB>)options.GetConverter(typeof(TB));
        private readonly Type _valueAType = typeof(TA);
        private readonly Type _valueBType = typeof(TB);

        public override Optional<TA, TB> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Try to deserialize as TA
            try
            {
                TA? valueA = JsonSerializer.Deserialize<TA>(ref reader, options);
                return new Optional<TA, TB>(valueA);
            }
            catch { }

            // If deserialization as TA fails, try TB
            try
            {
                TB? valueB = JsonSerializer.Deserialize<TB>(ref reader, options);
                return new Optional<TA, TB>(valueB);
            }
            catch { }

            throw new JsonException($"Unable to deserialize value as either {_valueAType} or {_valueBType}");
        }

        public override void Write(Utf8JsonWriter writer, Optional<TA, TB> options1, JsonSerializerOptions options)
        {
            object? value = options1.Value;
            if (value is TA valueA)
            {
                _valueAConverter.Write(writer, valueA, options);
                return;
            }

            if (value is TB valueB)
            {
                _valueBConverter.Write(writer, valueB, options);
                return;
            }

            throw new JsonException($"Value is neither {_valueAType} nor {_valueBType}");
        }
    }
}

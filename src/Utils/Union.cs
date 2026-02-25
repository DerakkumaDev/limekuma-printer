using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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

public sealed class UnionJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType &&
        typeToConvert.GetGenericTypeDefinition() ==
        typeof(Union<,>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type[] typeArgs = typeToConvert.GetGenericArguments();
        Type converterType = typeof(UnionJsonConverterInner<,>).MakeGenericType(typeArgs);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class UnionJsonConverterInner<T1, T2> : JsonConverter<Union<T1, T2>>
    {
        public override Union<T1, T2> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new(null);
            }

            bool preferFirst = TryChooseFirstAdvanced(typeof(T1), typeof(T2), ref reader, options)
                               ?? ShouldChooseFirst(typeof(T1), typeof(T2), reader.TokenType);
            if (preferFirst)
            {
                T1? first = JsonSerializer.Deserialize<T1>(ref reader, options);
                return new(first);
            }
            T2? second = JsonSerializer.Deserialize<T2>(ref reader, options);
            return new(second);
        }

        public override void Write(Utf8JsonWriter writer, Union<T1, T2> value, JsonSerializerOptions options)
        {
            if (value.Value is null)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, value.Value, options);
        }

        private static bool? TryChooseFirstAdvanced(Type rawT1, Type rawT2, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Type t1 = UnwrapNullable(rawT1);
            Type t2 = UnwrapNullable(rawT2);
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    {
                        bool t1Obj = IsObjectLike(t1);
                        bool t2Obj = IsObjectLike(t2);
                        if (!t1Obj && !t2Obj)
                        {
                            return null;
                        }
                        if (t1Obj && !t2Obj)
                        {
                            return true;
                        }
                        if (!t1Obj && t2Obj)
                        {
                            return false;
                        }

                        HashSet<string> jsonProps = ReadObjectPropertyNames(ref reader, options.PropertyNameCaseInsensitive);
                        HashSet<string> t1Props = GetTypeJsonPropertyNames(t1, options);
                        HashSet<string> t2Props = GetTypeJsonPropertyNames(t2, options);

                        int m1 = 0;
                        foreach (var p in jsonProps)
                        {
                            if (t1Props.Contains(p))
                            {
                                m1++;
                            }
                        }
                        int m2 = 0;
                        foreach (var p in jsonProps)
                        {
                            if (t2Props.Contains(p))
                            {
                                m2++;
                            }
                        }

                        if (m1 > m2)
                        {
                            return true;
                        }
                        if (m2 > m1)
                        {
                            return false;
                        }

                        bool t1Dict = IsDictionaryLike(t1);
                        bool t2Dict = IsDictionaryLike(t2);
                        if (t1Dict != t2Dict)
                        {
                            if (m1 == 0 && m2 == 0)
                            {
                                return t1Dict;
                            }
                        }

                        return null;
                    }
                case JsonTokenType.StartArray:
                    {
                        bool t1Arr = IsArrayLike(t1);
                        bool t2Arr = IsArrayLike(t2);
                        if (!t1Arr && !t2Arr)
                        {
                            return null;
                        }
                        if (t1Arr && !t2Arr)
                        {
                            return true;
                        }
                        if (!t1Arr && t2Arr)
                        {
                            return false;
                        }

                        Utf8JsonReader copy = reader;
                        int depth = 0;
                        bool foundElement = false;
                        JsonTokenType elemToken = JsonTokenType.Null;
                        while (copy.Read())
                        {
                            if (copy.TokenType == JsonTokenType.StartArray)
                            {
                                depth++;
                                continue;
                            }
                            if (copy.TokenType == JsonTokenType.EndArray)
                            {
                                if (depth == 1)
                                {
                                    break;
                                }
                                depth--;
                                continue;
                            }
                            if (depth == 1)
                            {
                                if (copy.TokenType != JsonTokenType.Comment)
                                {
                                    elemToken = copy.TokenType;
                                    foundElement = true;
                                    break;
                                }
                            }
                        }

                        if (!foundElement)
                        {
                            return null;
                        }

                        Type? e1 = GetEnumerableElementType(t1);
                        Type? e2 = GetEnumerableElementType(t2);
                        if (e1 is null || e2 is null)
                        {
                            return null;
                        }
                        return ShouldChooseFirst(e1, e2, elemToken);
                    }
                default:
                    return null;
            }
        }

        private static bool ShouldChooseFirst(Type t1, Type t2, JsonTokenType kind)
        {
            t1 = UnwrapNullable(t1);
            t2 = UnwrapNullable(t2);
            switch (kind)
            {
                case JsonTokenType.String:
                    {
                        bool t1Match = IsStringLike(t1);
                        bool t2Match = IsStringLike(t2);
                        return t1Match switch
                        {
                            true when !t2Match => true,
                            false when t2Match => false,
                            _ => true
                        };
                    }
                case JsonTokenType.Number:
                    {
                        bool t1Match = IsNumericLike(t1) || t1.IsEnum;
                        bool t2Match = IsNumericLike(t2) || t2.IsEnum;
                        return t1Match switch
                        {
                            true when !t2Match => true,
                            false when t2Match => false,
                            _ => true
                        };
                    }
                case JsonTokenType.True:
                case JsonTokenType.False:
                    {
                        bool t1Match = IsBooleanLike(t1);
                        bool t2Match = IsBooleanLike(t2);
                        return t1Match switch
                        {
                            true when !t2Match => true,
                            false when t2Match => false,
                            _ => true
                        };
                    }
                case JsonTokenType.StartArray:
                    {
                        bool t1Match = IsArrayLike(t1);
                        bool t2Match = IsArrayLike(t2);
                        return t1Match switch
                        {
                            true when !t2Match => true,
                            false when t2Match => false,
                            _ => true
                        };
                    }
                case JsonTokenType.StartObject:
                    {
                        bool t1Match = IsObjectLike(t1);
                        bool t2Match = IsObjectLike(t2);
                        return t1Match switch
                        {
                            true when !t2Match => true,
                            false when t2Match => false,
                            _ => true
                        };
                    }
                case JsonTokenType.None:
                case JsonTokenType.EndObject:
                case JsonTokenType.EndArray:
                case JsonTokenType.PropertyName:
                case JsonTokenType.Comment:
                case JsonTokenType.Null:
                default:
                    return true;
            }
        }

        private static Type UnwrapNullable(Type t) => Nullable.GetUnderlyingType(t) ?? t;

        private static bool IsDictionaryLike(Type t)
        {
            if (typeof(IDictionary).IsAssignableFrom(t))
            {
                return true;
            }
            foreach (Type i in t.GetInterfaces())
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    return true;
                }
            }
            return false;
        }

        private static HashSet<string> GetTypeJsonPropertyNames(Type t, JsonSerializerOptions options)
        {
            StringComparer comparer = options.PropertyNameCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            HashSet<string> set = new(comparer);
            JsonTypeInfo? info;
            try
            {
                info = options.GetTypeInfo(t);
            }
            catch
            {
                info = null;
            }
            if (info is not null && info.Kind == JsonTypeInfoKind.Object)
            {
                foreach (JsonPropertyInfo p in info.Properties)
                {
                    if (!string.IsNullOrEmpty(p.Name))
                    {
                        set.Add(p.Name);
                    }
                }
                if (set.Count > 0)
                {
                    return set;
                }
            }

            foreach (PropertyInfo prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                {
                    continue;
                }
                if (prop.GetMethod is null && prop.SetMethod is null)
                {
                    continue;
                }
                JsonPropertyNameAttribute? nameAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                string name = nameAttr is not null
                    ? nameAttr.Name
                    : (options.PropertyNamingPolicy is null ? prop.Name : options.PropertyNamingPolicy.ConvertName(prop.Name));
                set.Add(name);
            }

            foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                {
                    continue;
                }
                if (field.GetCustomAttribute<JsonIncludeAttribute>() is null)
                {
                    continue;
                }
                JsonPropertyNameAttribute? nameAttr = field.GetCustomAttribute<JsonPropertyNameAttribute>();
                string name = nameAttr is not null
                    ? nameAttr.Name
                    : (options.PropertyNamingPolicy is null ? field.Name : options.PropertyNamingPolicy.ConvertName(field.Name));
                set.Add(name);
            }

            return set;
        }

        private static HashSet<string> ReadObjectPropertyNames(ref Utf8JsonReader reader, bool caseInsensitive)
        {
            StringComparer comparer = caseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            HashSet<string> names = new(comparer);
            Utf8JsonReader copy = reader;
            if (copy.TokenType != JsonTokenType.StartObject)
            {
                return names;
            }
            int depth = 0;
            bool started = false;
            while (true)
            {
                if (!copy.Read())
                {
                    break;
                }
                JsonTokenType token = copy.TokenType;
                if (token == JsonTokenType.StartObject || token == JsonTokenType.StartArray)
                {
                    depth++;
                    started = true;
                    continue;
                }
                if (token == JsonTokenType.EndObject || token == JsonTokenType.EndArray)
                {
                    if (started && depth == 0)
                    {
                        break;
                    }
                    if (depth > 0)
                    {
                        depth--;
                    }
                    continue;
                }
                if (token == JsonTokenType.PropertyName && depth == 0)
                {
                    string? name = copy.GetString();
                    if (name is not null)
                    {
                        names.Add(name);
                    }
                }
            }
            return names;
        }

        private static bool IsStringLike(Type t)
        {
            if (t == typeof(string))
            {
                return true;
            }

            if (t == typeof(Guid))
            {
                return true;
            }

            if (t == typeof(DateTime))
            {
                return true;
            }

            if (t == typeof(DateTimeOffset))
            {
                return true;
            }

            switch (t.FullName)
            {
                case "System.DateOnly":
                case "System.TimeOnly":
                    return true;
            }

            if (t == typeof(Uri))
            {
                return true;
            }

            return false;
        }

        private static bool IsNumericLike(Type t)
        {
            TypeCode code = Type.GetTypeCode(t);
            return code switch
            {
                TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32
                    or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double
                    or TypeCode.Decimal => true,
                _ => false
            };
        }

        private static bool IsBooleanLike(Type t) => t == typeof(bool);

        private static bool IsArrayLike(Type t)
        {
            if (t == typeof(string))
            {
                return false;
            }

            if (t.IsArray)
            {
                return true;
            }

            if (typeof(IDictionary).IsAssignableFrom(t))
            {
                return false;
            }

            foreach (Type i in t.GetInterfaces())
            {
                switch (i.IsGenericType)
                {
                    case true when i.GetGenericTypeDefinition() == typeof(IDictionary<,>):
                        return false;
                    case true when i.GetGenericTypeDefinition() == typeof(IEnumerable<>):
                        return true;
                }
            }

            return typeof(IEnumerable).IsAssignableFrom(t) &&
                   !typeof(IDictionary).IsAssignableFrom(t);
        }

        private static bool IsObjectLike(Type t)
        {
            if (IsStringLike(t))
            {
                return false;
            }

            if (IsNumericLike(t) || t.IsEnum)
            {
                return false;
            }

            if (IsBooleanLike(t))
            {
                return false;
            }

            if (IsArrayLike(t))
            {
                return false;
            }

            return true;
        }

        private static Type? GetEnumerableElementType(Type t)
        {
            if (t.IsArray)
            {
                return t.GetElementType();
            }
            foreach (Type i in t.GetInterfaces())
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return i.GetGenericArguments()[0];
                }
            }
            return null;
        }
    }
}
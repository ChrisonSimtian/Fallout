using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fallout.Common.Utilities;

namespace Fallout.Common.Tooling;

/// <summary>
/// Redirects (de)serialization of a type through one of its non-public fields.
/// Polymorphic over a closed type or an open generic definition — hence the factory shape.
/// </summary>
internal class ObjectFromFieldConverter(Type targetType, string fieldName) : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
            ? typeToConvert.GetGenericTypeDefinition() == targetType
            : typeToConvert.IsAssignableTo(targetType);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(TypedConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, fieldName).NotNull();
    }

    private class TypedConverter<T>(string fieldName) : JsonConverter<T>
    {
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var field = GetField(typeof(T));
            var fieldValue = field.GetValue(value).NotNull();
            JsonSerializer.Serialize(writer, fieldValue, field.FieldType, options);
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var instance = Activator.CreateInstance(typeToConvert).NotNull();
            var field = GetField(typeToConvert);
            var fieldValue = JsonSerializer.Deserialize(ref reader, field.FieldType, options);
            field.SetValue(instance, fieldValue);
            return (T)instance;
        }

        private FieldInfo GetField(Type type)
        {
            return (FieldInfo)type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(x => x.Name == fieldName);
        }
    }
}

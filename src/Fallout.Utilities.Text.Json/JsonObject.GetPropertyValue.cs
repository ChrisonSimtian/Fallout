using System.Text.Json.Nodes;

namespace Fallout.Common.Utilities;

public static partial class JsonNodeExtensions
{
    /// <summary>
    /// Reads property <paramref name="name"/> as <typeparamref name="T"/>, returning <c>default</c> if absent.
    /// </summary>
    public static T GetPropertyValueOrNull<T>(this JsonObject jsonObject, string name)
    {
        return jsonObject[name] is JsonNode node ? node.GetValue<T>() : default;
    }

    /// <summary>
    /// Reads property <paramref name="name"/> as <typeparamref name="T"/>, throwing if absent.
    /// </summary>
    public static T GetPropertyValue<T>(this JsonObject jsonObject, string name)
    {
        return jsonObject[name].NotNull($"Property '{name}' not found").GetValue<T>();
    }

    /// <summary>
    /// Reads property <paramref name="name"/> as a nested <see cref="JsonObject"/>, throwing if absent.
    /// </summary>
    public static JsonObject GetPropertyValue(this JsonObject jsonObject, string name)
    {
        return jsonObject[name].NotNull($"Property '{name}' not found").AsObject();
    }

    /// <summary>
    /// Reads property <paramref name="name"/> as a string, throwing if absent.
    /// </summary>
    public static string GetPropertyStringValue(this JsonObject jsonObject, string name)
    {
        return jsonObject.GetPropertyValue<string>(name);
    }
}

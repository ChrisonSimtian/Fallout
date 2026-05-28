using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fallout.Common.IO;

namespace Fallout.Common.Utilities;

public static class JsonExtensions
{
    public static JsonSerializerOptions DefaultSerializerOptions { get; } =
        new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

    /// <summary>
    /// Serializes an object as JSON string via System.Text.Json.
    /// </summary>
    public static string ToJson<T>(this T obj, JsonSerializerOptions options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? DefaultSerializerOptions);
    }

    /// <summary>
    /// Deserializes an object from a JSON string via System.Text.Json.
    /// </summary>
    public static T GetJson<T>(this string content, JsonSerializerOptions options = null)
    {
        return JsonSerializer.Deserialize<T>(content, options ?? DefaultSerializerOptions);
    }

    /// <summary>
    /// Parses a <see cref="JsonObject"/> from a JSON string.
    /// </summary>
    public static JsonObject GetJsonObject(this string content, JsonNodeOptions? nodeOptions = null)
    {
        return JsonNode.Parse(content, nodeOptions: nodeOptions)?.AsObject();
    }

    /// <summary>
    /// Serializes an object as JSON to a file via System.Text.Json.
    /// </summary>
    public static AbsolutePath WriteJson<T>(this AbsolutePath path, T obj, JsonSerializerOptions options = null)
    {
        return path.WriteAllText(obj.ToJson(options));
    }

    /// <summary>
    /// Deserializes an object as JSON from a file via System.Text.Json.
    /// </summary>
    public static T ReadJson<T>(this AbsolutePath path, JsonSerializerOptions options = null)
    {
        return path.ReadAllText().GetJson<T>(options);
    }

    /// <summary>
    /// Parses a <see cref="JsonObject"/> from a file.
    /// </summary>
    public static JsonObject ReadJsonObject(this AbsolutePath path, JsonNodeOptions? nodeOptions = null)
    {
        return path.ReadAllText().GetJsonObject(nodeOptions);
    }

    /// <summary>
    /// Deserializes from a file, applies updates, and writes back — System.Text.Json variant.
    /// </summary>
    public static AbsolutePath UpdateJson<T>(this AbsolutePath path, Action<T> update, JsonSerializerOptions options = null)
    {
        var obj = path.ReadJson<T>(options);
        update.Invoke(obj);
        return path.WriteJson(obj, options);
    }

    /// <summary>
    /// Parses a <see cref="JsonObject"/> from a file, applies updates, and writes back.
    /// </summary>
    public static AbsolutePath UpdateJsonObject(
        this AbsolutePath path,
        Action<JsonObject> update,
        JsonNodeOptions? nodeOptions = null)
    {
        var obj = path.ReadJsonObject(nodeOptions) ?? new JsonObject();
        update.Invoke(obj);
        return path.WriteAllText(obj.ToJsonString(DefaultSerializerOptions));
    }
}

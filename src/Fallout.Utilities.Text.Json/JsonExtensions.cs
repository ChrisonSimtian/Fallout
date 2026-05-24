// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fallout.Common.IO;
using Fallout.Common.Tooling;

namespace Fallout.Common.Utilities;

public static class JsonExtensions
{
    public static JsonSerializerSettings DefaultSerializerSettings =
        new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new AllWritableContractResolver()
        };

    public static JsonSerializerOptions DefaultSerializerOptions { get; } =
        new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

    // ─────────────────────────────────────────────────────────────────────
    // Newtonsoft surface — [Obsolete] for v11 removal as part of #83.
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Serializes an object as JSON string via Newtonsoft.Json.
    /// </summary>
    [Obsolete("Use the JsonSerializerOptions overload instead. Newtonsoft.Json surface is scheduled for removal in v11 as part of the System.Text.Json migration (#83).")]
    public static string ToJson<T>(
        this T obj,
        JsonSerializerSettings serializerSettings = null,
        Formatting formatting = Formatting.Indented)
    {
        return JsonConvert.SerializeObject(obj, formatting, serializerSettings ?? DefaultSerializerSettings);
    }

    /// <summary>
    /// Deserializes an object from a JSON string via Newtonsoft.Json.
    /// </summary>
    [Obsolete("Use the JsonSerializerOptions overload instead. Newtonsoft.Json surface is scheduled for removal in v11 as part of the System.Text.Json migration (#83).")]
    public static T GetJson<T>(this string content, JsonSerializerSettings serializerSettings = null)
    {
        return JsonConvert.DeserializeObject<T>(content, serializerSettings ?? DefaultSerializerSettings);
    }

    /// <summary>
    /// Deserializes a Newtonsoft <see cref="JObject"/> from a JSON string.
    /// </summary>
    [Obsolete("Use GetJsonObject (returns System.Text.Json.Nodes.JsonObject) instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static JObject GetJson(this string content, JsonSerializerSettings serializerSettings = null)
    {
#pragma warning disable CS0618 // Self-call into the obsolete generic overload; both retire together in v11.
        return content.GetJson<JObject>(serializerSettings);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Serializes an object as JSON to a file via Newtonsoft.Json.
    /// </summary>
    [Obsolete("Use the JsonSerializerOptions overload instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static AbsolutePath WriteJson<T>(this AbsolutePath path, T obj, JsonSerializerSettings serializerSettings)
    {
#pragma warning disable CS0618 // ToJson (Newtonsoft overload) retires alongside.
        var content = obj.ToJson(serializerSettings);
#pragma warning restore CS0618
        return path.WriteAllText(content);
    }

    /// <summary>
    /// Deserializes an object as JSON from a file via Newtonsoft.Json.
    /// </summary>
    [Obsolete("Use the JsonSerializerOptions overload instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static T ReadJson<T>(this AbsolutePath path, JsonSerializerSettings serializerSettings)
    {
        var content = path.ReadAllText();
#pragma warning disable CS0618 // GetJson (Newtonsoft overload) retires alongside.
        return content.GetJson<T>(serializerSettings);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Deserializes a Newtonsoft <see cref="JObject"/> from a file.
    /// </summary>
    [Obsolete("Use ReadJsonObject (returns System.Text.Json.Nodes.JsonObject) instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static JObject ReadJson(this AbsolutePath path, JsonSerializerSettings serializerSettings = null)
    {
#pragma warning disable CS0618 // ReadJson<T>(Newtonsoft) retires alongside.
        return path.ReadJson<JObject>(serializerSettings);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Deserializes an object as JSON from a file, applies updates, and serializes it back via Newtonsoft.Json.
    /// </summary>
    [Obsolete("Use the JsonSerializerOptions overload instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static AbsolutePath UpdateJson<T>(
        this AbsolutePath path,
        Action<T> update,
        JsonSerializerSettings serializerSettings = null)
    {
        var before = path.ReadAllText();
#pragma warning disable CS0618 // Newtonsoft helpers retire together.
        var obj = before.GetJson<T>(serializerSettings);
        update.Invoke(obj);
        var after = obj.ToJson(serializerSettings);
#pragma warning restore CS0618
        return path.WriteAllText(after);
    }

    /// <summary>
    /// Deserializes a Newtonsoft <see cref="JObject"/> from a file, applies updates, and writes it back.
    /// </summary>
    [Obsolete("Use UpdateJsonObject (takes Action<JsonObject>) instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static AbsolutePath UpdateJson(
        this AbsolutePath path,
        Action<JObject> update,
        JsonSerializerSettings serializerSettings = null)
    {
#pragma warning disable CS0618 // UpdateJson<T>(Newtonsoft) retires alongside.
        return path.UpdateJson<JObject>(update, serializerSettings);
#pragma warning restore CS0618
    }

    // ─────────────────────────────────────────────────────────────────────
    // System.Text.Json surface — preferred. STJ overloads require explicit
    // JsonSerializerOptions to avoid ambiguity with the Newtonsoft no-args
    // path; callers wanting framework defaults should pass DefaultSerializerOptions.
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Serializes an object as JSON string via System.Text.Json.
    /// </summary>
    public static string ToJson<T>(this T obj, JsonSerializerOptions options)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, options ?? DefaultSerializerOptions);
    }

    /// <summary>
    /// Deserializes an object from a JSON string via System.Text.Json.
    /// </summary>
    public static T GetJson<T>(this string content, JsonSerializerOptions options)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(content, options ?? DefaultSerializerOptions);
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
    public static AbsolutePath WriteJson<T>(this AbsolutePath path, T obj, JsonSerializerOptions options)
    {
        var content = obj.ToJson(options);
        return path.WriteAllText(content);
    }

    /// <summary>
    /// Deserializes an object as JSON from a file via System.Text.Json.
    /// </summary>
    public static T ReadJson<T>(this AbsolutePath path, JsonSerializerOptions options)
    {
        var content = path.ReadAllText();
        return content.GetJson<T>(options);
    }

    /// <summary>
    /// Parses a <see cref="JsonObject"/> from a file.
    /// </summary>
    public static JsonObject ReadJsonObject(this AbsolutePath path, JsonNodeOptions? nodeOptions = null)
    {
        var content = path.ReadAllText();
        return content.GetJsonObject(nodeOptions);
    }

    /// <summary>
    /// Deserializes from a file, applies updates, and writes back — System.Text.Json variant.
    /// </summary>
    public static AbsolutePath UpdateJson<T>(this AbsolutePath path, Action<T> update, JsonSerializerOptions options)
    {
        var before = path.ReadAllText();
        var obj = before.GetJson<T>(options);
        update.Invoke(obj);
        var after = obj.ToJson(options);
        return path.WriteAllText(after);
    }

    /// <summary>
    /// Parses a <see cref="JsonObject"/> from a file, applies updates, and writes back.
    /// </summary>
    public static AbsolutePath UpdateJsonObject(
        this AbsolutePath path,
        Action<JsonObject> update,
        JsonNodeOptions? nodeOptions = null)
    {
        var content = path.ReadAllText();
        var obj = JsonNode.Parse(content, nodeOptions: nodeOptions)?.AsObject() ?? new JsonObject();
        update.Invoke(obj);
        var after = obj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        return path.WriteAllText(after);
    }
}

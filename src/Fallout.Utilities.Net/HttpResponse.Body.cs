// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fallout.Common.IO;

namespace Fallout.Common.Utilities.Net;

// TODO: reduce with overloads
public static partial class HttpResponseExtensions
{
    /// <summary>
    /// Reads the HTTP response body as JSON via <see cref="Newtonsoft.Json.JsonConvert.DeserializeObject{T}(string)"/>.
    /// </summary>
    public static async Task<T> GetBodyAsJson<T>(this HttpResponseInspector inspector)
    {
        return JsonConvert.DeserializeObject<T>(await inspector.GetBodyAsync());
    }

    /// <summary>
    /// Reads the HTTP response body as a Newtonsoft <see cref="JObject"/>.
    /// </summary>
    [Obsolete("Use GetBodyAsJsonObject (returns System.Text.Json.Nodes.JsonObject) instead. Newtonsoft.Json.Linq.JObject is scheduled for removal in v11 as part of the System.Text.Json migration (#83).")]
    public static async Task<JObject> GetBodyAsJson(this HttpResponseInspector inspector)
    {
        return await inspector.GetBodyAsJson<JObject>();
    }

    /// <summary>
    /// Reads the HTTP response body as JSON via <see cref="Newtonsoft.Json.JsonConvert.DeserializeObject{T}(string, JsonSerializerSettings)"/>.
    /// </summary>
    [Obsolete("Use the JsonSerializerOptions overload instead. Newtonsoft.Json.JsonSerializerSettings is scheduled for removal in v11 as part of the System.Text.Json migration (#83).")]
    public static async Task<T> GetBodyAsJson<T>(this HttpResponseInspector inspector, JsonSerializerSettings settings)
    {
        return JsonConvert.DeserializeObject<T>(await inspector.GetBodyAsync(), settings);
    }

    /// <summary>
    /// Reads the HTTP response body as a Newtonsoft <see cref="JObject"/> using the given <paramref name="settings"/>.
    /// </summary>
    [Obsolete("Use GetBodyAsJsonObject with JsonSerializerOptions instead. Newtonsoft types are scheduled for removal in v11 as part of the System.Text.Json migration (#83).")]
    public static async Task<JObject> GetBodyAsJson(this HttpResponseInspector inspector, JsonSerializerSettings settings)
    {
#pragma warning disable CS0618 // Self-call into the obsolete settings-taking generic overload; both retire together in v11.
        return await inspector.GetBodyAsJson<JObject>(settings);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Reads the HTTP response body as JSON via <see cref="System.Text.Json.JsonSerializer.Deserialize{TValue}(string, JsonSerializerOptions?)"/>.
    /// </summary>
    public static async Task<T> GetBodyAsJson<T>(this HttpResponseInspector inspector, JsonSerializerOptions options)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(await inspector.GetBodyAsync(), options);
    }

    /// <summary>
    /// Reads the HTTP response body as a System.Text.Json <see cref="JsonObject"/>.
    /// </summary>
    public static async Task<JsonObject> GetBodyAsJsonObject(this HttpResponseInspector inspector)
    {
        return JsonNode.Parse(await inspector.GetBodyAsync())?.AsObject();
    }

    /// <summary>
    /// Reads the HTTP response body as a System.Text.Json <see cref="JsonObject"/> using the given <paramref name="options"/>.
    /// </summary>
    public static async Task<JsonObject> GetBodyAsJsonObject(this HttpResponseInspector inspector, JsonNodeOptions options)
    {
        return JsonNode.Parse(await inspector.GetBodyAsync(), nodeOptions: options)?.AsObject();
    }

    public static async Task WriteToFile(this HttpResponseInspector inspector, AbsolutePath path, FileMode mode = FileMode.CreateNew)
    {
        using var fileStream = File.Open(path, mode);
        await inspector.Response.Content.CopyToAsync(fileStream);
    }
}

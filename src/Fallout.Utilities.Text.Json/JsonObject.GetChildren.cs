using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Fallout.Common.Utilities;

public static partial class JsonNodeExtensions
{
    /// <summary>
    /// Reads property <paramref name="name"/> as a <see cref="JsonArray"/> and returns its elements typed as <typeparamref name="T"/>.
    /// </summary>
    public static IEnumerable<T> GetChildren<T>(this JsonObject jsonObject, string name)
        where T : JsonNode
    {
        var array = jsonObject[name] as JsonArray;
        return array?.OfType<T>() ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Reads property <paramref name="name"/> as a <see cref="JsonArray"/> of <see cref="JsonObject"/> children.
    /// </summary>
    public static IEnumerable<JsonObject> GetChildren(this JsonObject jsonObject, string name)
    {
        return jsonObject.GetChildren<JsonObject>(name);
    }
}

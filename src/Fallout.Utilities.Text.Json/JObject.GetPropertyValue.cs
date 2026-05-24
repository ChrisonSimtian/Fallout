// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Fallout.Common.Utilities;

public static partial class JObjectExtensions
{
    [Obsolete("Use the JsonObject overload in JsonNodeExtensions instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static T GetPropertyValueOrNull<T>(this JObject jobject, string name)
    {
        var property = jobject.Property(name);
        return property != null
            ? property.Value.Value<T>()
            : default;
    }

    [Obsolete("Use the JsonObject overload in JsonNodeExtensions instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static T GetPropertyValue<T>(this JObject jobject, string name)
    {
        var property = jobject.Property(name).NotNull($"Property '{name}' not found");
        return property.Value.Value<T>();
    }

    [Obsolete("Use the JsonObject overload in JsonNodeExtensions instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static JObject GetPropertyValue(this JObject jobject, string name)
    {
#pragma warning disable CS0618 // Newtonsoft helpers retire together.
        return jobject.GetPropertyValue<JObject>(name);
#pragma warning restore CS0618
    }

    [Obsolete("Use the JsonObject overload in JsonNodeExtensions instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static string GetPropertyStringValue(this JObject jobject, string name)
    {
#pragma warning disable CS0618 // Newtonsoft helpers retire together.
        return jobject.GetPropertyValue<string>(name);
#pragma warning restore CS0618
    }
}

// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using Newtonsoft.Json.Linq;

namespace Fallout.Common.Utilities;

public static partial class JObjectExtensions
{
    [Obsolete("Use the JsonObject overload in JsonNodeExtensions instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static JEnumerable<T> GetChildren<T>(this JObject jobject, string name)
        where T : JToken
    {
#pragma warning disable CS0618 // Newtonsoft helpers retire together.
        return jobject.GetPropertyValue<JArray>(name).Children<T>();
#pragma warning restore CS0618
    }

    [Obsolete("Use the JsonObject overload in JsonNodeExtensions instead. Newtonsoft.Json surface is scheduled for removal in v11 (#83).")]
    public static JEnumerable<JObject> GetChildren(this JObject jobject, string name)
    {
#pragma warning disable CS0618 // Newtonsoft helpers retire together.
        return jobject.GetChildren<JObject>(name);
#pragma warning restore CS0618
    }
}

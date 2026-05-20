// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using Newtonsoft.Json.Linq;

namespace Fallout.Common.Utilities;

public static partial class JObjectExtensions
{
    public static JEnumerable<T> GetChildren<T>(this JObject jobject, string name)
        where T : JToken
    {
        return jobject.GetPropertyValue<JArray>(name).Children<T>();
    }

    public static JEnumerable<JObject> GetChildren(this JObject jobject, string name)
    {
        return jobject.GetChildren<JObject>(name);
    }
}

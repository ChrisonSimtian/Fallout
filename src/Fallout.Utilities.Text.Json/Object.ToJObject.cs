// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fallout.Common.Utilities;

// Renamed from ObjectExtensions in 2026-05: the Fallout.Utilities assembly
// declares a same-named partial class under the same namespace (Apply/When/
// Clone). Two assemblies declaring `Fallout.Common.Utilities.ObjectExtensions`
// blocks the TransitionShimGenerator (CS0433 if it tried to delegate). Since
// ToJObject is only ever called as an extension method, the class name is
// invisible to callers and a rename is the cleanest fix.
public static class JsonObjectExtensions
{
    public static JObject ToJObject(this object obj, JsonSerializer serializer = null)
    {
        serializer ??= JsonSerializer.CreateDefault();
        return JObject.FromObject(obj, serializer);
    }
}

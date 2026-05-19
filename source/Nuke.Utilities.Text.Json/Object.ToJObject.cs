// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nuke.Common.Utilities;

public static class ObjectExtensions
{
    public static JObject ToJObject(this object obj, JsonSerializer serializer = null)
    {
        serializer ??= JsonSerializer.CreateDefault();
        return JObject.FromObject(obj, serializer);
    }
}

// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using Newtonsoft.Json;
using System;

namespace Fallout.Common.Tools.Teams;

public partial class TeamsMessage
{
    [JsonProperty("@type")]
    internal string Type => "MessageCard";
    [JsonProperty("@context")]
    internal string Context => "http://schema.org/extensions";
}

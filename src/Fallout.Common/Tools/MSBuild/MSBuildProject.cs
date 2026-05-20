// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using JetBrains.Annotations;

namespace Fallout.Common.Tools.MSBuild;

[PublicAPI]
public class MSBuildProject : DynamicObject
{
    internal MSBuildProject(
        bool isSdkProject,
        IReadOnlyDictionary<string, string> properties,
        ILookup<string, string> itemGroups)
    {
        IsSdkProject = isSdkProject;
        Properties = properties;
        ItemGroups = itemGroups;
    }

    public bool IsSdkProject { get; }
    public bool IsLegacyProject => !IsSdkProject;
    public IReadOnlyDictionary<string, string> Properties { get; }
    public ILookup<string, string> ItemGroups { get; }
}

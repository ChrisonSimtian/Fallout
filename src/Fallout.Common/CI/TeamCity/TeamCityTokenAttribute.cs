// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Fallout.Common.CI.TeamCity;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TeamCityTokenAttribute : Attribute
{
    public TeamCityTokenAttribute(string name, string guid)
    {
        Name = name;
        Guid = guid;
    }

    public string Name { get; }
    public string Guid { get; }
}

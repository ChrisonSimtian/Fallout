// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Fallout.Common.CI.AppVeyor;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AppVeyorSecretAttribute : Attribute
{
    public AppVeyorSecretAttribute(string parameter, string value)
    {
        Parameter = parameter;
        Value = value;
    }

    public string Parameter { get; }
    public string Value { get; }
}

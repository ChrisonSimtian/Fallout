// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Fallout.Common.Utilities;

namespace Fallout.Common.Tooling;

// TODO: rename to TaskOptions / CommandOptions ?
[PublicAPI]
public abstract partial class ToolOptions : Options
{
    internal static event EventHandler Created;

    private readonly Dictionary<string, PropertyInfo> _allProperties;

    protected ToolOptions()
    {
        _allProperties = GetType().GetAllMembers(x => x is PropertyInfo, ReflectionUtility.Instance, allowAmbiguity: true)
            .Cast<PropertyInfo>().ToDictionary(x => x.Name, x => x);

        Set(() => ProcessEnvironmentVariables, EnvironmentInfo.Variables);
        Created?.Invoke(this, EventArgs.Empty);
    }

    internal partial IEnumerable<string> GetArguments();
    internal partial IEnumerable<string> GetSecrets();
    internal partial Action<OutputType, string> GetLogger();
}

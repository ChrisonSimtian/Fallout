// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;

namespace Nuke.Common;

// Parameter / Secret injection attributes — most consumer Build.cs files use these.

/// <summary>Transition shim for <see cref="Fallout.Common.ParameterAttribute"/>.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class ParameterAttribute : Fallout.Common.ParameterAttribute
{
    public ParameterAttribute(string description = null) : base(description) { }
}

/// <summary>Transition shim for <see cref="Fallout.Common.SecretAttribute"/>.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SecretAttribute : Fallout.Common.SecretAttribute
{
}

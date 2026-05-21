// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;

namespace Nuke.Common.ProjectModel;

/// <summary>Transition shim for <see cref="Fallout.Common.ProjectModel.SolutionAttribute"/>.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SolutionAttribute : Fallout.Common.ProjectModel.SolutionAttribute
{
    public SolutionAttribute() { }
    public SolutionAttribute(string relativePath) : base(relativePath) { }
}

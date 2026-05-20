// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Reflection;
using Fallout.Common.ValueInjection;

namespace Fallout.Common.Tooling;

public abstract class ToolInjectionAttributeBase : ValueInjectionAttributeBase
{
    public override bool SuppressWarnings => true;

    public abstract ToolRequirement GetRequirement(MemberInfo member);
}

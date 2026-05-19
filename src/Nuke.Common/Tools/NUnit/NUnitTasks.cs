// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using JetBrains.Annotations;
using Nuke.Common.Tooling;

namespace Nuke.Common.Tools.NUnit;

[PublicAPI]
public class NUnitVerbosityMappingAttribute : VerbosityMappingAttribute
{
    public NUnitVerbosityMappingAttribute()
        : base(typeof(NUnitTraceLevel))
    {
        Quiet = nameof(NUnitTraceLevel.Off);
        Minimal = nameof(NUnitTraceLevel.Warning);
        Normal = nameof(NUnitTraceLevel.Info);
        Verbose = nameof(NUnitTraceLevel.Verbose);
    }
}

// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using JetBrains.Annotations;
using Fallout.Common.Tooling;

namespace Fallout.Common.Tools.ReportGenerator;

[PublicAPI]
public class ReportGeneratorVerbosityMappingAttribute : VerbosityMappingAttribute
{
    public ReportGeneratorVerbosityMappingAttribute()
        : base(typeof(ReportGeneratorVerbosity))
    {
        Quiet = nameof(ReportGeneratorVerbosity.Off);
        Minimal = nameof(ReportGeneratorVerbosity.Warning);
        Normal = nameof(ReportGeneratorVerbosity.Info);
        Verbose = nameof(ReportGeneratorVerbosity.Verbose);
    }
}

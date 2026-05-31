using Fallout.Application.Tooling;

namespace Fallout.Common.Tools.NUnit;

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

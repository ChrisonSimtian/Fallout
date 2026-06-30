using Fallout.Common.Tooling;

using Fallout.Common;
namespace Fallout.Application.Tools.Testing.NUnit;

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

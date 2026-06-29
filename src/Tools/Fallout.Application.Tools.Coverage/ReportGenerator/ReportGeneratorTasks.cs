using Fallout.Common.Tooling;

namespace Fallout.Common.Tools.ReportGenerator;

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

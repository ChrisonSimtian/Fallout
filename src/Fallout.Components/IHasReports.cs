using System;
using System.Linq;
using Fallout.Kernel.IO;

namespace Fallout.Application.Components;

public interface IHasReports : IHasArtifacts
{
    AbsolutePath ReportDirectory => ArtifactsDirectory / "reports";
}

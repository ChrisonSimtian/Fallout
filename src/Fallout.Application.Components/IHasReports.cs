using System;
using System.Linq;
using Fallout.Core.IO;

namespace Fallout.Application.Components;

public interface IHasReports : IHasArtifacts
{
    AbsolutePath ReportDirectory => ArtifactsDirectory / "reports";
}

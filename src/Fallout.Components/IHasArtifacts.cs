using System;
using System.Linq;
using Fallout.Common.IO;
using Fallout.Application;

namespace Fallout.Application.Components;

public interface IHasArtifacts : IFalloutBuild
{
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
}

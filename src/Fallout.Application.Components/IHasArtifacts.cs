using System;
using System.Linq;
using Fallout.Application;
using Fallout.Core.IO;

namespace Fallout.Application.Components;

public interface IHasArtifacts : IFalloutBuild
{
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
}

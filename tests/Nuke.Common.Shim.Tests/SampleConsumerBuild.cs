// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE
//
// This file is the **compile-only test** for the Nuke.Common shim. It mirrors what a
// typical NUKE-era consumer Build.cs imports. If this file compiles, the MVP shim covers
// the surface it claims to cover. Nothing here is executed.

#pragma warning disable CS0649  // unassigned readonly — these are injected at runtime
#pragma warning disable CS0169  // unused fields — same reason

using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;

namespace Nuke.Common.Shim.Tests;

// Consumer's typical Build.cs entry-point — inherits the shim's NukeBuild,
// uses the canonical Fallout types via `Fallout.Common.Target` (shim doesn't
// bridge delegates; that's `fallout-migrate`'s job).
public abstract class SampleConsumerBuild : NukeBuild, INukeBuild
{
    [Parameter("Configuration to build")] readonly string Configuration;
    [Parameter] readonly bool RunTests;
    [Secret] readonly string NuGetApiKey;
    [Solution] readonly Fallout.Common.ProjectModel.Solution Solution;
    [Solution("path/to/explicit.slnx")] readonly Fallout.Common.ProjectModel.Solution ExplicitSolution;
    [GitRepository] readonly Fallout.Common.Git.GitRepository GitRepository;
}

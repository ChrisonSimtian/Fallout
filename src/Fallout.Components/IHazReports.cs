// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.IO;

namespace Fallout.Components;

[PublicAPI]
public interface IHazReports : IHazArtifacts
{
    AbsolutePath ReportDirectory => ArtifactsDirectory / "reports";
}

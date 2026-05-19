// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.IO;
using Nuke.Common.Tools.ReSharper;
using static Nuke.Common.Tools.ReSharper.ReSharperTasks;

namespace Nuke.Components;

[PublicAPI]
public interface IReportDuplicates : IHazReports, IHazSolution
{
    AbsolutePath DupFinderReportFile => ReportDirectory / "dupfinder.xml";

    Target ReportDuplicates => _ => _
        .TryAfter<ITest>()
        .Executes(() =>
        {
            ReSharperDupFinder(_ => _
                .SetSource(Solution)
                .SetOutputFile(DupFinderReportFile)
                .EnableShowText()
                .SetExcludeFiles(
                    "**/*.Generated.cs",
                    "**/obj/**",
                    "**/bin/**"));

            TeamCity.Instance?.ImportData(TeamCityImportType.DotNetDupFinder, DupFinderReportFile);
        });
}

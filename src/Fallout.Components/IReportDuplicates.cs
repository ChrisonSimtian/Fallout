// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common;
using Fallout.Common.CI.TeamCity;
using Fallout.Common.IO;
using Fallout.Common.Tools.ReSharper;
using static Fallout.Common.Tools.ReSharper.ReSharperTasks;

namespace Fallout.Components;

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

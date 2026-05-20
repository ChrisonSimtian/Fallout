// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.TeamCity.Configuration;

[PublicAPI]
public class TeamCityConfiguration : ConfigurationEntity
{
    public virtual string[] Header =>
        new[]
        {
            "import jetbrains.buildServer.configs.kotlin.v2018_1.*",
            "import jetbrains.buildServer.configs.kotlin.v2018_1.buildFeatures.*",
            "import jetbrains.buildServer.configs.kotlin.v2018_1.buildSteps.*",
            "import jetbrains.buildServer.configs.kotlin.v2018_1.triggers.*",
            "import jetbrains.buildServer.configs.kotlin.v2018_1.vcs.*",
            "",
            $"version = {Version.DoubleQuote()}",
            ""
        };

    public string Version { get; set; }
    public TeamCityProject Project { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        Header.ForEach(writer.WriteLine);
        Project.Write(writer);
        Project.VcsRoot.Write(writer);
        Project.BuildTypes.ForEach(x => x.Write(writer));
    }
}

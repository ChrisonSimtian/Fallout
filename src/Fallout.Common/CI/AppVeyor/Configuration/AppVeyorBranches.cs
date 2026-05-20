// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.AppVeyor.Configuration;

[PublicAPI]
public class AppVeyorBranches : ConfigurationEntity
{
    public string[] Only { get; set; }
    public string[] Except { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        if (Only.Length > 0)
        {
            using (writer.WriteBlock("only:"))
            {
                Only.ForEach(x => writer.WriteLine($"- {x}"));
            }
        }

        if (Except.Length > 0)
        {
            using (writer.WriteBlock("except:"))
            {
                Except.ForEach(x => writer.WriteLine($"- {x}"));
            }
        }
    }
}

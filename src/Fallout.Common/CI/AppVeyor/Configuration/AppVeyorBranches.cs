using System;
using System.Linq;
using Fallout.Application.CI;
using Fallout.Application.Utilities;
using Fallout.Kernel.Collections;

namespace Fallout.Common.CI.AppVeyor.Configuration;

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

using System;
using System.Linq;
using Fallout.Application.Utilities;
using Fallout.Kernel;

namespace Fallout.Infrastructure.CI.SpaceAutomation.Configuration;

public class SpaceAutomationCronScheduleTrigger : SpaceAutomationTrigger
{
    public string CronExpression { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"schedule {{ cron({CronExpression.DoubleQuote()}) }}");
    }
}

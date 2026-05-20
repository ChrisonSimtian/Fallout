// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.Utilities;

namespace Fallout.Common.CI.SpaceAutomation.Configuration;

[PublicAPI]
public class SpaceAutomationCronScheduleTrigger : SpaceAutomationTrigger
{
    public string CronExpression { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"schedule {{ cron({CronExpression.DoubleQuote()}) }}");
    }
}

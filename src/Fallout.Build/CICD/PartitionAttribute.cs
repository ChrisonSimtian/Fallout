// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Reflection;

namespace Fallout.Common.CI;

public class PartitionAttribute : ParameterAttribute
{
    public PartitionAttribute(int total)
    {
        Total = total;
    }

    public int Total { get; }

    public override bool List => false;

    public override object GetValue(MemberInfo member, object instance)
    {
        var part = ParameterService.GetParameter<int?>(member);
        return part.HasValue
            ? new Partition { Part = part.Value, Total = Total }
            : Partition.Single;
    }
}
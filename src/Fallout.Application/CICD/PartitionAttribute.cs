using System;
using System.Linq;
using System.Reflection;
using Fallout.Application;

namespace Fallout.Application.CI;

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
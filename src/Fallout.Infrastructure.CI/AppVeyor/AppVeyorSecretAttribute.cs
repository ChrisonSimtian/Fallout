using System;
using System.Linq;

namespace Fallout.Infrastructure.CI.AppVeyor;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AppVeyorSecretAttribute : Attribute
{
    public AppVeyorSecretAttribute(string parameter, string value)
    {
        Parameter = parameter;
        Value = value;
    }

    public string Parameter { get; }
    public string Value { get; }
}

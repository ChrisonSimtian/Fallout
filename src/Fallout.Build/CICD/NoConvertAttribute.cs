using System;
using System.Linq;

namespace Fallout.Common.CI;

[AttributeUsage(AttributeTargets.Property)]
public class NoConvertAttribute : Attribute
{
}

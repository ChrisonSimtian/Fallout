using System;
using System.Linq;

namespace Fallout.Application.CI;

[AttributeUsage(AttributeTargets.Property)]
public class NoConvertAttribute : Attribute
{
}

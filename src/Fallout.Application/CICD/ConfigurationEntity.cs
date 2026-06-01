using System;
using System.Linq;
using Fallout.Application.Utilities;

namespace Fallout.Application.CI;

public abstract class ConfigurationEntity
{
    public abstract void Write(CustomFileWriter writer);
}

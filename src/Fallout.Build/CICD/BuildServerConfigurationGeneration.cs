using System;
using System.Linq;
using Fallout.Application;

namespace Fallout.Application.CI;

public static class BuildServerConfigurationGeneration
{
    public static bool IsActive { get; } = ParameterService.GetParameter<string>(ConfigurationParameterName) != null;

    public const string ConfigurationParameterName = "generate-configuration";
}

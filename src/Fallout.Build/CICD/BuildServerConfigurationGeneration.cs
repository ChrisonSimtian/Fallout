// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Fallout.Common.CI;

public static class BuildServerConfigurationGeneration
{
    public static bool IsActive { get; } = ParameterService.GetParameter<string>(ConfigurationParameterName) != null;

    public const string ConfigurationParameterName = "generate-configuration";
}

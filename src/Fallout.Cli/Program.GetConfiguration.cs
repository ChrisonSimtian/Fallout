using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Cli;

partial class Program
{
    // internal (not private): shared with AddPackage/Update/Cake; will move into a configuration
    // service in the final #392 collapse PR.
    internal const string BUILD_PROJECT_FILE = nameof(BUILD_PROJECT_FILE);
    private const string TEMP_DIRECTORY = nameof(TEMP_DIRECTORY);
    private const string DOTNET_GLOBAL_FILE = nameof(DOTNET_GLOBAL_FILE);
    private const string DOTNET_INSTALL_URL = nameof(DOTNET_INSTALL_URL);
    private const string DOTNET_CHANNEL = nameof(DOTNET_CHANNEL);

    // Residual after the :get-configuration command moved to GetConfigurationCommand: this helper is
    // shared with add-package/update/cake and moves into a configuration service in the #392 collapse PR.
    internal static Dictionary<string, string> GetConfiguration(AbsolutePath buildScript, bool evaluate)
    {
        string ReplaceScriptDirectory(string value)
            => evaluate
                ? value
                    .Replace("$SCRIPT_DIR", buildScript.Parent)
                    .Replace("$PSScriptRoot", buildScript.Parent)
                : value;

        return File.ReadAllLines(buildScript)
            .SkipWhile(x => !x.StartsWithOrdinalIgnoreCase("# CONFIGURATION"))
            .TakeWhile(x => !x.StartsWithOrdinalIgnoreCase("# EXECUTION"))
            .Where(x => !x.IsNullOrEmpty() && !x.StartsWithAny("#", "export ", "$env:"))
            .Select(ReplaceScriptDirectory)
            .Select(x => x.Split("="))
            .ToDictionary(
                x => x.ElementAt(0).TrimStart("$").Trim().SplitCamelHumpsWithKnownWords().JoinUnderscore().ToUpperInvariant(),
                x => x.ElementAt(1).Trim().TrimMatchingDoubleQuotes());
    }
}

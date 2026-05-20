// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

namespace Fallout.Common.Utilities;

public static partial class StringExtensions
{
    private static readonly string[] KnownWords =
    {
        "DotNet",
        "GitHub",
        "GitVersion",
        "MSBuild",
        "NuGet",
        "ReSharper",
        "AppVeyor",
        "TeamCity",
        "GitLab",
        "SignPath",
        "JetBrains"
    };
}